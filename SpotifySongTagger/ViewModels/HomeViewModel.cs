using Backend;
using MaterialDesignThemes.Wpf;
using Serilog;
using System;
using System.IO;
using System.Windows.Controls;
using static Backend.ConnectionManager;

namespace SpotifySongTagger.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private ISnackbarMessageQueue MessageQueue { get; set; }
        public HomeViewModel(ISnackbarMessageQueue messageQueue)
        {
            MessageQueue = messageQueue;
        }


        private bool rememberMe = true;
        public bool RememberMe
        {
            get => rememberMe;
            set => SetProperty(ref rememberMe, value, nameof(RememberMe));
        }

        public static string FullApplicationName => $"Song Tagger for Spotify v{VersionStr}";
        public static string VersionStr => typeof(HomeViewModel).Assembly.GetName().Version.ToString(3);

        public bool ChangeDatabasePathOverwritesFile()
        {
            if (DataContainer.User == null) return false;

            return true;
        }

        public void ChangeDatabasePath(string selectedPath)
        {
            var result = ConnectionManager.ChangeDatabaseFolder(selectedPath);
            if (result == ChangeDatabaseFolderResult.CopiedToNewFolder)
                MessageQueue.Enqueue("Copied current database file to new folder");
            else if(result == ChangeDatabaseFolderResult.UseExistingDbInNewFolder)
            {
                var msg = $"Found existing database file in new folder{Environment.NewLine}From now on the existing database file will be used";
                var duration = TimeSpan.FromSeconds(10);
                MessageQueue.Enqueue(msg, null, null, null, false, true, duration);
            }
        }
    }
}
