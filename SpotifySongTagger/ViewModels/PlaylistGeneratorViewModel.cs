using Backend;
using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;
using SpotifySongTagger.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifySongTagger.ViewModels
{
    public class PlaylistGeneratorViewModel : BaseViewModel
    {
        public static async Task Init()
        {
            await DataContainer.LoadSourcePlaylists();
        }


        #region NodeTypes
        public List<NodeTypeCategory> NodeTypeCategories { get; } = new()
        {
            new NodeTypeCategory("Inputs", true, new[]
            {
                new NodeType("Playlist Input", typeof(PlaylistInputNode)),
            }),
            new NodeTypeCategory("Set Operations", true, new[]
            {
                new NodeType("Concat", typeof(ConcatNode)),
                new NodeType("Deduplicate", typeof(DeduplicateNode)),
                new NodeType("Remove", typeof(RemoveNode)),
            }),
            new NodeTypeCategory("Filters", true, new[]
            {
                new NodeType("Artist", typeof(FilterArtistNode)),
                new NodeType("Tag", typeof(FilterTagNode)),
                new NodeType("Release Year", typeof(FilterYearNode)),
            }),
            new NodeTypeCategory("Outputs", true, new[]
            {
                new NodeType("Playlist Output", typeof(PlaylistOutputNode)),
                new NodeType("Assign Tag", typeof(AssignTagNode)),
            }),
        };
        #endregion

        #region GraphGeneratorPages
        private bool isRunningAll;
        public bool IsRunningAll
        {
            get => isRunningAll;
            set => SetProperty(ref isRunningAll, value, nameof(IsRunningAll));
        }
        public ObservableCollection<GraphGeneratorPageViewModel> GraphGeneratorPageVMs { get; } =
            new(ConnectionManager.Instance.Database.GraphGeneratorPages
                .Include(ggp => ggp.GraphNodes).ThenInclude(gn => gn.Outputs)
                .Select(ggp => new GraphGeneratorPageViewModel(ggp)));
        private GraphGeneratorPageViewModel selectedGraphGeneratorPageVM;
        public GraphGeneratorPageViewModel SelectedGraphGeneratorPageVM
        {
            get => selectedGraphGeneratorPageVM;
            set
            {
                SetProperty(ref selectedGraphGeneratorPageVM, value, nameof(SelectedGraphGeneratorPageVM));

                if (value == null)
                    GraphEditorVM = null;
                else
                    GraphEditorVM = new GraphEditorViewModel(value.GraphGeneratorPage);
                NotifyPropertyChanged(nameof(GraphEditorVM));
            }
        }
        private GraphEditorViewModel graphEditorVM;
        public GraphEditorViewModel GraphEditorVM
        {
            get => graphEditorVM;
            set => SetProperty(ref graphEditorVM, value, nameof(GraphEditorVM));
        }
        private string newGraphGeneratorPageName;
        public string NewGraphGeneratorPageName
        {
            get => newGraphGeneratorPageName;
            set => SetProperty(ref newGraphGeneratorPageName, value, nameof(NewGraphGeneratorPageName));
        }
        public void AddGraphGeneratorPage(string name)
        {
            // add to db
            var page = new GraphGeneratorPage { Name = name };
            var node = new PlaylistOutputNode { PlaylistName = name };
            page.GraphNodes.Add(node);
            ConnectionManager.Instance.Database.GraphGeneratorPages.Add(page);
            ConnectionManager.Instance.Database.SaveChanges();

            // update ui
            GraphGeneratorPageVMs.Add(new GraphGeneratorPageViewModel(page));
        }
        public void RemoveGraphGeneratorPage(GraphGeneratorPageViewModel toDelete)
        {
            if (toDelete == null) return;

            // remove in db
            ConnectionManager.Instance.Database.GraphGeneratorPages.Remove(toDelete.GraphGeneratorPage);
            ConnectionManager.Instance.Database.SaveChanges();

            // update ui
            GraphGeneratorPageVMs.Remove(toDelete);
        }

        public void EditGraphGeneratorPageName()
        {
            SelectedGraphGeneratorPageVM.GraphGeneratorPage.Name = NewGraphGeneratorPageName;
            ConnectionManager.Instance.Database.SaveChanges();
        }

        #endregion
    }

    public record NodeTypeCategory(string Name, bool IsExpanded, NodeType[] NodeTypes);
    public record NodeType(string Name, Type Type);
}
