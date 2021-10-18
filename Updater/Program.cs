using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Updater
{
    public class Program
    {
        private const string APPLICATION_NAME = "SongTaggerForSpotify";
        private const string APPLICATION= "SpotifySongTagger.exe";
        private const string UPDATER_NAME = "Updater";
        public const string TEMP_DIR = "temp";

        private static readonly Action<string> LogInformation = UpdateLogger.Information;
        private static readonly Action<string> LogError = UpdateLogger.Information;

        private static void Main(string[] args)
        {
            Console.Title = $"{APPLICATION_NAME} {UPDATER_NAME}";
            Console.CursorVisible = false;

            // log version
            var version = typeof(Program).Assembly.GetName().Version;
            LogInformation($"Updater {version}");

            int? procId = args.Length == 0 ? null : int.Parse(args[0]);
            TerminateApplication(procId);

            MoveTempToBaseDir();

            LogInformation($"Starting application");
            try
            {
                Process.Start(APPLICATION);
            }
            catch(Exception e)
            {
                LogError($"could not start application {APPLICATION} (workdir={Directory.GetCurrentDirectory()}): {e.Message}");
            }
            

            LogInformation($"Finished updater");
            UpdateLogger.CloseAndFlush();
        }

        private static void TerminateApplication(int? procId)
        {
            // wait for application to exit
            Thread.Sleep(1000);
            if (procId == null) return;

            try
            {
                // kill application
                if (Process.GetProcesses().Any(p => p.Id == procId))
                {
                    Process.GetProcessById(procId.Value).Kill();
                    LogInformation("Killed application process");
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to terminate application: {e.Message}");
            }
        }

        private static void MoveTempToBaseDir()
        {
            // move everything in TEMP_DIR/APPLICATION_NAME to base path
            string path = string.Empty;
            try
            {
                path = Path.Combine(TEMP_DIR, APPLICATION_NAME);
            }
            catch(Exception)
            {
                LogError($"invalid path (TEMP_DIR={TEMP_DIR} APPLICATION_NAME={APPLICATION_NAME})");
                return;
            }
            
            foreach (var dir in Directory.GetDirectories(path))
            {
                var dstPath = dir[(path.Length + 1)..^0];
                try
                {
                    if (Directory.Exists(dstPath))
                        Directory.Delete(dstPath, true);
                    Directory.Move(dir, dstPath);
                    LogInformation($"Updated directory {dstPath}");
                }
                catch (Exception e)
                {
                    LogError($"Failed to copy directory {dstPath}: {e.Message}");
                }
            }
            foreach (var file in Directory.GetFiles(path))
            {
                var dstPath = file[(path.Length + 1)..^0];
                try
                {
                    File.Move(file, dstPath, true);
                    LogInformation($"Updated file {dstPath}");
                }
                catch (Exception e)
                {
                    LogError($"Failed to copy file {dstPath}: {e.Message}");
                }
            }
        }
    }
}
