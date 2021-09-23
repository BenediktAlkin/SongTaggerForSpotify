using NUnit.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class BaseTests
    {
        protected static DatabaseContext Db => ConnectionManager.Instance.Database;

        [SetUp]
        public virtual void SetUp()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            ConnectionManager.InitDb("TestDb", dropDb:true, logTo:DatabaseQueryLogger.Instance.Information);
        }
        [TearDown]
        public virtual void TearDown()
        {
            Log.CloseAndFlush();
        }
    }
}
