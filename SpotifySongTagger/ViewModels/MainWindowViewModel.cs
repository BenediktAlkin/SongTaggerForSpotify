using MaterialDesignThemes.Wpf;
using SpotifySongTagger.Views;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SpotifySongTagger.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ISnackbarMessageQueue MessageQueue { get; set; }
        public MainWindowViewModel(ISnackbarMessageQueue messageQueue)
        {
            SelectedItem = MenuItems[0];
            MessageQueue = messageQueue;
        }



        private bool checkedForUpdates;
        public bool CheckedForUpdates
        {
            get => checkedForUpdates;
            set => SetProperty(ref checkedForUpdates, value, nameof(CheckedForUpdates));
        }

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
            set => SetProperty(ref selectedItem, value, nameof(SelectedItem));
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
