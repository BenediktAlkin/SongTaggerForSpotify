using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace BackendAPI
{
    public class RequestTimer<T> : IDisposable
    {
        private ILogger<T> Logger { get; }
        public string LogMessage { get; set; }
        public string DetailMessage { get; set; }
        public string ErrorMessage { get; set; }

        private Stopwatch Stopwatch { get; set; }

        public RequestTimer(ILogger<T> logger)
        {
            Logger = logger;
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }
        public RequestTimer(string logMessage, ILogger<T> logger) : this(logger)
        {
            LogMessage = logMessage;
        }

        public void Dispose()
        {
            Stopwatch.Stop();
            var msgStr = LogMessage == default ? "No LogMessage for RequestTimer" : LogMessage;
            var detailMessageStr = DetailMessage == null ? string.Empty : $" {DetailMessage}";
            var errorStr = ErrorMessage == null ? string.Empty : $" failed({ErrorMessage})";
            var timeStr = $" ({Stopwatch.ElapsedMilliseconds}ms)";

            Logger.LogInformation($"{msgStr}{detailMessageStr}{errorStr}{timeStr}");
            GC.SuppressFinalize(this);
        }
    }
}
