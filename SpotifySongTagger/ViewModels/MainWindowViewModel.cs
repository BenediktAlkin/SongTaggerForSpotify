using Backend;
using MaterialDesignThemes.Wpf;
using SpotifySongTagger.Utils;
using SpotifySongTagger.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Util;

namespace SpotifySongTagger.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            SelectedItem = MenuItems[0];
        }



        private bool checkedForUpdates;
        public bool CheckedForUpdates
        {
            get => checkedForUpdates;
            set => SetProperty(ref checkedForUpdates, value, nameof(CheckedForUpdates));
        }

        public List<MenuItem> MenuItems { get; } = new()
        {
            new MenuItem("Login", typeof(HomeView), false),
            new MenuItem("Song Tagger", typeof(TagEditor), true),
            new MenuItem("Playlist Generator", typeof(PlaylistGenerator), false),
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
        public void LoadSelectedView() => View = (FrameworkElement)Activator.CreateInstance(SelectedItem.ViewType);


        private bool isSyncingLibrary = true;
        public bool IsSyncingLibrary
        {
            get => isSyncingLibrary;
            set => SetProperty(ref isSyncingLibrary, value, nameof(IsSyncingLibrary));
        }

        public CommandImpl SyncLibraryCommand { get; } = new(async sender =>
        {
            var viewModel = sender as MainWindowViewModel;
            await viewModel.SyncLibrary();
        });
        public async Task SyncLibrary()
        {
            IsSyncingLibrary = true;
            await DatabaseOperations.SyncLibrary();
            IsSyncingLibrary = false;
        }
    }

    public record MenuItem(string Name, Type ViewType, bool CanSyncLibrary);
}
