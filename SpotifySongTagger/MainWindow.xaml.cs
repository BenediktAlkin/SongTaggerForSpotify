using Backend;
using MaterialDesignThemes.Wpf;
using Serilog;
using SpotifySongTagger.Utils;
using SpotifySongTagger.ViewModels;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
#if !DEBUG
using Util;
#endif

namespace SpotifySongTagger
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

#if !DEBUG
            // check for update and update
            Action shutdownAction = () =>
            {
                Close();
                Application.Current.Shutdown();
            };
            await UpdateManager.Instance.UpdateToLatestRelease("BenediktAlkin", "SpotifySongTagger", typeof(MainWindow).Assembly.GetName().Version, "Updater", "SpotifySongTagger", shutdownAction);
#endif

            await ConnectionManager.TryInitFromSavedToken();
            ViewModel.CheckedForUpdates = true;
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
            catch (Exception e)
            {
                Log.Error($"Error syncing library {e.Message}");
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
        private static void SetTheme(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);

            PlayerManager.Instance.SetTheme(isDarkTheme);
        }

        private void LoadSelectedView(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => ViewModel.LoadSelectedView();

        private void OpenGithub(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/BenediktAlkin/SpotifySongTagger",
                UseShellExecute = true
            });
        }
        private void OpenDiscord(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/g8xurBu3ST",
                UseShellExecute = true
            });
        }
        private void OpenYoutube(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.youtube.com/playlist?list=PLrFmMNaJAkFA6dUw9Oc3AEkXfdoiELZFn",
                UseShellExecute = true
            });
        }
    }
}