using Backend;
using SpotifySongTagger.Utils;
using SpotifySongTagger.Views;
using MaterialDesignThemes.Wpf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Util;

namespace SpotifySongTagger.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private bool checkedForUpdates;
        public bool CheckedForUpdates
        {
            get => checkedForUpdates;
            set => SetProperty(ref checkedForUpdates, value, nameof(CheckedForUpdates));
        }

        public List<MenuItem> MenuItems { get; } = new List<MenuItem>
        {
            new MenuItem { Name = "Login", ViewType = typeof(HomeView) },
            new MenuItem { Name = "Song Tagger", ViewType = typeof(TagEditor) },
            new MenuItem { Name = "Playlist Generator", ViewType = typeof(PlaylistGenerator) },
        };
        private MenuItem selectedItem;
        public MenuItem SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value, nameof(SelectedItem));
        }

        private int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set => SetProperty(ref selectedIndex, value, nameof(SelectedIndex));
        }
        private bool isSyncingLibrary = true;
        public bool IsSyncingLibrary
        {
            get => isSyncingLibrary;
            set => SetProperty(ref isSyncingLibrary, value, nameof(IsSyncingLibrary));
        }

        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue) { }


        public CommandImpl SyncLibraryCommand { get; } = new CommandImpl(async sender => 
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

    public class MenuItem : NotifyPropertyChangedBase
    {
        public string Name { get; set; }
        public Type ViewType { get; set; }

        public FrameworkElement View => (FrameworkElement)Activator.CreateInstance(ViewType);

    }
}
