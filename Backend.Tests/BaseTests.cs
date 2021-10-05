using NUnit.Framework;
using Serilog;

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
            ConnectionManager.InitDb("TestDb", dropDb: true, logTo: DatabaseQueryLogger.Instance.Information);
        }

        protected void RefreshDbConnection()
        {
            Db.Dispose();
            ConnectionManager.InitDb("TestDb", logTo: DatabaseQueryLogger.Instance.Information);
        }

        [TearDown]
        public virtual void TearDown()
        {
            Log.CloseAndFlush();
        }
    }
}
