using Backend;
using Backend.Entities;
using SpotifySongTagger.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            }catch(Exception e)
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
            if(!button.IsEnabled)
                button.IsEnabled = true;
        }
    }
}
