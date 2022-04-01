using Backend;
using MaterialDesignThemes.Wpf;
using SpotifySongTagger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifySongTagger.ViewModels
{
    public class SpotifyPlayerViewModel : BaseViewModel
    {
        protected ISnackbarMessageQueue MessageQueue { get; set; }
        private List<TrackViewModel> trackVMs;
        public List<TrackViewModel> TrackVMs
        {
            get => trackVMs;
            set => SetProperty(ref trackVMs, value, nameof(TrackVMs));
        }
        public SpotifyPlayerViewModel(ISnackbarMessageQueue messageQueue)
        {
            MessageQueue = messageQueue;
        }

        public virtual void OnLoaded()
        {
            // register PlayerManager error handling
            BaseViewModel.PlayerManager.OnPlayerError += OnPlayerError;

            // register player updates
            BaseViewModel.PlayerManager.OnTrackChanged += UpdatePlayingTrack;
            BaseViewModel.PlayerManager.OnProgressChanged += SetProgressSpotify;
            BaseViewModel.PlayerManager.OnVolumeChanged += SetVolumeSpotify;
        }
        public virtual void OnUnloaded()
        {
            // unregister PlayerManager error handling
            BaseViewModel.PlayerManager.OnPlayerError -= OnPlayerError;

            // unregister player updates
            BaseViewModel.PlayerManager.OnTrackChanged -= UpdatePlayingTrack;
            BaseViewModel.PlayerManager.OnProgressChanged -= SetProgressSpotify;
            BaseViewModel.PlayerManager.OnVolumeChanged -= SetVolumeSpotify;
        }

        private void OnPlayerError(PlayerManager.PlayerError error)
        {
            object msg;
            switch (error)
            {
                case PlayerManager.PlayerError.RequiresSpotifyPremium:
                    msg = UIComposer.ComposeRequiresSpotifyPremiumLink();
                    break;
                case PlayerManager.PlayerError.RequiresOpenSpotifyPlayer:
                    msg = UIComposer.ComposeRequiresOpenSpotifyPlayerLink();
                    break;
                default:
                    msg = "Unknown Error from Spotify Player";
                    break;
            }
            MessageQueue.Enqueue(msg);
        }
        private void UpdatePlayingTrack(string newId)
        {
            if (TrackVMs == null) return;
            foreach (var trackVM in TrackVMs)
                trackVM.IsPlaying = trackVM.Track.Id == newId;
        }

        #region Player volume
        public bool IsDraggingVolume { get; set; }
        private void SetVolumeSpotify(int newVolume) => SetVolume(newVolume, UpdateSource.Spotify);
        public void SetVolume(int newVolume, UpdateSource source)
        {
            if (IsDraggingVolume && source == UpdateSource.Spotify) return;

            volume = newVolume;
            VolumeSource = source;
            NotifyPropertyChanged(nameof(Volume));
        }
        public UpdateSource VolumeSource { get; private set; }
        private int volume;
        public int Volume
        {
            get => volume;
            set => SetVolume(value, UpdateSource.User);
        }
        #endregion


        #region Player progress
        public bool IsDraggingProgress { get; set; }
        private void SetProgressSpotify(int newProgress) => SetProgress(newProgress, UpdateSource.Spotify);
        public void SetProgress(int newProgress, UpdateSource source)
        {
            if (IsDraggingProgress && source == UpdateSource.Spotify) return;

            progress = newProgress;
            ProgressSource = source;
            NotifyPropertyChanged(nameof(Progress));
        }
        public UpdateSource ProgressSource { get; private set; }
        private int progress;
        public int Progress
        {
            get => progress;
            set => SetProgress(value, UpdateSource.User);
        }
        #endregion
    }
}
