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
            var input = new InputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var output = new OutputNode { PlaylistName = "ChilloutLoungeCopy" };
            output.AddInput(input);
            Db.OutputNodes.Add(output);
            Db.SaveChanges();

            var nodes = Db.OutputNodes.ToList();
            Assert.AreEqual(1, Db.OutputNodes.Count());
            Assert.AreEqual(1, Db.InputNodes.Count());
        }
    }
}