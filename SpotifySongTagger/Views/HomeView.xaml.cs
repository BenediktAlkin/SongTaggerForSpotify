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
                Log.Error(e.Message);
            }

            var dialogHost = sender as DialogHost;
            dialogHost.IsOpen = false;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Logout");
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
    }
}
