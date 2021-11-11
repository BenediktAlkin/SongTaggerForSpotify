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

        [Test]
        public void InitialCreate_Migrate()
        {
            ConnectionManager.InitDb("res/InitialCreate");
            //ConnectionManager.InitDb("res/InitialCreateTest");
            using (var db = ConnectionManager.NewContext())
            {
            }
        }
    }
}
