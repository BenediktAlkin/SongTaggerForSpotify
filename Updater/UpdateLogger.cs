using System;
using System.IO;

namespace Updater
{
    public static class UpdateLogger
    {
        private const string LOG_FILE = "updater.log";
        private const string FORMAT = "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}";

        private static StreamWriter Writer { get; } = new StreamWriter(File.OpenWrite(LOG_FILE));

        public static void CloseAndFlush()
        {
            Writer.Flush();
            Writer.Close();
        }

        private static void Log(string level, string msg)
        {
            var composedMessage = string.Format(FORMAT, DateTime.Now, level, msg);
            Writer.WriteLine(composedMessage);
            Console.WriteLine(composedMessage);
        }

        public static void Information(string msg) => Log("INF", msg);
        public static void Error(string msg) => Log("ERR", msg);

    }
}
