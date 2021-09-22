using Backend;
using Backend.Entities;
using Backend.Entities.GraphNodes;
using NUnit.Framework;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Backend
{
    public class DatabaseTests
    {
        private DatabaseContext Db;

        [SetUp]
        public void SetUp()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            ConnectionManager.InitDb("TestDb");
            Db = ConnectionManager.Instance.Database;
        }
        [TearDown]
        public void TearDown()
        {
            Log.CloseAndFlush();
        }

        [Test]
        public void Graph_Saving()
        {
            var input = new InputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var output = new OutputNode
            {
                PlaylistName = "ChilloutLoungeCopy",
                Inputs = new List<GraphNode> { input },
            };
            Db.OutputNodes.Add(output);
            Db.SaveChanges();

            var nodes = Db.OutputNodes.ToList();
            Assert.AreEqual(1, Db.OutputNodes.Count());
            Assert.AreEqual(1, Db.InputNodes.Count());
        }
    }
}