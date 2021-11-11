using Backend;
using Serilog;
using System;
using System.IO;

namespace SpotifySongTagger.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private bool rememberMe = true;
        public bool RememberMe
        {
            get => rememberMe;
            set => SetProperty(ref rememberMe, value, nameof(RememberMe));
        }

        public static string FullApplicationName => $"Song Tagger for Spotify v{VersionStr}";
        public static string VersionStr => typeof(HomeViewModel).Assembly.GetName().Version.ToString(3);

        public void ChangeDatabasePath(string selectedPath)
        {
            var oldPath = Settings.DatabasePath;
            var fileName = DataContainer.DbFileName;
            // move database file
            var src = string.Empty;
            var dst = string.Empty;
            try
            {
                src = Path.Combine(oldPath, fileName);
                dst = Path.Combine(selectedPath, fileName);
                File.Move(src, dst);
                Settings.DatabasePath = selectedPath;
                Log.Information($"moved database file ({fileName}) from {src} to {dst}");
                //ConnectionManager.InitDb() // TODO
            }
            catch(Exception e)
            {
                Log.Information($"failed to move database file ({fileName}) from {src} to {dst}: {e.Message}");
            }
        }
    }
}
