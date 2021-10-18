using MaterialDesignThemes.Wpf;
using Serilog;
using SpotifySongTagger.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using Util;

namespace SpotifySongTagger.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public static UpdateManager UpdateManager => UpdateManager.Instance;

        private ISnackbarMessageQueue MessageQueue { get; set; }
        public MainWindowViewModel(ISnackbarMessageQueue messageQueue)
        {
            SelectedItem = MenuItems[0];
            MessageQueue = messageQueue;
            UpdateManager.Instance.OnUpdatingStateChanged += newState => NotifyPropertyChanged(nameof(ShowSpinner));
        }

        private bool isLoggingIn;
        public bool IsLoggingIn
        {
            get => isLoggingIn;
            set
            {
                SetProperty(ref isLoggingIn, value, nameof(IsLoggingIn));
                NotifyPropertyChanged(nameof(ShowSpinner));
                NotifyPropertyChanged(nameof(ShowContent));
            }
        }

        private bool checkedForUpdates = !Settings.AutoUpdate;
        public bool CheckedForUpdates
        {
            get => checkedForUpdates;
            set
            {
                SetProperty(ref checkedForUpdates, value, nameof(CheckedForUpdates));
                NotifyPropertyChanged(nameof(ShowSpinner));
                NotifyPropertyChanged(nameof(ShowContent));
            }
        }
        public bool ShowSpinner => IsLoggingIn || (!CheckedForUpdates && !UpdateManager.Instance.IsDownloading);
        public bool ShowContent => !IsLoggingIn && CheckedForUpdates;

        public List<MenuItem> MenuItems { get; } = new()
        {
            new MenuItem("Login", typeof(HomeView), true),
            new MenuItem("Song Tagger", typeof(TagEditor), true),
            new MenuItem("Playlist Generator", typeof(PlaylistGenerator), true),
        };
        private MenuItem selectedItem;
        public MenuItem SelectedItem
        {
            get => selectedItem;
            set
            {
                SetProperty(ref selectedItem, value, nameof(SelectedItem));
                Log.Information($"change view to {value.Name}");
            }
        }
        private FrameworkElement view;
        public FrameworkElement View
        {
            get => view;
            set => SetProperty(ref view, value, nameof(View));
        }
        public void LoadSelectedView()
        {
            if (SelectedItem.ViewType.GetConstructor(Type.EmptyTypes) != null)
            {
                // view has parameterless constructor
                View = (FrameworkElement)Activator.CreateInstance(SelectedItem.ViewType);
            }
            else
            {
                // view requires messageQueue
                View = (FrameworkElement)Activator.CreateInstance(SelectedItem.ViewType, new[] { MessageQueue });
            }
        }
    }

    public record MenuItem(string Name, Type ViewType, bool CanSyncLibrary);
}
