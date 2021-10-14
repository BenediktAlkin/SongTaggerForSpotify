namespace Util.Tests
{
    public class UpdateManagerTests : BaseTests
    {
        //private const string USER = "BenediktAlkin";
        //private const string REPO = "UpdaterTest";
        //private const string UPDATER_NAME = "Updater";
        //private const string APPLICATION_NAME = "Application";
        //private static readonly Version MIN_VERSION = new(2, 0, 0);

        //private const int MAX_TIMEOUT = 10000;
        //private const int TIMEOUT = 1000;

        //private static List<string> RunProcess(string fileName, string args = "", bool captureOutput = false)
        //{
        //    var proc = new Process
        //    {
        //        StartInfo = new ProcessStartInfo
        //        {
        //            FileName = fileName,
        //            Arguments = args,
        //            UseShellExecute = false,
        //            RedirectStandardOutput = captureOutput,
        //            CreateNoWindow = true
        //        }
        //    };
        //    Log.Information($"Starting {fileName} {args}");
        //    var stdout = new List<string>();
        //    proc.Start();
        //    // for some reason if RedirectStandardOutput = true the process terminates immediatley
        //    // this happens always with Update.exe and sometimes with Application.exe
        //    if (captureOutput)
        //    {
        //        while (!proc.StandardOutput.EndOfStream)
        //        {
        //            var line = proc.StandardOutput.ReadLine();
        //            stdout.Add(line);
        //            Log.Information(line);
        //        }
        //    }
        //    proc.WaitForExit();
        //    Log.Information($"Finished {fileName} {proc.ExitCode}");

        //    return stdout;
        //}

        //private static async Task RemoveAllFiles()
        //{
        //    // avoid access denied to remove files because of other tests
        //    var timeout = 0;
        //    while (timeout < MAX_TIMEOUT)
        //    {
        //        try
        //        {
        //            foreach (var filePath in Directory.GetFiles(Directory.GetCurrentDirectory()))
        //            {
        //                var fileName = Path.GetFileName(filePath);
        //                if (fileName.StartsWith(UPDATER_NAME))
        //                    File.Delete(filePath);
        //                if (fileName.StartsWith(APPLICATION_NAME))
        //                    File.Delete(filePath);
        //            }
        //            if (Directory.Exists(UpdateManager.TEMP_DIR))
        //                Directory.Delete(UpdateManager.TEMP_DIR, true);
        //            return;
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error(e.Message);
        //            timeout += TIMEOUT;
        //            await Task.Delay(TIMEOUT);
        //        }
        //    }
        //    Assert.Fail("Failed to remove old files");
        //}

        //[Test]
        //public async Task DownloadUpdate()
        //{
        //    await RemoveAllFiles();

        //    await UpdateManager.Instance.UpdateToLatestRelease(USER, REPO, new Version(1, 0, 0), UPDATER_NAME, APPLICATION_NAME, null);

        //    // zip exists
        //    Assert.AreEqual(1, Directory.GetFiles(UpdateManager.TEMP_DIR).Length);
        //    // zip is extracted
        //    Assert.AreEqual(1, Directory.GetDirectories(UpdateManager.TEMP_DIR).Length);
        //    // check if APPLICATION_NAME is correct
        //    Assert.AreEqual(Path.Combine(UpdateManager.TEMP_DIR, APPLICATION_NAME), Directory.GetDirectories(UpdateManager.TEMP_DIR).First());
        //    // updater is updated (moved to base directory)
        //    Assert.IsTrue(File.Exists($"{UPDATER_NAME}.exe"));
        //    Assert.IsFalse(File.Exists(Path.Combine(UpdateManager.TEMP_DIR, APPLICATION_NAME, $"{UPDATER_NAME}.exe")));
        //}

        //[Test]
        //public async Task ApplyLatestUpdate()
        //{
        //    await RemoveAllFiles();

        //    var newVersion = await UpdateManager.Instance.UpdateToLatestRelease(USER, REPO, MIN_VERSION,
        //        UPDATER_NAME, APPLICATION_NAME, null, false);

        //    // automatic updater call in update is disabled
        //    // run updater manually
        //    var updaterOutput = RunProcess($"{UPDATER_NAME}.exe");
        //    //Assert.AreEqual($"{UPDATER_NAME} {newVersion}.0", updaterOutput[0]);
        //    Assert.IsTrue(File.Exists($"{APPLICATION_NAME}.exe"));

        //    // run application
        //    var applicationOutput = RunProcess($"{APPLICATION_NAME}.exe", captureOutput: true);
        //    Assert.AreEqual($"{APPLICATION_NAME} {newVersion}.0", applicationOutput[0]);
        //}
        //[Test]
        //public async Task ApplySpecificUpdate()
        //{
        //    await RemoveAllFiles();

        //    var releases = await Github.GetReleases(USER, REPO);
        //    var release = releases.First(r => r.Version == new Version(2, 0, 0));
        //    var newVersion = release.Version;
        //    await UpdateManager.Instance.UpdateToRelease(USER, REPO, release,
        //        UPDATER_NAME, APPLICATION_NAME, null, false);

        //    // automatic updater call in update is disabled
        //    // run updater manually
        //    var updaterOutput = RunProcess($"{UPDATER_NAME}.exe", captureOutput: false);
        //    //Assert.AreEqual($"{UPDATER_NAME} {newVersion}.0", updaterOutput[0]);
        //    Assert.IsTrue(File.Exists($"{APPLICATION_NAME}.exe"));

        //    // run application
        //    //var applicationOutput = RunProcess($"{APPLICATION_NAME}.exe", captureOutput: false);
        //    //Assert.AreEqual($"{APPLICATION_NAME} {newVersion}.0", applicationOutput[0]);
        //}
    }
}
