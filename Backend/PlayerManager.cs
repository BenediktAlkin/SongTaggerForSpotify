using Backend.Entities;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
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
        private static ILogger logger;
        protected static ILogger Logger
        {
            get
            {
                if (logger == null)
                    logger = Log.ForContext("SourceContext", "PM");
                return logger;
            }
        }

        public static PlayerManager Instance { get; } = new();
        private PlayerManager() { }

        private static ISpotifyClient Spotify => ConnectionManager.Instance.Spotify;

        public enum PlayerError
        {
            RequiresSpotifyPremium,
            RequiresOpenSpotifyPlayer,
        }

        public delegate void PlayerErrorEventHandler(PlayerError error);
        public event PlayerErrorEventHandler OnPlayerError;

        public void SetTheme(bool isDarkTheme)
        {
            if (!HasAlbumUrl)
                AlbumUrl = isDarkTheme ? SPOTIFY_ICON_DARK : SPOTIFY_ICON_LIGHT;
            SpotifyLogoUrl = isDarkTheme ? SPOTIFY_ICON_DARK : SPOTIFY_ICON_LIGHT;
        }

        public static bool IsPremiumUser => DataContainer.Instance.User != null && DataContainer.Instance.User.Product == "premium";

        public delegate void NewTrackEventHandler(string newId);
        public event TrackChangedEventHandler OnNewTrack;
        public delegate void TrackChangedEventHandler(string newId);
        public event TrackChangedEventHandler OnTrackChanged;
        public delegate void ProgressChangedEventHandler(int newProgress);
        public event ProgressChangedEventHandler OnProgressChanged;
        public delegate void VolumeChangedEventHandler(int newVolume);
        public event VolumeChangedEventHandler OnVolumeChanged;

        private bool hasAlbumUrl;
        public bool HasAlbumUrl
        {
            get => hasAlbumUrl;
            set => SetProperty(ref hasAlbumUrl, value, nameof(HasAlbumUrl));
        }
        private string albumUrl;
        public string AlbumUrl
        {
            get => albumUrl;
            set => SetProperty(ref albumUrl, value, nameof(AlbumUrl));
        }
        private string spotifyLogoUrl;
        public string SpotifyLogoUrl
        {
            get => spotifyLogoUrl;
            set => SetProperty(ref spotifyLogoUrl, value, nameof(SpotifyLogoUrl));
        }
        private string trackId;
        public string TrackId
        {
            get => trackId;
            set
            {
                if (value != trackId) {
                    SetProperty(ref trackId, value, nameof(TrackId));
                    OnNewTrack?.Invoke(value);
                }
                
                OnTrackChanged?.Invoke(value);
            }
        }
        private string trackName;
        public string TrackName
        {
            get => trackName;
            set => SetProperty(ref trackName, value, nameof(TrackName));
        }
        private FullTrack track;
        public FullTrack Track
        {
            get => track;
            set => SetProperty(ref track, value, nameof(Track));
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
            set
            {
                SetProperty(ref volume, value, nameof(Volume));
                OnVolumeChanged?.Invoke(value);
            }
        }
        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            set => SetProperty(ref isPlaying, value, nameof(IsPlaying));
        }

        private IList<Tag> tags;
        public IList<Tag> Tags
        {
            get => tags;
            set => SetProperty(ref tags, value, nameof(tags));
        }

        private string artistsGenreString;
        public string ArtistsGenreString
        { 
            get => artistsGenreString;
            set
            {
                SetProperty(ref artistsGenreString, value, nameof(artistsGenreString));
            }
        }

        private string tagString;
        public string TagString
        {
            get => tagString;
            set
            {
                SetProperty(ref tagString, value, nameof(tagString));
            }
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
            Logger.Information("started updating TrackInfo");
        }
        public void StopUpdateTrackInfoTimer()
        {
            UpdateTrackInfoTimer.Enabled = false;
            Logger.Information("stopped updating TrackInfo");
        }
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
            UpdatePlaybackInfoTimer.Elapsed += async (sender, e) => { try { await UpdatePlaybackInfo(); } catch (Exception ex) { Logger.Error($"UpdatePlaybackInfo error: {ex.Message}"); } };
            UpdatePlaybackInfoTimer.Enabled = true;
            Logger.Information("started updating PlaybackInfo");
        }
        public void StopUpdatePlaybackInfoTimer()
        {
            UpdatePlaybackInfoTimer.Enabled = false;
            Logger.Information("stopped updating PlaybackInfo");
        }

        private void UpdateTrackInfo(FullTrack track)
        {
            if (Spotify == null) return;
            var bestImg = track.Album.Images.OrderBy(img => img.Height).FirstOrDefault(img => img.Height >= 80 && img.Width >= 80);
            if (bestImg == null)
                bestImg = track.Album.Images.First();
            AlbumUrl = bestImg.Url;
            HasAlbumUrl = true;
            Track = track;
            TrackId = track.Id;
            TrackName = track.Name;
            ArtistsString = string.Join(", ", track.Artists.Select(a => a.Name));
            ProgressMax = track.DurationMs;
        }
        public async Task UpdatePlaybackInfo()
        {
            if (Spotify == null) return;
            try
            {
                var info = await Spotify.Player.GetCurrentPlayback(new PlayerCurrentPlaybackRequest());
                if (info == null) return;

                Volume = info.Device.VolumePercent ?? 0;
            }
            catch (Exception e)
            {
                Logger.Error($"Error in UpdatePlaybackInfo {e.Message}");
            }
        }
        public async Task UpdateTrackInfo()
        {
            if (Spotify == null) return;
            try
            {
                var info = await Spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
                if (info == null) return;
                IsPlaying = info.IsPlaying;
                Progress = info.ProgressMs ?? 0;

                if (info.Item is FullTrack track)
                    UpdateTrackInfo(track);
            }
            catch (Exception e)
            {
                Logger.Error($"Error in UpdateTrackInfo {e.Message}");
            }
        }
        public async Task Play()
        {
            if (!IsPremiumUser)
            {
                Logger.Information("can't resume playback (no spotify premium)");
                OnPlayerError?.Invoke(PlayerError.RequiresSpotifyPremium);
                return;
            }
            if (Spotify == null) return;
            if (IsPlaying) return;
            try
            {
                var success = await Spotify.Player.ResumePlayback();
                if (success)
                {
                    IsPlaying = true;
                    Logger.Information("resumed playback");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error in PlayerManager.Play {e.Message}");
            }
        }
        public async Task Pause()
        {
            if (!IsPremiumUser)
            {
                Logger.Information("can't pause playback (no spotify premium)");
                OnPlayerError?.Invoke(PlayerError.RequiresOpenSpotifyPlayer);
                return;
            }
            if (Spotify == null) return;
            if (!IsPlaying) return;
            try
            {
                var success = await Spotify.Player.PausePlayback();
                if (success)
                {
                    IsPlaying = false;
                    Logger.Information("paused playback");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error in PlayerManager.Pause {e.Message}");
            }
        }
        public async Task SetVolume(int newVolume)
        {
            if (!IsPremiumUser)
            {
                Logger.Information("can't update volume (no spotify premium)");
                OnPlayerError?.Invoke(PlayerError.RequiresSpotifyPremium);
                return;
            }
            if (Spotify == null) return;
            try
            {
                var success = await Spotify.Player.SetVolume(new PlayerVolumeRequest(newVolume));
                if (success)
                {
                    Logger.Information($"updated volume to {newVolume}");
                    Volume = newVolume;
                }

            }
            catch (Exception e)
            {
                Logger.Error($"Error in SetVolume {e.Message}");
            }
        }
        public async Task SetProgress(int newProgress)
        {
            if (!IsPremiumUser)
            {
                Logger.Information("can't update progress (no spotify premium)");
                OnPlayerError?.Invoke(PlayerError.RequiresSpotifyPremium);
                return;
            }
            if (Spotify == null) return;
            try
            {
                var success = await Spotify.Player.SeekTo(new PlayerSeekToRequest(newProgress));
                if (success)
                {
                    Logger.Information($"updated progress to {newProgress}");
                    Progress = newProgress;
                }

            }
            catch (Exception e)
            {
                Logger.Error($"Error in SetProgress {e.Message}");
            }
        }
        public async Task SetTrack(Track t)
        {
            if (!IsPremiumUser)
            {
                Logger.Information("can't update playing track (no spotify premium)");
                OnPlayerError?.Invoke(PlayerError.RequiresSpotifyPremium);
                return;
            }
            if (Spotify == null) return;
            try
            {
                var success = await Spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest { Uris = new List<string> { $"spotify:track:{t.Id}" } });
                if (success)
                {
                    Logger.Information($"updated playing track to {t.Name}");
                    await UpdateTrackInfo();
                }
            }
            catch (Exception e)
            {
                // check if a device is available
                var availableDevicesResponse = await Spotify.Player.GetAvailableDevices();
                if (availableDevicesResponse.Devices.Count == 0)
                {
                    Logger.Error($"can't update playing track (no device is available)");
                    OnPlayerError?.Invoke(PlayerError.RequiresOpenSpotifyPlayer);
                    return;
                }

                // if no device is set as active, GetCurrentPlayback returns null
                var playbackInfo = await Spotify.Player.GetCurrentPlayback();
                if (playbackInfo == null)
                {
                    Logger.Information($"No active device found --> using first device");
                    await Spotify.Player.TransferPlayback(new PlayerTransferPlaybackRequest(new List<string> { availableDevicesResponse.Devices[0].Id }) { Play = false });
                    // wait for playback to transfer
                    await Task.Delay(1000);
                    try
                    {
                        var success = await Spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest { Uris = new List<string> { $"spotify:track:{t.Id}" } });
                        if (success)
                            await UpdateTrackInfo();
                    }
                    catch (Exception e2)
                    {
                        Logger.Error($"Error in SetTrack could not ResumePlayback after using first available device {e2.Message}");
                    }
                    return;
                }
                Logger.Error($"Error in SetTrack {e.Message}");
            }
        }
    }
}
