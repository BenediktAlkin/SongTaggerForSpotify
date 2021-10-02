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
        private const string TOKEN_FILE = "token.txt";
        private const string CLIENT_ID = "c15508ab1a5f453396e3da29d16a506b";
        private const int PORT = 63846;
        private static readonly string SERVER_URL = $"http://localhost:{PORT}/";
        private static readonly string CALLBACK_URL = $"{SERVER_URL}callback/";

        public static ConnectionManager Instance { get; } = new();
        private ConnectionManager() { }


        private HttpListener Server { get; set; }

        public SpotifyClient Spotify { get; private set; }
        public DatabaseContext Database { get; private set; }

        public static void InitDb(string dbName, bool dropDb = false, Action<string> logTo = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>().UseSqlite($"Data Source={dbName}.sqlite");
            if (logTo != null)
                optionsBuilder.LogTo(logTo, minimumLevel: Microsoft.Extensions.Logging.LogLevel.Information);
            try
            {
                Instance.Database = new DatabaseContext(optionsBuilder.Options, dropDb);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to initialize Database {e.Message}");
                throw;
            }
        }
        public static async Task TryInitFromSavedToken()
        {
            var tokenData = GetSavedToken();
            await InitSpotify(tokenData);
        }

        private static PKCETokenResponse GetSavedToken()
        {
            if (!File.Exists(TOKEN_FILE)) return null;

            var tokenData = File.ReadAllText(TOKEN_FILE).Split('\n');
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
        private static void SaveToken(PKCETokenResponse tokenData)
        {
            var tokenStr = string.Join('\n', new[]
            {
                tokenData.AccessToken,
                tokenData.RefreshToken,
                tokenData.TokenType,
                $"{tokenData.ExpiresIn}",
                tokenData.Scope,
                $"{tokenData.CreatedAt.Ticks}",
            });
            File.WriteAllText(TOKEN_FILE, tokenStr);
        }

        private static SpotifyClient CreateSpotifyClient(PKCETokenResponse tokenData)
        {
            if (tokenData == null) return null;

            var authenticator = new PKCEAuthenticator(CLIENT_ID, tokenData);
            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(authenticator)
                .WithRetryHandler(new SimpleRetryHandler());
            return new SpotifyClient(config);
        }

        private static async Task<bool> InitSpotify(PKCETokenResponse tokenData)
        {
            var client = CreateSpotifyClient(tokenData);
            if (client == null) return false;
            try
            {
                DataContainer.Instance.User = await client.UserProfile.Current();
                Instance.Spotify = client;
                InitDb(DataContainer.Instance.User.Id);
            }
            catch (Exception e)
            {
                Log.Information($"Failed to initialize spotify client {e.Message}");
                return false;
            }
            return true;
        }

        public static void Logout()
        {
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
        }
        public void CancelLogin()
        {
            Server.Stop();
            Server = null;
        }
        public async Task Login(bool rememberMe)
        {
            // start server
            if (Server != null)
                Server.Stop();
            Server = new HttpListener();
            Server.Prefixes.Add(SERVER_URL);
            Server.Start();
            Log.Information($"Listening for connections on {PORT}");

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

            // start browser to authenticate
            var uri = loginRequest.ToUri();
            Process.Start(new ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            });

            // listen for request
            Log.Information("opened login url in browser --> waiting for token");
            HttpListenerContext ctx;
            try
            {
                ctx = await Server.GetContextAsync();
            }
            catch (Exception)
            {
                Log.Information("failed to get login response");
                return;
            }

            // extract token
            var code = ctx.Request.Url.ToString().Replace($"{CALLBACK_URL}?code=", "");
            Log.Information("got token");

            // create spotify client
            var tokenRequest = new PKCETokenRequest(CLIENT_ID, code, new Uri(CALLBACK_URL), verifier);
            var tokenData = await new OAuthClient().RequestToken(tokenRequest);
            var tokenIsValid = await InitSpotify(tokenData);
            if (tokenIsValid && rememberMe)
                SaveToken(tokenData);

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
    }
}
