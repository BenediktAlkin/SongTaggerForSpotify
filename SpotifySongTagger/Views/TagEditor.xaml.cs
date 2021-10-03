using Backend;
using Backend.Entities;
using MaterialDesignThemes.Wpf;
using Serilog;
using SpotifySongTagger.Utils;
using SpotifySongTagger.ViewModels;
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
    public partial class TagEditor : UserControl
    {
        private TagEditorViewModel ViewModel { get; }

        public TagEditor()
        {
            InitializeComponent();
            ViewModel = new TagEditorViewModel();
            DataContext = ViewModel;
        }
        #region load/unload
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            BaseViewModel.PlayerManager.OnTrackChanged += ViewModel.UpdatePlayingTrack;
            BaseViewModel.PlayerManager.OnProgressChanged += ViewModel.SetProgressSpotify;

            Task updatePlaybackInfoTask = null;
            if (!Settings.Instance.HidePlayer)
            {
                BaseViewModel.PlayerManager.StartUpdateTrackInfoTimer();
                BaseViewModel.PlayerManager.StartUpdatePlaybackInfoTimer();
                updatePlaybackInfoTask = BaseViewModel.PlayerManager.UpdatePlaybackInfo();
            }
            await BaseViewModel.DataContainer.LoadSourcePlaylists();
            BaseViewModel.DataContainer.LoadGeneratedPlaylists();
            ViewModel.IsLoadingPlaylists = false;
            await BaseViewModel.DataContainer.LoadTags();

            if (updatePlaybackInfoTask != null)
                await updatePlaybackInfoTask;
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            BaseViewModel.PlayerManager.OnTrackChanged -= ViewModel.UpdatePlayingTrack;
            BaseViewModel.PlayerManager.OnProgressChanged -= ViewModel.SetProgressSpotify;
            BaseViewModel.PlayerManager.StopUpdateTrackInfoTimer();
            BaseViewModel.PlayerManager.StopUpdatePlaybackInfoTimer();
        }
        #endregion


        #region tag drag & drop
        private void Tag_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var chip = sender as Chip;
            // DragDrop.DoDragDrop leads to no further events (e.g. on the delete button of the tag)
            if (TagEditOrDeleteIsHovered && e.LeftButton == MouseButtonState.Pressed)
            {

            }
            else if (chip != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var tag = chip.DataContext as Tag;
                DragDrop.DoDragDrop(chip, tag.Name, DragDropEffects.Link);
            }
        }
        private void Tracks_Drop(object sender, DragEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid.Items.Count == 0) return;

            var index = UIHelper.GetDataGridRowIndex(dataGrid, e);

            // assign tag to track
            var tag = e.Data.GetData(DataFormats.StringFormat) as string;
            var trackVM = (TrackViewModel)dataGrid.Items.GetItemAt(index);
            AssignTag(trackVM.Track, tag);
            Log.Information($"Assigned {tag} to {trackVM.Track.Name}");
            e.Handled = true;
        }
        #endregion

        private void AssignedTag_DeleteClick(object sender, RoutedEventArgs e)
        {
            var chip = sender as Chip;
            var tag = chip.DataContext as Tag;
            ViewModel.RemoveAssignment(tag);
        }


        private async void Playlists_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // clear tracks
            var treeView = sender as TreeView;
            var playlist = treeView.SelectedItem as Playlist;
            if (treeView.SelectedItem == null || playlist == null)
            {
                ViewModel.SelectedPlaylist = null;
                ViewModel.TrackVMs.Clear();
                return;
            }

            // load new tracks
            ViewModel.SelectedPlaylist = playlist;
            try
            {
                await ViewModel.LoadTracks(playlist);
                Log.Information($"Selected playlist {playlist.Name} with {ViewModel.TrackVMs.Count} songs");
            }
            catch (TaskCanceledException)
            {
                Log.Information($"Aborted GetTracks {playlist.Id}");
            }
        }

        #region add/edit/delete tag dialog
        public void AddTagDialog_Cancel(object sender, RoutedEventArgs e)
        {
            ViewModel.NewTagName = null;
        }
        private void AddTagDialog_Add(object sender, RoutedEventArgs e)
        {
            ViewModel.AddTag();
            ViewModel.NewTagName = null;
        }
        public void EditTagDialog_Cancel(object sender, RoutedEventArgs e)
        {
            ViewModel.NewTagName = null;
            ViewModel.ClickedTag = null;
        }
        private void EditTagDialog_Save(object sender, RoutedEventArgs e)
        {
            ViewModel.EditTag();
            ViewModel.NewTagName = null;
            ViewModel.ClickedTag = null;
        }
        public void DeleteTagDialog_Cancel(object sender, RoutedEventArgs e)
        {
            ViewModel.NewTagName = null;
            ViewModel.ClickedTag = null;
        }
        private void DeleteTagDialog_Delete(object sender, RoutedEventArgs e)
        {
            ViewModel.DeleteTag();
            ViewModel.NewTagName = null;
            ViewModel.ClickedTag = null;
        }


        private void NewTagName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // if validation gives an error for NewTagName, it is not updated in the ViewModel
            var textBox = sender as TextBox;
            ViewModel.NewTagName = textBox.Text;
        }
        #endregion

        #region tag edit/delete button behaviour
        private void ToggleDeleteMode(object sender, RoutedEventArgs e)
        {

            ViewModel.IsTagEditMode = false;
            ViewModel.IsTagDeleteMode = !ViewModel.IsTagDeleteMode;
        }
        private void ToggleEditMode(object sender, RoutedEventArgs e)
        {
            ViewModel.IsTagDeleteMode = false;
            ViewModel.IsTagEditMode = !ViewModel.IsTagEditMode;
        }

        private bool TagEditOrDeleteIsHovered { get; set; }
        private void TagEditOrDelete_MouseEnter(object sender, MouseEventArgs e) => TagEditOrDeleteIsHovered = true;
        private void TagEditOrDelete_MouseLeave(object sender, MouseEventArgs e) => TagEditOrDeleteIsHovered = false;

        private void EditOrDeleteTagButton_Click(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            ViewModel.ClickedTag = frameworkElement.DataContext as Tag;
            ViewModel.NewTagName = ViewModel.ClickedTag.Name;
        }
        #endregion

        #region player
        private async void Play_Click(object sender, RoutedEventArgs e) => await PlayerManager.Instance.Play();
        private async void Pause_Click(object sender, RoutedEventArgs e) => await PlayerManager.Instance.Pause();

        private async void PlayTrack(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.SelectedTrackVM == null) return;
            await BaseViewModel.PlayerManager.SetTrack(ViewModel.SelectedTrackVM.Track);
        }

        private void DisableVolumeUpdates(object sender, DragStartedEventArgs e) => ViewModel.DisableVolumeUpdates = true;
        private async void SetVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ViewModel.DisableVolumeUpdates) return;
            await SetVolume(sender);
        }
        private async void SetVolume_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            await SetVolume(sender);
            ViewModel.DisableVolumeUpdates = false;
        }
        private static async Task SetVolume(object sender)
        {
            var slider = sender as Slider;
            var newVolume = (int)slider.Value;
            await BaseViewModel.PlayerManager.SetVolume(newVolume);
        }

        private void DisableProgressUpdates(object sender, DragStartedEventArgs e)
        {
            ViewModel.DisableSpotifyProgressUpdates = true;
        }
        private async void SetProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            await SetProgress(sender);
            ViewModel.DisableSpotifyProgressUpdates = false;
        }
        private async void SetProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ViewModel.ProgressSource == ProgressUpdateSource.Spotify) return;
            await SetProgress(sender);
        }
        private static async Task SetProgress(object sender)
        {
            var slider = sender as Slider;
            var newProgress = (int)slider.Value;
            await BaseViewModel.PlayerManager.SetProgress(newProgress);
        }
        #endregion
    }
}
