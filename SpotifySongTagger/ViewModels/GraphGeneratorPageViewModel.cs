using Backend;
using Backend.Entities;
using Backend.Entities.GraphNodes;
using Serilog;
using System.Linq;
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
            var assignTagNodes = GraphGeneratorPage.GraphNodes.Where(gn => gn is AssignTagNode).Cast<AssignTagNode>();
            IsRunning = true;
            foreach (var outputNode in outputNodes)
                await SpotifyOperations.SyncOutputNode(outputNode);
            foreach (var assignTagNode in assignTagNodes)
                await DatabaseOperations.AssignTags(assignTagNode);
            IsRunning = false;
            Log.Information($"finished page {GraphGeneratorPage.Name}");
        }
    }
}
