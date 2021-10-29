using NUnit.Framework;
using Serilog;
using Util;

namespace Backend.Tests.Spotify
{
    public class BaseTests
    {
        [SetUp]
        public virtual void SetUp()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(formatter: new LogFormatter("??"))
                .CreateLogger();
        }
        [TearDown]
        public virtual void TearDown()
        {
            Log.CloseAndFlush();
        }
    }
}
