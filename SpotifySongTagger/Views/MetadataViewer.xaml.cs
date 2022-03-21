using Backend;
using Backend.Entities;
using MaterialDesignThemes.Wpf;
using Serilog;
using SpotifySongTagger.Utils;
using SpotifySongTagger.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static SpotifySongTagger.ViewModels.TagEditorViewModel;

namespace SpotifySongTagger.Views
{
    /// <summary>
    /// Interaction logic for TagEditor.xaml
    /// </summary>
    public partial class MetadataViewer : UserControl
    {
        private MetadataViewerViewModel ViewModel { get; }

        public MetadataViewer(ISnackbarMessageQueue messageQueue)
        {
            InitializeComponent();
            ViewModel = new MetadataViewerViewModel(messageQueue);
            DataContext = ViewModel;
        }

        #region load/unload
        private void UserControl_Loaded(object sender, RoutedEventArgs e) => ViewModel.OnLoaded();
        private void UserControl_Unloaded(object sender, RoutedEventArgs e) => ViewModel.OnUnloaded();
        #endregion


        private async void Playlists_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // clear tracks
            var treeView = sender as TreeView;
            PlaylistOrTag playlistOrTag = null;
            if (treeView.SelectedItem is Playlist playlist)
                playlistOrTag = new PlaylistOrTag(playlist);
            else if (treeView.SelectedItem is Tag tag)
                playlistOrTag = new PlaylistOrTag(tag);
            
            if (treeView.SelectedItem == null || playlistOrTag == null)
            {
                ViewModel.SelectedPlaylistOrTag = null;
                ViewModel.TrackVMs = null;
                return;
            }

            // load new tracks
            ViewModel.SelectedPlaylistOrTag = playlistOrTag;
            await ViewModel.LoadTracks(playlistOrTag);
        }


        #region play/pause
        private async void Play_Click(object sender, RoutedEventArgs e) => await PlayerManager.Instance.Play();
        private async void Pause_Click(object sender, RoutedEventArgs e) => await PlayerManager.Instance.Pause();

        private async void PlayTrack(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.SelectedTrackVM == null) return;
            await BaseViewModel.PlayerManager.SetTrack(ViewModel.SelectedTrackVM.Track);
        }
        #endregion

        #region volume
        private void SetVolume_DragStarted(object sender, DragStartedEventArgs e)
        {
            //Log.Information("volume drag start");
            ViewModel.IsDraggingVolume = true;
        }
        private async void SetVolume_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //Log.Information("volume drag completed");
            await SetVolume(sender);
            ViewModel.IsDraggingVolume = false;
        }
        private async void SetVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // avoid firing an event when the progress bar is updated from spotify
            if (ViewModel.VolumeSource == UpdateSource.Spotify)
            {
                //Log.Information("avoided volume value changed (updated by spotify)");
                return;
            }
            // avoid firing when volume bar is currently dragged
            if (ViewModel.IsDraggingVolume)
            {
                //Log.Information("avoided volume value changed (is dragging slider)");
                return;
            }

            //Log.Information("volume value changed");
            await SetVolume(sender);
        }
        private static async Task SetVolume(object sender)
        {
            var slider = sender as Slider;
            var newVolume = (int)slider.Value;
            await BaseViewModel.PlayerManager.SetVolume(newVolume);
        }
        #endregion

        #region progress
        private void SetProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            //Log.Information("progress drag start");
            ViewModel.IsDraggingProgress = true;
        }
        private async void SetProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //Log.Information("progress drag completed");
            await SetProgress(sender);
            ViewModel.IsDraggingProgress = false;
        }
        private async void SetProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // avoid firing an event when the progress bar is updated from spotify
            if (ViewModel.ProgressSource == UpdateSource.Spotify)
            {
                //Log.Information("avoided volume value changed (updated by spotify)");
                return;
            }
            // avoid firing when progress bar is currently dragged
            if (ViewModel.IsDraggingProgress)
            {
                //Log.Information("avoided volume value changed (is dragging slider)");
                return;
            }

            // this event is fired if the progress bar is set by just clicking somewhere (not dragging it there)
            //Log.Information("progress value changed");
            await SetProgress(sender);
        }
        private static async Task SetProgress(object sender)
        {
            var slider = sender as Slider;
            var newProgress = (int)slider.Value;
            await BaseViewModel.PlayerManager.SetProgress(newProgress);
        }
        #endregion

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.ToString(),
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}
