﻿using Backend;
using MaterialDesignThemes.Wpf;
using Serilog;
using SpotifySongTagger.Utils;
using SpotifySongTagger.ViewModels;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Util;

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
            SetTheme(Settings.Instance.IsDarkTheme);

            if (!ViewModel.CheckedForUpdates)
            {
                string os = string.Empty;
                if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    os = "windows";
                if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                    os = "linux"; 
                if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                    os = "mac";
                Log.Information($"checking for updates (OperatingSystem={os})");

#if !DEBUG
                // check for update and update
                Action shutdownAction = () =>
                {
                    Close();
                    Application.Current.Shutdown();
                };
                await UpdateManager.Instance.UpdateToLatestRelease(
                    os, 
                    "BenediktAlkin", 
                    "SongTaggerForSpotify", 
                    typeof(MainWindow).Assembly.GetName().Version, 
                    "Updater", 
                    "SongTaggerForSpotify", 
                    shutdownAction);
#endif
                ViewModel.CheckedForUpdates = true;
                Log.Information("checked for updates");
            }

            ViewModel.IsLoggingIn = true;
            await ConnectionManager.Instance.TryInitFromSavedToken();
            ViewModel.IsLoggingIn = false;
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
            if (SyncCancellationTokenSource != null)
                SyncCancellationTokenSource.Cancel();

            var tokenSource = new CancellationTokenSource();
            SyncCancellationTokenSource = tokenSource;
            try
            {
                await DatabaseOperations.SyncLibrary(tokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Log.Information("Cancelled sync library");
                tokenSource.Dispose();
            }
            catch (OperationCanceledException)
            {
                Log.Information("Cancelled sync library");
                tokenSource.Dispose();
            }
            catch (Exception e)
            {
                Log.Error($"Error syncing library {e.Message}");
            }
            finally
            {
                if (tokenSource != null)
                    tokenSource.Dispose();
            }

            if (SyncCancellationTokenSource == tokenSource)
            {
                var dialogHost = sender as DialogHost;
                dialogHost.IsOpen = false;
                SyncCancellationTokenSource = null;
            }
        }

        private void Cancel_Sync(object sender, RoutedEventArgs e)
        {
            if (SyncCancellationTokenSource != null)
                SyncCancellationTokenSource.Cancel();
        }

        private void MenuDarkModeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var isDarkTheme = checkBox.IsChecked.Value;

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
                FileName = "https://github.com/BenediktAlkin/SongTaggerForSpotify",
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

        private void OpenDonate(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.paypal.com/donate/?hosted_button_id=9RBNSGWNNQ57C",
                UseShellExecute = true
            });
        }
    }
}