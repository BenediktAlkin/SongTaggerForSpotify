﻿using Backend;
using Backend.Entities;
using Backend.Entities.GraphNodes;
using Backend.Entities.GraphNodes.AudioFeaturesFilters;
using MaterialDesignThemes.Wpf;
using Serilog;
using SpotifySongTagger.Converters;
using SpotifySongTagger.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SpotifySongTagger.ViewModels
{
    public class PlaylistGeneratorViewModel : BaseViewModel
    {
        private ISnackbarMessageQueue MessageQueue { get; set; }
        public PlaylistGeneratorViewModel(ISnackbarMessageQueue messageQueue)
        {
            MessageQueue = messageQueue;
        }

        public async Task Init()
        {
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
                new NodeType("Meta Playlist Input", "all songs of a meta playlist", typeof(PlaylistInputMetaNode)),
                new NodeType("Liked Playlist Input", "all songs of a liked playlist", typeof(PlaylistInputLikedNode)),
            }),
            new NodeTypeCategory("Outputs", "generate an output", true, new[]
            {
                new NodeType("Playlist Output", "generate a playlist", typeof(PlaylistOutputNode)),
                new NodeType("Assign Tag", "assign a tag", typeof(AssignTagNode)),
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
            new NodeTypeCategory("Advanced Filters", "remove all songs that do not match a filter", false, new[]
            {
                new NodeType("Duration", "filter by duration", typeof(FilterDurationMsNode)),
                new NodeType("Genre", "filter by genre", typeof(FilterGenreNode)),
                new NodeType("Tempo (BPM)", "filter by Tempo", typeof(FilterTempoNode)),

                new NodeType("Danceability", "filter by danceability", typeof(FilterDanceabilityNode)),
                new NodeType("Energy", "filter by energy", typeof(FilterEnergyNode)),
                new NodeType("Instrumentalness", "filter by instrumentalness", typeof(FilterInstrumentalnessNode)),
                new NodeType("Liveness", "filter by liveness", typeof(FilterLivenessNode)),
                new NodeType("Speechiness", "filter by speechiness", typeof(FilterSpeechinessNode)),
                new NodeType("Valence", "filter by valence", typeof(FilterValenceNode)),
                
                new NodeType("Acousticness", "filter by acousticness", typeof(FilterAcousticnessNode)),
                new NodeType("Loudness", "filter by loudness", typeof(FilterLoudnessNode)),
                
                new NodeType("Key", "filter by key", typeof(FilterKeyNode)),
                new NodeType("Mode", "filter by mode", typeof(FilterModeNode)),
                new NodeType("Time Signature", "filter by time signature", typeof(FilterTimeSignatureNode)),
            }),
        };
        #endregion

        #region GraphGeneratorPages header
        private bool isRunningAll;
        public bool IsRunningAll
        {
            get => isRunningAll;
            set => SetProperty(ref isRunningAll, value, nameof(IsRunningAll));
        }
        public async Task RunAll()
        {
            if (IsRunningAll) return;

            Log.Information("RunAll GraphGeneratorPages");
            IsRunningAll = true;
            foreach (var ggp in BaseViewModel.DataContainer.GraphGeneratorPages)
                ggp.IsRunning = true;

            foreach (var ggp in BaseViewModel.DataContainer.GraphGeneratorPages)
                await Run(ggp);

            IsRunningAll = false;
            Log.Information("Finished RunAll GraphGeneratorPages");
        }
        public async Task Run(GraphGeneratorPage ggp)
        {
            ggp.IsRunning = true;
            Log.Information($"Run page {ggp.Name}");
            var runnableNodes = await Task.Run(() => DatabaseOperations.GetRunnableGraphNodes(ggp));

            foreach (var runnableNode in runnableNodes)
            {
                var success = await runnableNode.Run();
                if (!success)
                {
                    var converter = new GraphNodeToNameConverter();
                    var text1 = new TextBlock { Text = "can't run " };
                    var nodeStr = (string)converter.Convert(runnableNode, typeof(string), null, CultureInfo.CurrentUICulture);
                    var text2 = new TextBlock { Text = nodeStr, FontWeight = FontWeights.Bold };
                    var text3 = new TextBlock { Text = " (graph contains invalid node)" };
                    text1.Inlines.Add(text2);
                    text1.Inlines.Add(text3);
                    MessageQueue.Enqueue(text1);
                }
            }

            ggp.IsRunning = false;
            Log.Information($"Finished page {ggp.Name}");
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
            DatabaseOperations.AddGraphGeneratorPage(page);

            // update ui
            BaseViewModel.DataContainer.GraphGeneratorPages.Add(page);
            NewGraphGeneratorPageName = null;
        }
        #endregion


        #region GraphGeneratorPages
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
        public void RemoveGraphGeneratorPage(GraphGeneratorPage page)
        {
            // remove in db
            if (DatabaseOperations.DeleteGraphGeneratorPage(page))
            {
                // update ui
                BaseViewModel.DataContainer.GraphGeneratorPages.Remove(page);
            }

        }
        public void EditGraphGeneratorPageName(GraphGeneratorPage page)
        {
            // edit in db
            if (DatabaseOperations.EditGraphGeneratorPage(page, NewGraphGeneratorPageName))
            {
                // update ui
                page.Name = NewGraphGeneratorPageName;
            }

            NewGraphGeneratorPageName = null;
        }
        #endregion
    }

    public record NodeTypeCategory(string Name, string ToolTip, bool IsExpanded, NodeType[] NodeTypes);
    public record NodeType(string Name, string ToolTip, Type Type);
}
