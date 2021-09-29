﻿using Backend;
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
        public List<NodeType> NodeTypes { get; } = new()
        {
            new NodeType {Name = "Playlist Input", Type = typeof(PlaylistInputNode) },
            new NodeType {Name = "Concat", Type = typeof(ConcatNode) },
            new NodeType {Name = "Deduplicate", Type = typeof(DeduplicateNode) },
            new NodeType {Name = "Filter Artist", Type = typeof(FilterArtistNode) },
            new NodeType {Name = "Filter Tag", Type = typeof(FilterTagNode) },
            new NodeType {Name = "Playlist Output", Type = typeof(PlaylistOutputNode) },
            new NodeType {Name = "Assign Tag", Type = typeof(AssignTagNode) },
            new NodeType {Name = "Remove", Type = typeof(RemoveNode) },
        };
        public NodeType SelectedNodeType { get; set; }
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

    public class NodeType
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }
}
