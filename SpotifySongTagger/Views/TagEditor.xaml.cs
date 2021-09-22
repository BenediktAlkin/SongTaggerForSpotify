using Backend;
using Backend.Entities;
using SpotifySongTagger.Converters;
using SpotifySongTagger.Utils;
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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PlayerManager.OnTrackChanged += ViewModel.UpdatePlayingTrack;
            ViewModel.PlayerManager.OnProgressChanged += ViewModel.SetProgressSpotify;

            Task updatePlaybackInfoTask = null;
            if (!Settings.Instance.HidePlayer)
            {
                ViewModel.PlayerManager.StartUpdateTrackInfoTimer();
                ViewModel.PlayerManager.StartUpdatePlaybackInfoTimer();
                updatePlaybackInfoTask = ViewModel.PlayerManager.UpdatePlaybackInfo();
            }
            await ViewModel.DataContainer.LoadSourcePlaylists();
            Log.Debug("Finished loading playlists");
            await ViewModel.DataContainer.LoadTags();
            Log.Debug("Finished loading tags");
            
            if (updatePlaybackInfoTask != null)
                await updatePlaybackInfoTask;
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PlayerManager.OnTrackChanged -= ViewModel.UpdatePlayingTrack;
            ViewModel.PlayerManager.OnProgressChanged -= ViewModel.SetProgressSpotify;
            ViewModel.PlayerManager.StopUpdateTrackInfoTimer();
            ViewModel.PlayerManager.StopUpdatePlaybackInfoTimer();
        }

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
                Log.Information("Tag_PreviewMouseDown");
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
            ViewModel.AssignTag(trackVM.Track, tag);

            Log.Information($"Drop idx={index} track={trackVM.Track.Name}");
            e.Handled = true;
        }

        private void AssignedTag_DeleteClick(object sender, RoutedEventArgs e)
        {
            var chip = sender as Chip;
            var tag = chip.DataContext as Tag;
            ViewModel.RemoveAssignment(ViewModel.SelectedTrackVM.Track, tag);
        }


        private async void Playlists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var playlist = ViewModel.SelectedPlaylist;
            if (playlist == null)
            {
                ViewModel.TrackVMs.Clear();
                return;
            }
            try
            {
                await ViewModel.LoadTracks(playlist.Id);
            }
            catch (TaskCanceledException)
            {
                Log.Information($"Aborted GetTracks {playlist.Id}");
            }
        }

        public void AddTagDialog_Cancel(object sender, RoutedEventArgs e)
        {
            ViewModel.NewTagName = null;
        }
        private void AddTagDialog_Add(object sender, RoutedEventArgs e)
        {
            ViewModel.AddTag(ViewModel.NewTagName);
            ViewModel.NewTagName = null;
        }
        public void EditTagDialog_Cancel(object sender, RoutedEventArgs e)
        {
            ViewModel.NewTagName = null;
            ViewModel.ClickedTag = null;
        }
        private void EditTagDialog_Save(object sender, RoutedEventArgs e)
        {
            ViewModel.EditTag(ViewModel.ClickedTag, ViewModel.NewTagName);
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
            ViewModel.DeleteTag(ViewModel.ClickedTag);
            ViewModel.NewTagName = null;
            ViewModel.ClickedTag = null;
        }

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

        private void NewTagName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // if validation gives an error for NewTagName, it is not updated in the ViewModel
            var textBox = sender as TextBox;
            ViewModel.NewTagName = textBox.Text;
        }

        private async void Play_Click(object sender, RoutedEventArgs e) => await PlayerManager.Instance.Play();
        private async void Pause_Click(object sender, RoutedEventArgs e) => await PlayerManager.Instance.Pause();


        private async void PlayTrack(object sender, MouseButtonEventArgs e)
        {
            await ViewModel.PlayerManager.SetTrack(ViewModel.SelectedTrackVM.Track);
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
        private async Task SetVolume(object sender)
        {
            var slider = sender as Slider;
            var newVolume = (int)slider.Value;
            await ViewModel.PlayerManager.SetVolume(newVolume);
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
        private async Task SetProgress(object sender)
        {
            var slider = sender as Slider;
            var newProgress = (int)slider.Value;
            await ViewModel.PlayerManager.SetProgress(newProgress);
        }

    }
}
