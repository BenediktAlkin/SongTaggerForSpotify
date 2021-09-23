using Backend.Entities;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Util;

namespace Backend
{
    public class PlayerManager : NotifyPropertyChangedBase
    {
        private const string SPOTIFY_ICON_LIGHT = "/Res/Spotify_Icon_RGB_Green.png";
        private const string SPOTIFY_ICON_DARK = "/Res/Spotify_Icon_RGB_White.png";

        public static PlayerManager Instance { get; } = new PlayerManager();
        private PlayerManager() { }

        private static SpotifyClient Spotify => ConnectionManager.Instance.Spotify;

        public void SetTheme(bool isDarkTheme)
        {
            if (!HasAlbumUrl)
                AlbumUrl = isDarkTheme ? SPOTIFY_ICON_DARK : SPOTIFY_ICON_LIGHT;
        }

        public static bool IsPremiumUser => DataContainer.Instance.User != null && DataContainer.Instance.User.Product == "premium";

        public delegate void TrackChangedEventHandler(string newId);
        public event TrackChangedEventHandler OnTrackChanged;
        public delegate void ProgressChangedEventHandler(int newProgress);
        public event ProgressChangedEventHandler OnProgressChanged;

        private bool HasAlbumUrl { get; set; }
        private string albumUrl;
        public string AlbumUrl
        {
            get => albumUrl;
            set => SetProperty(ref albumUrl, value, nameof(AlbumUrl));
        }
        private string trackId;
        public string TrackId
        {
            get => trackId;
            set
            {
                SetProperty(ref trackId, value, nameof(TrackId));
                OnTrackChanged?.Invoke(value);
            }
        }
        private string trackName;
        public string TrackName
        {
            get => trackName;
            set => SetProperty(ref trackName, value, nameof(TrackName));
        }
        private string artistsString;
        public string ArtistsString
        {
            get => artistsString;
            set => SetProperty(ref artistsString, value, nameof(ArtistsString));
        }
        private int progress;
        public int Progress
        {
            get => progress;
            set
            {
                SetProperty(ref progress, value, nameof(Progress));
                OnProgressChanged?.Invoke(value);
            }
        }
        private int progressMax;
        public int ProgressMax
        {
            get => progressMax;
            set => SetProperty(ref progressMax, value, nameof(ProgressMax));
        }
        private int volume;
        public int Volume
        {
            get => volume;
            set => SetProperty(ref volume, value, nameof(Volume));
        }
        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            set => SetProperty(ref isPlaying, value, nameof(IsPlaying));
        }

        private Timer UpdateTrackInfoTimer { get; set; }
        public void StartUpdateTrackInfoTimer()
        {
            if (UpdateTrackInfoTimer != null)
                UpdateTrackInfoTimer.Enabled = false;

            UpdateTrackInfoTimer = new Timer
            {
                AutoReset = true,
                Interval = 1000,
            };
            UpdateTrackInfoTimer.Elapsed += async (sender, e) => await UpdateTrackInfo();
            UpdateTrackInfoTimer.Enabled = true;
        }
        public void StopUpdateTrackInfoTimer() => UpdateTrackInfoTimer.Enabled = false;
        private Timer UpdatePlaybackInfoTimer { get; set; }
        public void StartUpdatePlaybackInfoTimer()
        {
            if (UpdatePlaybackInfoTimer != null)
                UpdatePlaybackInfoTimer.Enabled = false;

            UpdatePlaybackInfoTimer = new Timer
            {
                AutoReset = true,
                Interval = 5000,
            };
            UpdatePlaybackInfoTimer.Elapsed += async (sender, e) => await UpdatePlaybackInfo();
            UpdatePlaybackInfoTimer.Enabled = true;
        }
        public void StopUpdatePlaybackInfoTimer() => UpdatePlaybackInfoTimer.Enabled = false;

        private void UpdateTrackInfo(FullTrack track)
        {
            if (Spotify == null) return;
            var bestImg = track.Album.Images.OrderBy(img => img.Height).FirstOrDefault(img => img.Height >= 72 && img.Width >= 72);
            if (bestImg == null)
                bestImg = track.Album.Images.First();
            AlbumUrl = bestImg.Url;
            HasAlbumUrl = true;
            TrackId = track.Id;
            TrackName = track.Name;
            ArtistsString = string.Join(", ", track.Artists.Select(a => a.Name));
            ProgressMax = track.DurationMs;
        }
        public async Task UpdatePlaybackInfo()
        {
            if (Spotify == null) return;
            var info = await Spotify.Player.GetCurrentPlayback(new PlayerCurrentPlaybackRequest());
            if (info == null) return;

            Volume = info.Device.VolumePercent ?? 0;
        }
        public async Task UpdateTrackInfo()
        {
            if (Spotify == null) return;
            var info = await Spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
            if (info == null) return;
            IsPlaying = info.IsPlaying;
            Progress = info.ProgressMs ?? 0;

            if (info.Item is FullTrack track)
                UpdateTrackInfo(track);
        }
        public async Task Play()
        {
            if (!IsPremiumUser) return;
            if (Spotify == null) return;
            if (IsPlaying) return;
            try
            {
                var success = await Spotify.Player.ResumePlayback();
                if (success)
                    IsPlaying = true;
            }
            catch (Exception e)
            {
                Log.Error($"Error in PlayerManager.Play {e.Message}");
            }

        }
        public async Task Pause()
        {
            if (!IsPremiumUser) return;
            if (Spotify == null) return;
            if (!IsPlaying) return;
            try
            {
                var success = await Spotify.Player.PausePlayback();
                if (success)
                    IsPlaying = false;
            }
            catch (Exception e)
            {
                Log.Error($"Error in PlayerManager.Pause {e.Message}");
            }
        }
        public async Task SetVolume(int newVolume)
        {
            if (!IsPremiumUser) return;
            if (Spotify == null) return;
            var success = await Spotify.Player.SetVolume(new PlayerVolumeRequest(newVolume));
            if (success)
                Volume = newVolume;
        }
        public async Task SetProgress(int newProgress)
        {
            if (!IsPremiumUser) return;
            if (Spotify == null) return;
            var success = await Spotify.Player.SeekTo(new PlayerSeekToRequest(newProgress));
            if (success)
                Progress = newProgress;
        }
        public async Task SetTrack(Track t)
        {
            if (!IsPremiumUser) return;
            if (Spotify == null) return;
            var success = await Spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest { ContextUri = $"spotify:track:{t.Id}" });
            if (success)
                await UpdateTrackInfo();
        }
    }
}
