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
            ConnectionManager.ChangeDatabaseFolder(selectedPath);
        }
    }
}
