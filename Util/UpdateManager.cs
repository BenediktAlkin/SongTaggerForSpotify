using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Util
{
    public class UpdateManager : NotifyPropertyChangedBase
    {
        public const string TEMP_DIR = "temp";
        public const string ZIP_DEFAULT_NAME = "temp.zip";

        public static UpdateManager Instance { get; } = new();
        private UpdateManager() { }

        public bool IsChecking => State == UpdatingState.Checking;
        public bool IsPreparing => State == UpdatingState.Preparation;
        public bool IsDownloading => State == UpdatingState.Downloading;
        public bool IsExtracting => State == UpdatingState.Extracting;
        public bool IsRestarting => State == UpdatingState.Restarting;

        private UpdatingState state;
        public UpdatingState State
        {
            get => state;
            private set
            {
                SetProperty(ref state, value, nameof(State));
                NotifyPropertyChanged(nameof(IsChecking));
                NotifyPropertyChanged(nameof(IsPreparing));
                NotifyPropertyChanged(nameof(IsDownloading));
                NotifyPropertyChanged(nameof(IsExtracting));
                NotifyPropertyChanged(nameof(IsRestarting));
            }
        }
        public enum UpdatingState
        {
            Checking,
            Preparation,
            Downloading,
            Extracting,
            Restarting
        }
        private double updateSizeMb;
        public double UpdateSizeMb
        {
            get => updateSizeMb;
            set => SetProperty(ref updateSizeMb, value, nameof(UpdateSizeMb));
        }
        private double updateProgressMb;
        public double UpdateProgressMb
        {
            get => updateProgressMb;
            set => SetProperty(ref updateProgressMb, value, nameof(UpdateProgressMb));
        }
        private double updateProgressPercent;
        public double UpdateProgressPercent
        {
            get => updateProgressPercent;
            set => SetProperty(ref updateProgressPercent, value, nameof(UpdateProgressPercent));
        }

        public async Task<Version> UpdateToLatestRelease(string user, string repo, Version currentVersion, string updaterName, string applicationName, Action shutdownAction, bool startUpdater = true)
        {
            State = UpdatingState.Checking;
            var latestRelease = await Github.CheckForUpdate(user, repo, currentVersion);
            if (latestRelease == null)
                return null;

            await UpdateToRelease(user, repo, latestRelease, updaterName, applicationName, shutdownAction, startUpdater);
            return latestRelease.Version;
        }
        public async Task UpdateToRelease(string user, string repo, Github.Release release, string updaterName, string applicationName, Action shutdownAction, bool startUpdater = true)
        {
            var url = release.Assets[0].BrowserDownloadUrl;
            var (zipFileName, zipFilePath) = PrepareUpdate(url);
            await DownloadUpdate(url, zipFilePath);
            ExtractUpdate(zipFilePath, updaterName, applicationName);
            State = UpdatingState.Restarting;

            if (startUpdater)
            {
                try
                {
                    Process.Start(updaterName, $"{Environment.ProcessId}");
                }
                catch (Exception e)
                {
                    Log.Error($"Error starting updater: {e.Message}");
                }
            }
            shutdownAction?.Invoke();
        }
        private (string, string) PrepareUpdate(string url)
        {
            State = UpdatingState.Preparation;

            var zipFileName = url.Split('/').LastOrDefault() ?? ZIP_DEFAULT_NAME;
            var zipFilePath = Path.Combine(TEMP_DIR, zipFileName);
            try
            {
                Log.Information("Creating temp directory");
                if (Directory.Exists(TEMP_DIR))
                    Directory.Delete(TEMP_DIR, true);
                Directory.CreateDirectory(TEMP_DIR);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to create/clear the temp directory: {e.Message}");
                return (null, null);
            }
            return (zipFileName, zipFilePath);
        }
        private async Task DownloadUpdate(string url, string filePath)
        {
            State = UpdatingState.Downloading;

            try
            {
                using (var wc = new WebClient())
                {
                    Log.Information("Downloading latest version");
                    wc.DownloadProgressChanged += (sender, e) =>
                    {
                        UpdateSizeMb = (double)e.TotalBytesToReceive / 1024 / 1024;
                        UpdateProgressMb = (double)e.BytesReceived / 1024 / 1024;
                        UpdateProgressPercent = e.ProgressPercentage;
                    };
                    await wc.DownloadFileTaskAsync(url, filePath);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed update download: {e.Message}");
            }
        }
        private void ExtractUpdate(string zipFilePath, string updaterName, string applicationName)
        {
            State = UpdatingState.Extracting;
            try
            {
                // extract zip
                File.Move(zipFilePath, zipFilePath.Replace("rar", "zip"));
                Log.Information("Extracting files...");
                ZipFile.ExtractToDirectory(zipFilePath, TEMP_DIR);

                // update updater (copy all files starting with updaterName [e.g. Updater.exe, Updater.dll, ...])
                var tempAppDirPath = Path.Combine(TEMP_DIR, applicationName);
                foreach (var filePath in Directory.GetFiles(tempAppDirPath))
                {
                    var fileName = filePath[(tempAppDirPath.Length + 1)..^0];
                    if (fileName.StartsWith(updaterName))
                    {
                        var newUpdaterFilePath = Path.Combine(tempAppDirPath, fileName);
                        File.Move(newUpdaterFilePath, fileName, true);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to extract update: {e.Message}");
                return;
            }
        }
    }
}
