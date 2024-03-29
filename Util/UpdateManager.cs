﻿using Serilog;
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
        private static ILogger Logger { get; set; }
        public static UpdateManager Instance { get; } = new();
        private UpdateManager() { }

        public bool IsChecking => State == UpdatingState.Checking;
        public bool IsPreparing => State == UpdatingState.Preparation;
        public bool IsDownloading => State == UpdatingState.Downloading;
        public bool IsExtracting => State == UpdatingState.Extracting;
        public bool IsRestarting => State == UpdatingState.Restarting;

        public delegate void UpdatingStateChangedEventHandler(UpdatingState newValue);
        public event UpdatingStateChangedEventHandler OnUpdatingStateChanged;
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
                OnUpdatingStateChanged?.Invoke(value);
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

        private static void InitLogger()
        {
            // for some reason doing this in the property initialization returned SilentLogger
            if (Logger == null)
                Logger = Log.ForContext("SourceContext", "UM");
        }

        public async Task<Version> UpdateToLatestRelease(string os, string user, string repo, Version currentVersion,
            string updaterName, string zipRootFolder, Action shutdownAction, bool startUpdater = true)
        {
            InitLogger();

            State = UpdatingState.Checking;
            var latestRelease = await Github.CheckForUpdate(user, repo, currentVersion);
            if (latestRelease == null)
                return null;

            await UpdateToRelease(os, user, repo, latestRelease, updaterName, zipRootFolder, shutdownAction, startUpdater);
            return latestRelease.Version;
        }
        public async Task UpdateToRelease(string os, string user, string repo, Github.Release release,
            string updaterName, string zipRootFolder, Action shutdownAction, bool startUpdater = true)
        {
            InitLogger();

            // filter installer
            var portableAssets = release.Assets.Where(a => a.Name.ToLower().Contains(".zip"));

            // get asset for os
            var asset = portableAssets.FirstOrDefault(a => a.Name.ToLower().Contains(os.ToLower()));
            if (asset == null)
            {
                Logger.Error($"failed to download update (no zip asset contains \"{os.ToLower()}\")");
                return;
            }
            var url = asset.BrowserDownloadUrl;
            var (zipFileName, zipFilePath) = PrepareUpdate(url);
            await DownloadUpdate(url, zipFilePath);
            ExtractUpdate(zipFilePath, updaterName, zipRootFolder);
            State = UpdatingState.Restarting;

            if (startUpdater)
            {
                try
                {
                    Process.Start(updaterName, $"{Environment.ProcessId}");
                }
                catch (Exception e)
                {
                    Logger.Error($"error starting updater: {e.Message}");
                }
                finally
                {
                    Logger.Information("started updater");
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
                Logger.Information("creating temp directory");
                if (Directory.Exists(TEMP_DIR))
                    Directory.Delete(TEMP_DIR, true);
                Directory.CreateDirectory(TEMP_DIR);
            }
            catch (Exception e)
            {
                Logger.Error($"failed to create/clear the temp directory: {e.Message}");
                return (null, null);
            }
            Logger.Information("created temp directory");
            return (zipFileName, zipFilePath);
        }
        private async Task DownloadUpdate(string url, string filePath)
        {
            State = UpdatingState.Downloading;

            try
            {
                using (var wc = new WebClient())
                {
                    Logger.Information("downloading latest version");
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
                Logger.Error($"failed update download: {e.Message}");
            }
            finally
            {
                Logger.Information("downloaded latest version");
            }
        }
        private void ExtractUpdate(string zipFilePath, string updaterName, string zipRootFolder)
        {
            State = UpdatingState.Extracting;
            try
            {
                // extract zip
                File.Move(zipFilePath, zipFilePath.Replace("rar", "zip"));
                Logger.Information("extracting zip file");
                ZipFile.ExtractToDirectory(zipFilePath, TEMP_DIR);

                // update updater (move all files starting with updaterName [e.g. Updater.exe, Updater.dll, ...])
                var tempAppDirPath = Path.Combine(TEMP_DIR, zipRootFolder);
                foreach (var filePath in Directory.GetFiles(tempAppDirPath))
                {
                    var fileName = filePath[(tempAppDirPath.Length + 1)..^0];
                    if (fileName.StartsWith(updaterName))
                    {
                        var newUpdaterFilePath = Path.Combine(tempAppDirPath, fileName);
                        File.Move(newUpdaterFilePath, fileName, true);
                        Logger.Information($"updated file {fileName}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"failed to extract update: {e.Message}");
            }
            finally
            {
                Logger.Information("extracted zip file");
            }
        }
    }
}
