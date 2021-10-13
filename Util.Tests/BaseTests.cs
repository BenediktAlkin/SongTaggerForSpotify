using NUnit.Framework;
using Serilog;

namespace Util.Tests
{
    public class BaseTests
    {
        [SetUp]
        public void SetUp()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(formatter: new LogFormatter("??"))
                .CreateLogger();
        }
        [TearDown]
        public void TearDown()
        {
            Log.CloseAndFlush();
        }
    }
}
