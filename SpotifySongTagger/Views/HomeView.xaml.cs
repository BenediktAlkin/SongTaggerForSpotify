using Backend;
using MaterialDesignThemes.Wpf;
using Serilog;
using SpotifySongTagger.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SpotifySongTagger.Views
{
    public partial class HomeView : UserControl
    {
        public HomeViewModel ViewModel { get; set; }

        public HomeView()
        {
            InitializeComponent();
            ViewModel = new HomeViewModel();
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

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "";
            dialog.DefaultExt = "";
            dialog.Filter = $"SpotifySongTagger export (*.json)|*.json";
            dialog.OverwritePrompt = false;

            var result = dialog.ShowDialog();
            if (result == true)
                await DatabaseOperations.ImportTags(dialog.FileName);
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "tags";
            dialog.DefaultExt = ".json";
            dialog.Filter = $"SpotifySongTagger export (*.json)|*.json";

            var result = dialog.ShowDialog();
            if (result == true)
                await DatabaseOperations.ExportTags(dialog.FileName);
        }
    }
}
