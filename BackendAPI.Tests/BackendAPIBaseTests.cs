using Backend;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Util;

namespace BackendAPI.Tests
{
    public class BackendAPIBaseTests : BaseTests
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            ConnectionManager.Instance.IsApi = true;
        }
    }
}
