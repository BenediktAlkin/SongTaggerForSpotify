using Backend;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using Serilog;
using SpotifySongTagger.Utils;
using SpotifySongTagger.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SpotifySongTagger.Views
{
    public partial class HomeView : UserControl
    {
        public HomeViewModel ViewModel { get; set; }

        public HomeView(ISnackbarMessageQueue messageQueue)
        {
            InitializeComponent();
            ViewModel = new HomeViewModel(messageQueue);
            DataContext = ViewModel;
        }

        private async void DialogHost_OnDialogOpened(object sender, DialogOpenedEventArgs eventArgs)
        {
            Log.Information("Prompting login");
            try
            {
                await ConnectionManager.Instance.Login(ViewModel.RememberMe);
            }
            catch (Exception e)
            {
                Log.Error($"Error logging in {e.Message}");
            }

            var dialogHost = sender as DialogHost;
            dialogHost.IsOpen = false;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionManager.Instance.Logout();
        }

        private void LoginDialog_Cancel(object sender, RoutedEventArgs e)
        {
            ConnectionManager.Instance.CancelLogin();
        }

        private void Button_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // for some reason at application startup the login button is sometimes disabled
            var button = sender as Button;
            if (!button.IsEnabled)
                button.IsEnabled = true;
        }

        private void ChangeDataFolderButton_Click(object sender, RoutedEventArgs e)
        {
            // dialog from https://github.com/ookii-dialogs/ookii-dialogs-wpf
            var dialog = new VistaFolderBrowserDialog();
            dialog.SelectedPath = ConnectionManager.Instance.DbPath + '\\';
            if (dialog.ShowDialog().Value)
                ViewModel.ChangeDatabasePath(dialog.SelectedPath); 
        }
    }
}
