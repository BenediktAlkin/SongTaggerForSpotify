using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Util;

namespace Backend.Tests
{
    public class MigrationTests : BaseTests
    {
        [SetUp]
        public override void SetUp()
        {
            REQUIRES_DB = false;
            base.SetUp();
        }

        private void AssertDbIsValid()
        {
            using (var db = ConnectionManager.NewContext())
            {
                Assert.AreEqual(1, db.TagGroups.Count());
            }
        }

        [Test]
        [TestCase("EnsureCreated")]
        [TestCase("InitialCreate")]
        [TestCase("TagGroups")]
        public void UpdatesToLatestVersion(string dbName)
        {
            ConnectionManager.InitDb($"res/{dbName}");
            AssertDbIsValid();
        }

        [Test]
        public void CreateNewDb()
        {
            ConnectionManager.InitDb($"MigrationNewDb");
            AssertDbIsValid();
        }
    }
}
