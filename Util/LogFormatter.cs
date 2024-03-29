﻿using Serilog.Events;
using Serilog.Formatting;
using System;
using System.IO;

namespace Util
{
    public class LogFormatter : ITextFormatter
    {
        private string DefaultSourceContext { get; }
        public LogFormatter(string defaultSourceContext)
        {
            DefaultSourceContext = defaultSourceContext;
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            var sourceContextStr = DefaultSourceContext;
            if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
            {
                // remove " before and after
                sourceContextStr = sourceContext.ToString();
                sourceContextStr = sourceContextStr.Substring(1, sourceContextStr.Length - 2);
                // extract class name
                sourceContextStr = sourceContextStr.Substring(sourceContextStr.LastIndexOf('.') + 1);
            }
            var exceptionStr = string.Empty;
            if (logEvent.Exception != null)
                exceptionStr = $"{logEvent.Exception}{Environment.NewLine}";


            output.Write($"{logEvent.Timestamp:HH:mm:ss} " +
                $"{logEvent.Level.ToString()[0]} " +
                $"{sourceContextStr} {logEvent.RenderMessage()}{Environment.NewLine}{exceptionStr}");
        }
    }
}
