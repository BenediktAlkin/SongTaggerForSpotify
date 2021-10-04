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
        public async Task Init()
        {
            await DataContainer.LoadSourcePlaylists();
            var ggps = ConnectionManager.Instance.Database.GraphGeneratorPages
                .Include(ggp => ggp.GraphNodes).ThenInclude(gn => gn.Outputs);
            foreach (var ggp in ggps)
                GraphGeneratorPages.Add(ggp);
        }


        #region NodeTypes
        public List<NodeTypeCategory> NodeTypeCategories { get; } = new()
        {
            new NodeTypeCategory("Inputs", "provide songs for other nodes", true, new[]
            {
                new NodeType("Playlist Input", "all songs of a playlist", typeof(PlaylistInputNode)),
            }),
            new NodeTypeCategory("Set Operations", "perform set operations on their inputs", true, new[]
            {
                new NodeType("Concat", "concatenate all inputs", typeof(ConcatNode)),
                new NodeType("Deduplicate", "remove duplicates", typeof(DeduplicateNode)),
                new NodeType("Remove", "removes a set from another set", typeof(RemoveNode)),
                new NodeType("Intersect", "intersection of all inputs", typeof(IntersectNode)),
            }),
            new NodeTypeCategory("Filters", "remove all songs that do not match a filter", true, new[]
            {
                new NodeType("Artist", "filter by artist", typeof(FilterArtistNode)),
                new NodeType("Tag", "filter by tag", typeof(FilterTagNode)),
                new NodeType("Release Year", "filter by release year", typeof(FilterYearNode)),
            }),
            new NodeTypeCategory("Outputs", "generate an output", true, new[]
            {
                new NodeType("Playlist Output", "generate a playlist", typeof(PlaylistOutputNode)),
                new NodeType("Assign Tag", "assign a tag", typeof(AssignTagNode)),
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
        public ObservableCollection<GraphGeneratorPage> GraphGeneratorPages { get; } = new();
        private GraphGeneratorPage selectedGraphGeneratorPage;
        public GraphGeneratorPage SelectedGraphGeneratorPage
        {
            get => selectedGraphGeneratorPage;
            set
            {
                SetProperty(ref selectedGraphGeneratorPage, value, nameof(SelectedGraphGeneratorPage));

                if (value == null)
                    GraphEditorVM = null;
                else
                    GraphEditorVM = new GraphEditorViewModel(value);
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
            GraphGeneratorPages.Add(page);
        }
        public void RemoveGraphGeneratorPage(GraphGeneratorPage toDelete)
        {
            if (toDelete == null) return;

            // remove in db
            ConnectionManager.Instance.Database.GraphGeneratorPages.Remove(toDelete);
            ConnectionManager.Instance.Database.SaveChanges();

            // update ui
            GraphGeneratorPages.Remove(toDelete);
        }

        public void EditGraphGeneratorPageName()
        {
            SelectedGraphGeneratorPage.Name = NewGraphGeneratorPageName;
            ConnectionManager.Instance.Database.SaveChanges();
        }

        #endregion
    }

    public record NodeTypeCategory(string Name, string ToolTip, bool IsExpanded, NodeType[] NodeTypes);
    public record NodeType(string Name, string ToolTip, Type Type);
}
