using NUnit.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util.Tests
{
    public class BaseTests
    {
        [SetUp]
        public void SetUp()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }
        [TearDown]
        public void TearDown()
        {
            Log.CloseAndFlush();
        }
    }
}
