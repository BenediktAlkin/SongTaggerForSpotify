using NUnit.Framework;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Util;
using static Backend.ConnectionManager;

namespace Backend.Tests
{
    public class ConnectionManagerTests : BaseTests
    {
        private const string DST_FOLDER_NAME = "ChangeDatabaseFolderTest";
        private const string USER_ID = "ChangeDatabaseTestUserId";


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            DataContainer.Instance.Clear();

            // change folder back to base path
            ConnectionManager.Instance.ChangeDatabaseFolder(Directory.GetCurrentDirectory());
            // remove previous run files
            if (Directory.Exists(DST_FOLDER_NAME))
                Directory.Delete(DST_FOLDER_NAME, true);
            File.Delete($"{USER_ID}.sqlite");
        }


        [Test]
        public void ChangeDatabaseFolder_NotLoggedIn()
        {
            var dstFolderPath = Path.Combine(Directory.GetCurrentDirectory(), DST_FOLDER_NAME);

            // dst folder does not exist
            Assert.AreEqual(ChangeDatabaseFolderResult.Failed, ConnectionManager.Instance.ChangeDatabaseFolder(dstFolderPath));

            Directory.CreateDirectory(DST_FOLDER_NAME);
            Assert.AreEqual(ChangeDatabaseFolderResult.ChangedPath, ConnectionManager.Instance.ChangeDatabaseFolder(dstFolderPath));
            Assert.AreEqual(dstFolderPath, File.ReadAllText(ConnectionManager.DB_FOLDER_FILE));
            Assert.AreEqual(dstFolderPath, ConnectionManager.Instance.DbPath);
        }

        [Test]
        public void ChangeDatabaseFolder_CopiedToNewFolder()
        {
            InitSpotify(user: new PrivateUser { Id = USER_ID });
            
            // check if dbfile exists
            Assert.IsTrue(File.Exists(DataContainer.Instance.DbFileName));
            // insert some data
            DatabaseOperations.AddTag(new Entities.Tag { Name = "TestTag" });
            Assert.AreEqual(1, DatabaseOperations.GetTagsWithGroups().Count);

            // change directory
            Directory.CreateDirectory(DST_FOLDER_NAME);
            var dstFolderPath = Path.Combine(Directory.GetCurrentDirectory(), DST_FOLDER_NAME);
            Assert.AreEqual(ChangeDatabaseFolderResult.CopiedToNewFolder, ConnectionManager.Instance.ChangeDatabaseFolder(dstFolderPath));

            // check if dbfile was copied
            Assert.IsTrue(File.Exists(DataContainer.Instance.DbFileName));
            Assert.IsTrue(File.Exists(Path.Combine(dstFolderPath, DataContainer.Instance.DbFileName)));
            // check if data is preserved
            Assert.AreEqual(1, DatabaseOperations.GetTagsWithGroups().Count);
            // in new directory there should be 1 file
            Assert.AreEqual(1, Directory.GetFiles(DST_FOLDER_NAME).Length);
        }

        [Test]
        public void ChangeDatabaseFolder_WithOverwriting()
        {
            InitSpotify(user: new PrivateUser { Id = USER_ID });

            // check if dbfile exists
            Assert.IsTrue(File.Exists(DataContainer.Instance.DbFileName));
            // insert some data
            DatabaseOperations.AddTag(new Entities.Tag { Name = "TestTag" });
            Assert.AreEqual(1, DatabaseOperations.GetTagsWithGroups().Count);
            // copy it to output directory
            Directory.CreateDirectory(DST_FOLDER_NAME);
            File.Copy(DataContainer.Instance.DbFileName, Path.Combine(DST_FOLDER_NAME, DataContainer.Instance.DbFileName));

            // add some more data
            DatabaseOperations.AddTag(new Entities.Tag { Name = "TestTag2" });

            // change directory
            var dstFolderPath = Path.Combine(Directory.GetCurrentDirectory(), DST_FOLDER_NAME);
            Assert.AreEqual(ChangeDatabaseFolderResult.UseExistingDbInNewFolder, ConnectionManager.Instance.ChangeDatabaseFolder(dstFolderPath));

            // check if dbfile was copied
            Assert.IsTrue(File.Exists(DataContainer.Instance.DbFileName));
            Assert.IsTrue(File.Exists(Path.Combine(dstFolderPath, DataContainer.Instance.DbFileName)));
            // check if data is preserved
            Assert.AreEqual(1, DatabaseOperations.GetTagsWithGroups().Count);
            // in new directory there should be 1 file (the existing db file)
            Assert.AreEqual(1, Directory.GetFiles(DST_FOLDER_NAME).Length);
        }
    }
}
