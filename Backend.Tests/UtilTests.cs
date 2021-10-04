using Backend.Entities;
using Backend.Entities.GraphNodes;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Tests
{
    public class UtilTests : BaseTests
    {

        [Test]
        public void SyncLists_Remove()
        {
            var list1 = new List<Playlist>
            {
                new Playlist { Id = "1", Name = "1" },
                new Playlist { Id = "2", Name = "2" },
                new Playlist { Id = "3", Name = "3" },
            };
            var list2 = list1.ToList();
            list2.RemoveAt(0);

            Utils.SyncLists(list1, list2);
            Assert.IsTrue(Enumerable.SequenceEqual(list1, list2));
        }
        [Test]
        public void SyncLists_Add_Ordered()
        {
            var list1 = new List<Playlist>
            {
                new Playlist { Id = "1", Name = "1" },
                new Playlist { Id = "2", Name = "2" },
                new Playlist { Id = "3", Name = "3" },
            };
            var list2 = list1.ToList();
            list2.Add(new Playlist { Id = "4", Name = "4" });

            Utils.SyncLists(list1, list2);
            Assert.IsTrue(Enumerable.SequenceEqual(list1, list2));
        }
        [Test]
        public void SyncLists_Add_Unordered()
        {
            var list1 = new List<Playlist>
            {
                new Playlist { Id = "1", Name = "1" },
                new Playlist { Id = "2", Name = "2" },
                new Playlist { Id = "3", Name = "3" },
            };
            var list2 = list1.ToList();
            list2.Insert(1, new Playlist { Id = "4", Name = "4" });

            Utils.SyncLists(list1, list2);
            Assert.IsTrue(Enumerable.SequenceEqual(list1, list2));
        }
        [Test]
        public void SyncLists_Same()
        {
            var list1 = new List<Playlist>
            {
                new Playlist { Id = "1", Name = "1" },
                new Playlist { Id = "2", Name = "2" },
                new Playlist { Id = "3", Name = "3" },
            };
            var list2 = list1.ToList();

            Utils.SyncLists(list1, list2);
            Assert.IsTrue(Enumerable.SequenceEqual(list1, list2));
        }
        [Test]
        public void SyncLists_ManyOperations()
        {
            var list1 = new List<Playlist>
            {
                new Playlist { Id = "1", Name = "1" },
                new Playlist { Id = "2", Name = "2" },
                new Playlist { Id = "3", Name = "3" },
            };
            var list2 = list1.ToList();
            list2.Add(new Playlist { Id = "4", Name = "4" });
            list2.RemoveAt(0);
            list2.Insert(0, new Playlist { Id = "5", Name = "5" });

            Utils.SyncLists(list1, list2);
            Assert.IsTrue(Enumerable.SequenceEqual(list1, list2));
        }
    }
}