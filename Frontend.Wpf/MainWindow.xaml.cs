using Backend;
using Frontend.Wpf.Utils;
using Frontend.Wpf.ViewModels;
using MaterialDesignThemes.Wpf;
using Serilog;
using Serilog.Events;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Util;

namespace Frontend.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel(MainSnackbar.MessageQueue);
            DataContext = ViewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(@"frontend.log")
                .WriteTo.Trace()
                .CreateLogger();
            SetTheme(Settings.Instance.IsDarkTheme);

            // check for update and update
            Action shutdownAction = () =>
            {
                Close();
                Application.Current.Shutdown();
            };
            await UpdateManager.Instance.UpdateToLatestRelease("BenediktAlkin", "UpdaterTest", new Version(1, 0, 0), "Updater", "Application", shutdownAction);
            await ConnectionManager.TryInitFromSavedToken();
            ViewModel.CheckedForUpdates = true;

            if (ConnectionManager.Instance.Spotify != null)
                ((MainWindowViewModel)DataContext).SelectedIndex = 2;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Log.CloseAndFlush();
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //until we had a StaysOpen glag to Drawer, this will help with scroll bars
            //var dependencyObject = Mouse.Captured as DependencyObject;

            //while (dependencyObject != null)
            //{
            //    if (dependencyObject is ScrollBar) return;
            //    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            //}

            MenuToggleButton.IsChecked = false;
        }

        private CancellationTokenSource SyncCancellationTokenSource { get; set; }
        private async void DialogHost_OnDialogOpened(object sender, DialogOpenedEventArgs eventArgs)
        {
            SyncCancellationTokenSource = new CancellationTokenSource();
            try
            {
                await DatabaseOperations.SyncLibrary(SyncCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Log.Information("Cancelled sync library");
                SyncCancellationTokenSource.Dispose();
                SyncCancellationTokenSource = null;
            }


            var dialogHost = sender as DialogHost;
            dialogHost.IsOpen = false;
        }

        private void Cancel_Sync(object sender, RoutedEventArgs e)
        {
            SyncCancellationTokenSource.Cancel();
        }

        private void MenuDarkModeButton_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            var isDarkTheme = toggleButton.IsChecked.Value;

            SetTheme(isDarkTheme);
        }
        private void SetTheme(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);

            PlayerManager.Instance.SetTheme(isDarkTheme);
        }
    }
}