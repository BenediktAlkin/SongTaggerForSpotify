using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Util
{
    public static class MockUtil
    {
        public static ILogger<T> GetMockedLogger<T>(Serilog.ILogger serilogLogger)
        {
            var mockedLogger = new Mock<ILogger<T>>();
            mockedLogger.Setup(x => x.Log(
                  It.IsAny<LogLevel>(),
                  It.IsAny<EventId>(),
                  It.IsAny<It.IsAnyType>(),
                  It.IsAny<Exception>(),
                  (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var logLevel = (LogLevel)invocation.Arguments[0];
                    var eventId = (EventId)invocation.Arguments[1];
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];
                    var invokeMethod = formatter.GetType().GetMethod("Invoke");
                    var logMessage = (string)invokeMethod?.Invoke(formatter, new[] { state, exception });
                    switch (logLevel)
                    {
                        case LogLevel.Debug: serilogLogger.Debug(logMessage); break;
                        case LogLevel.Information: serilogLogger.Information(logMessage); break;
                        case LogLevel.Warning: serilogLogger.Warning(logMessage); break;
                        case LogLevel.Error: serilogLogger.Error(logMessage); break;
                        case LogLevel.Critical: serilogLogger.Fatal(logMessage); break;
                        default: serilogLogger.Fatal(logMessage); break;
                    }
                }));
            return mockedLogger.Object;
        }
    }
}
