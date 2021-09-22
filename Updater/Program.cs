using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Updater
{
    public class Program
    {
        private const string LOG_FILE = "updater.log";
        private const string APPLICATION_NAME = "Application";
        private const string UPDATER_NAME = "Updater";
        public const string TEMP_DIR = "temp";

        private static readonly Action<string> LogInformation = Log.Information;
        private static readonly Action<string> LogError = Log.Error;

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(LOG_FILE)
                .CreateLogger();

            Console.Title = $"{APPLICATION_NAME} {UPDATER_NAME}";
            Console.CursorVisible = false;

            // log version
            var version = typeof(Program).Assembly.GetName().Version;
            LogInformation($"Updater {version}");

            int? procId = args.Length == 0 ? null : int.Parse(args[0]);
            TerminateApplication(procId);

            MoveTempToBaseDir();

            LogInformation($"Starting application");
            Process.Start($"{APPLICATION_NAME}.exe");

            LogInformation($"Finished updater");

            Log.CloseAndFlush();
        }

        private static void TerminateApplication(int? procId)
        {
            // wait application to exit
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
            var path = Path.Combine(TEMP_DIR, APPLICATION_NAME);
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
