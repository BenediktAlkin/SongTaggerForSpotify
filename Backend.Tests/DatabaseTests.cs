using Backend.Entities.GraphNodes;
using NUnit.Framework;
using System.Linq;

namespace Backend.Tests
{
    public class DatabaseTests : BaseTests
    {

        [Test]
        public void Graph_Saving()
        {
            var input = new PlaylistInputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var output = new PlaylistOutputNode { PlaylistName = "ChilloutLoungeCopy" };
            output.AddInput(input);
            Db.PlaylistOutputNodes.Add(output);
            Db.SaveChanges();

            var nodes = Db.PlaylistOutputNodes.ToList();
            Assert.AreEqual(1, Db.PlaylistOutputNodes.Count());
            Assert.AreEqual(1, Db.PlaylistInputNodes.Count());
        }
    }
}