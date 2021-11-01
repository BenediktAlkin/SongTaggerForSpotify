using Microsoft.EntityFrameworkCore;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class ConnectionManager
    {
        private const string CLIENT_ID = "c15508ab1a5f453396e3da29d16a506b";
        private const string SERVER_URL_TEMPLATE = "http://localhost:{0}/";
        private static readonly string CALLBACK_URL_TEMPLATE = "{0}callback/";

        private const string SST_TOKEN_FILE = "token.txt";
        private const int SST_PORT = 63846;
        // use different port for API to avoid conflicts when running API and songtagger concurrently
        private const string API_TOKEN_FILE = "token_api.txt";
        private const int API_PORT = 63847;


        public bool IsApi { get; set; }
        private string TOKEN_FILE => IsApi ? API_TOKEN_FILE : SST_TOKEN_FILE;
        private int PORT => IsApi ? API_PORT : SST_PORT;
        private string SERVER_URL => string.Format(SERVER_URL_TEMPLATE, API_PORT);
        private string CALLBACK_URL => string.Format(CALLBACK_URL_TEMPLATE, SERVER_URL);


        protected static ILogger Logger { get; } = Log.ForContext("SourceContext", "CM");
        public static ConnectionManager Instance { get; } = new();
        private ConnectionManager() { }

        #region Database
        public static DbContextOptionsBuilder<DatabaseContext> GetOptionsBuilder(string dbName, Action<string> logTo = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>().UseSqlite($"Data Source={dbName}.sqlite");
            if (logTo != null)
                optionsBuilder.LogTo(logTo, minimumLevel: Microsoft.Extensions.Logging.LogLevel.Information);
            //optionsBuilder.LogTo(Logger.Information, minimumLevel: Microsoft.Extensions.Logging.LogLevel.Information);
            //optionsBuilder.EnableSensitiveDataLogging();
            return optionsBuilder;
        }
        public static void InitDb(string dbName, Action<string> logTo = null)
        {
            OptionsBuilder = GetOptionsBuilder(dbName, logTo);
            // create database if it doesn't exist yet
            Logger.Information("initializing database");
            using var _ = ConnectionManager.NewContext(ensureCreated: true);
            Logger.Information("initialized database");
        }

        private static DbContextOptionsBuilder<DatabaseContext> OptionsBuilder { get; set; }
        public static DatabaseContext NewContext(bool ensureCreated = false, bool dropDb = false)
        {
            try
            {
                return new DatabaseContext(OptionsBuilder.Options, ensureCreated: ensureCreated, dropDb: dropDb);
            }
            catch (Exception e)
            {
                Logger.Error($"failed to connect to database {e.Message}");
                throw;
            }
        }
        #endregion


        #region Spotify
        public ISpotifyClient Spotify { get; private set; }


        // option to set spotifyClient from outside (mostly for testing with mocked spotify client)
        public void InitSpotify(ISpotifyClient spotifyClient) => Instance.Spotify = spotifyClient;
        public async Task<bool> TryInitFromSavedToken()
        {
            var tokenData = GetSavedToken();
            return await InitSpotify(tokenData);
        }
        private static PKCETokenResponse GetTokenFromString(string[] tokenData)
        {
            return new PKCETokenResponse
            {
                AccessToken = tokenData[0],
                RefreshToken = tokenData[1],
                TokenType = tokenData[2],
                ExpiresIn = int.Parse(tokenData[3]),
                Scope = tokenData[4],
                CreatedAt = new DateTime(long.Parse(tokenData[5])),
            };
        }
        public PKCETokenResponse GetSavedToken()
        {
            if (!File.Exists(TOKEN_FILE))
            {
                Logger.Information($"tokenfile not found ({TOKEN_FILE})");
                return null;
            }

            var tokenStr = File.ReadAllText(TOKEN_FILE);
            // remove \r to make token data independent of OS
            tokenStr = tokenStr.Replace("\r", "");
            var tokenData = tokenStr.Split('\n');
            var token = GetTokenFromString(tokenData);
            Logger.Information($"got token from {TOKEN_FILE}");
            return token;
        }
        private void SaveToken(PKCETokenResponse tokenData)
        {
            var tokenStr = string.Join(Environment.NewLine, new[]
            {
                tokenData.AccessToken,
                tokenData.RefreshToken,
                tokenData.TokenType,
                $"{tokenData.ExpiresIn}",
                tokenData.Scope,
                $"{tokenData.CreatedAt.Ticks}",
            });
            try
            {
                File.WriteAllText(TOKEN_FILE, tokenStr);
                Logger.Information("saved token to file");
            }
            catch (Exception)
            {
                Logger.Warning("failed to save token to file");
            }
        }

        private ISpotifyClient CreateSpotifyClient(PKCETokenResponse tokenData)
        {
            if (tokenData == null) return null;

            var authenticator = new PKCEAuthenticator(CLIENT_ID, tokenData);
            authenticator.TokenRefreshed += (_, token) =>
            {
                Logger.Information("TokenRefreshed");
                SaveToken(token);
            };
            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(authenticator)
                .WithRetryHandler(new SimpleRetryHandler());
            return new SpotifyClient(config);
        }
        public async Task<bool> InitSpotify(PKCETokenResponse tokenData)
        {
            var client = CreateSpotifyClient(tokenData);
            if (client == null) return false;
            try
            {
                DataContainer.Instance.User = await client.UserProfile.Current();
                Instance.Spotify = client;
                Logger.Information($"connected to spotify with user {DataContainer.Instance.User.DisplayName} ({DataContainer.Instance.User.Id})");
                InitDb(DataContainer.Instance.User.Id);
            }
            catch (Exception e)
            {
                Logger.Information($"Failed to initialize spotify client {e.Message}");
                return false;
            }
            return true;
        }
        #endregion


        #region Spotify login server
        private HttpListener Server { get; set; }
        public void Logout()
        {
            Log.Information("logging out");
            if (File.Exists(TOKEN_FILE))
            {
                try
                {
                    File.Delete(TOKEN_FILE);
                }
                catch (Exception)
                {

                }
            }

            DataContainer.Instance.Clear();
            Instance.Spotify = null;
            Log.Information("logged out");
        }
        public void CancelLogin()
        {
            Server.Stop();
            Server = null;
        }
        private (string, LoginRequest) CreateLoginRequest()
        {
            // create code
            var (verifier, challenge) = PKCEUtil.GenerateCodes();

            // create login request
            var loginRequest = new LoginRequest(
              new Uri(CALLBACK_URL),
              CLIENT_ID,
              LoginRequest.ResponseType.Code)
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[]
                {
                        Scopes.PlaylistReadPrivate,
                        Scopes.PlaylistReadCollaborative,
                        Scopes.PlaylistModifyPrivate,
                        Scopes.PlaylistModifyPublic,
                        Scopes.UserLibraryRead,
                        Scopes.UserReadPrivate,
                        Scopes.UserReadEmail,
                        Scopes.UserReadPlaybackState,
                        Scopes.UserModifyPlaybackState,
                    }
            };
            return (verifier, loginRequest);
        }
        public async Task Login(bool rememberMe)
        {
            // stop server if it is running
            if (Server != null)
            {
                try
                {
                    Server.Stop();
                }
                catch (Exception e)
                {
                    Logger.Information($"Failed to stop server {e.Message}");
                }
            }

            // start server
            Server = new HttpListener();
            Server.Prefixes.Add(SERVER_URL);
            Server.Start();
            Logger.Information($"Listening for connections on {PORT}");

            var (verifier, loginRequest) = CreateLoginRequest();

            // start browser to authenticate
            var uri = loginRequest.ToUri();
            Process.Start(new ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            });

            // listen for request
            Logger.Information("opened login url in browser --> waiting for token");
            HttpListenerContext ctx;
            try
            {
                ctx = await Server.GetContextAsync();
            }
            catch (Exception)
            {
                Logger.Information("failed to get login response");
                return;
            }

            // extract token
            var code = ctx.Request.Url.ToString().Replace($"{CALLBACK_URL}?code=", "");
            Logger.Information("got token");

            // create spotify client
            var tokenRequest = new PKCETokenRequest(CLIENT_ID, code, new Uri(CALLBACK_URL), verifier);
            PKCETokenResponse tokenData;
            bool tokenIsValid = false;
            try
            {
                tokenData = await new OAuthClient().RequestToken(tokenRequest);
                tokenIsValid = await InitSpotify(tokenData);
                if (tokenIsValid && rememberMe)
                    SaveToken(tokenData);
            }
            catch(Exception e)
            {
                Logger.Information($"login token is invalid: {e.Message}");
            }

            // write response
            var response = ctx.Response;
            var successStr = tokenIsValid ? "" : "not ";
            byte[] html = Encoding.UTF8.GetBytes($"<html><center><h1>Authentication was {successStr}successful</h1><br/><h3>Song Tagger for Spotify is now usable!</h3></center></html>");
            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = html.LongLength;
            await response.OutputStream.WriteAsync(html.AsMemory(0, html.Length));
            Server.Close();

            // stop server
            Server.Close();
            Server = null;
        }
        #endregion
    }
}
