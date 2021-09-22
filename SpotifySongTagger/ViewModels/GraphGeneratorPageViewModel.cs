using Backend;
using Backend.Entities;
using Backend.Entities.GraphNodes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifySongTagger.ViewModels
{

    public class GraphGeneratorPageViewModel : BaseViewModel
    {
        private bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
            set => SetProperty(ref isRunning, value, nameof(IsRunning));
        }
        private GraphGeneratorPage graphGeneratorPage;
        public GraphGeneratorPage GraphGeneratorPage
        {
            get => graphGeneratorPage;
            set => SetProperty(ref graphGeneratorPage, value, nameof(GraphGeneratorPage));
        }
        public GraphGeneratorPageViewModel(GraphGeneratorPage graphGeneratorPage)
        {
            GraphGeneratorPage = graphGeneratorPage;
        }

        public async Task Run()
        {
            var outputNodes = GraphGeneratorPage.GraphNodes.Where(gn => gn is OutputNode).Cast<OutputNode>();
            IsRunning = true;
            foreach (var outputNode in outputNodes)
                await SpotifyOperations.SyncOutputNode(outputNode);
            IsRunning = false;
            Log.Information($"finished page {GraphGeneratorPage.Name}");
        }
    }
}
