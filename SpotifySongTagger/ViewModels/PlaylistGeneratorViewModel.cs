using Backend;
using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;
using Serilog;
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
            await DataContainer.LoadGraphGeneratorPages();
            IsReady = true;
        }

        // PlaylistGenerator is ready when GraphGeneratorPages and SourcePlaylists are loaded
        private bool isReady;
        public bool IsReady
        {
            get => isReady;
            set => SetProperty(ref isReady, value, nameof(IsReady));
        }


        #region NodeTypes
        public List<NodeTypeCategory> NodeTypeCategories { get; } = new()
        {
            new NodeTypeCategory("Inputs", "provide songs for other nodes", true, new[]
            {
                new NodeType("Meta Playlist Input", "all songs of a meta playlist", typeof(MetaPlaylistInputNode)),
                new NodeType("Liked Playlist Input", "all songs of a liked playlist", typeof(LikedPlaylistInputNode)),
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
                new NodeType("Untagged", "filter for untagged songs", typeof(FilterUntaggedNode)),
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
        public void AddGraphGeneratorPage()
        {
            if (string.IsNullOrEmpty(NewGraphGeneratorPageName))
            {
                NewGraphGeneratorPageName = null;
                return;
            }

            // add to db
            var page = new GraphGeneratorPage { Name = NewGraphGeneratorPageName };
            var node = new PlaylistOutputNode { PlaylistName = NewGraphGeneratorPageName };
            page.GraphNodes.Add(node);
            DatabaseOperations.AddGraphGeneratorPage(page);

            // update ui
            BaseViewModel.DataContainer.GraphGeneratorPages.Add(page);
            NewGraphGeneratorPageName = null;
        }
        public void RemoveGraphGeneratorPage(GraphGeneratorPage page)
        {
            if (SelectedGraphGeneratorPage == null) return;

            // remove in db
            DatabaseOperations.DeleteGraphGeneratorPage(page);

            // update ui
            BaseViewModel.DataContainer.GraphGeneratorPages.Remove(page);
        }

        public async Task RunAll()
        {
            if (IsRunningAll) return;
            Log.Information("RunAll GraphGeneratorPages");
            IsRunningAll = true;
            foreach (var ggpVM in BaseViewModel.DataContainer.GraphGeneratorPages)
                ggpVM.IsRunning = true;
            foreach (var ggpVM in BaseViewModel.DataContainer.GraphGeneratorPages)
                await ggpVM.Run();
            IsRunningAll = false;
            Log.Information("Finished RunAll GraphGeneratorPages");
        }

        public void EditGraphGeneratorPageName(GraphGeneratorPage page)
        {
            DatabaseOperations.EditGraphGeneratorPage(page, NewGraphGeneratorPageName);
            NewGraphGeneratorPageName = null;
        }

        #endregion
    }

    public record NodeTypeCategory(string Name, string ToolTip, bool IsExpanded, NodeType[] NodeTypes);
    public record NodeType(string Name, string ToolTip, Type Type);
}
