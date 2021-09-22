﻿using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;

namespace Backend
{
    public class DataContainer : NotifyPropertyChangedBase
    {
        public static DataContainer Instance { get; } = new DataContainer();
        private DataContainer() { }

        public void Clear()
        {
            User = null;
            SourcePlaylists = null;
            Tags = null;
        }


        // spotify data
        private PrivateUser user;
        public PrivateUser User
        {
            get => user;
            set => SetProperty(ref user, value, nameof(User));
        }


        // source playlists
        private bool isLoadingSourcePlaylists;
        public bool IsLoadingSourcePlaylists
        {
            get => isLoadingSourcePlaylists;
            set => SetProperty(ref isLoadingSourcePlaylists, value, nameof(IsLoadingSourcePlaylists));
        }
        private List<Playlist> sourcePlaylists;
        public List<Playlist> SourcePlaylists
        {
            get => sourcePlaylists;
            set => SetProperty(ref sourcePlaylists, value, nameof(SourcePlaylists));
        }
        public async Task LoadSourcePlaylists(bool forceReload = false)
        {
            if (!forceReload && sourcePlaylists != null) return;

            IsLoadingSourcePlaylists = true;
            SourcePlaylists = await DatabaseOperations.SourcePlaylistCurrentUsers();
            IsLoadingSourcePlaylists = false;
        }


        // all tags in the database
        private bool isLoadingTags;
        public bool IsLoadingTags
        {
            get => isLoadingTags;
            set => SetProperty(ref isLoadingTags, value, nameof(IsLoadingTags));
        }
        private ObservableCollection<Tag> tags;
        public ObservableCollection<Tag> Tags
        {
            get => tags;
            set => SetProperty(ref tags, value, nameof(Tags));
        }
        public async Task LoadTags()
        {
            if (Tags != null) return;

            IsLoadingTags = true;
            var dbTags = await ConnectionManager.Instance.Database.Tags.ToListAsync();
            Tags = new ObservableCollection<Tag>(dbTags);
            IsLoadingTags = false;
        }


        // all output nodes
        private ObservableCollection<OutputNode> outputNodes;
        public ObservableCollection<OutputNode> OutputNodes
        {
            get => outputNodes;
            set => SetProperty(ref outputNodes, value, nameof(OutputNodes));
        }
        public async Task LoadOutputNodes()
        {
            if (OutputNodes != null) return;

            var dbNodes = await ConnectionManager.Instance.Database.OutputNodes.ToListAsync();
            OutputNodes = new ObservableCollection<OutputNode>(dbNodes);
        }


    }
}
