using Backend;
using DatabaseToSpotifyLibraryImporter;
using Serilog;
using SpotifyAPI.Web;
using Util;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatter: new LogFormatter("DTS"))
    .WriteTo.Trace(formatter: new LogFormatter("DTS"))
    .CreateLogger();

ConnectionManager.CLIENT_ID = "ca53ceb67c7044bf80bb3e5e8760651c";
ConnectionManager.SCOPE = new[]
{
    Scopes.PlaylistReadPrivate,
    Scopes.PlaylistReadCollaborative,
    Scopes.PlaylistModifyPrivate,
    Scopes.PlaylistModifyPublic,
    Scopes.UserFollowModify,
    Scopes.UserFollowRead,
    Scopes.UserLibraryRead,
    Scopes.UserLibraryModify,
    Scopes.UserReadPrivate,
    Scopes.UserReadEmail,
    Scopes.UserReadPlaybackState,
    Scopes.UserModifyPlaybackState,
};

if (!await ConnectionManager.Instance.TryInitFromSavedToken())
    await ConnectionManager.Instance.Login(rememberMe: true);


await SpotifyCleaner.ClearLibrary();
await SpotifyImporter.Import();