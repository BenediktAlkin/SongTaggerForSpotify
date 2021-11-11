using NUnit.Framework;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Util;

namespace Backend.Tests
{
    public class ConnectionManagerTests : BaseTests
    {
        [Test]
        public void MoveDatabaseFile()
        {
            const string USER_ID = "MoveDatabaseTestUserId";
            InitSpotify(user: new PrivateUser { Id = USER_ID });
            
            // check if dbfile exists
            Assert.IsTrue(File.Exists(DataContainer.Instance.DbFileName));
            // insert some data
            DatabaseOperations.AddTag(new Entities.Tag { Name = "TestTag" });
            Assert.AreEqual(1, DatabaseOperations.GetTagsWithGroups().Count);

            // move file
            const string DST_FOLDER_NAME = "MoveDatabaseTo";
            Directory.CreateDirectory(DST_FOLDER_NAME);
            var dstFolderPath = Path.Combine(Directory.GetCurrentDirectory(), DST_FOLDER_NAME);
            ConnectionManager.MoveDatabaseFile(Directory.GetCurrentDirectory(), dstFolderPath);

            // check if dbfile was moved
            Assert.IsFalse(File.Exists(DataContainer.Instance.DbFileName));
            Assert.IsTrue(File.Exists(Path.Combine(dstFolderPath, DataContainer.Instance.DbFileName)));
            // check if data is preserved
            Assert.AreEqual(1, DatabaseOperations.GetTagsWithGroups().Count);
        }
    }
}
