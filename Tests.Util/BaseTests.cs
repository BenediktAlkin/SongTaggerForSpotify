﻿using Backend;
using Backend.Entities;
using Backend.Entities.GraphNodes;
using NUnit.Framework;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using Util;

namespace Tests.Util
{
    public class BaseTests
    {
        private const string TEST_USER_ID = "TestUserId";

        protected bool REQUIRES_DB { get; set; }
        protected bool LOG_DB_QUERIES { get; set; }


        // convenience getters
        protected static ISpotifyClient SpotifyClient => ConnectionManager.Instance.Spotify;


        // nodes require ids
        private int idCounter;
        protected int NewId() => idCounter++;


        [SetUp]
        public virtual void SetUp()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(formatter: new LogFormatter("??"))
                .CreateLogger();

            if (REQUIRES_DB)
            {
                Action<string> logTo = LOG_DB_QUERIES ? DatabaseQueryLogger.Instance.Information : null;
                ConnectionManager.Instance.InitDb(TEST_USER_ID, dropDb:true, logTo: logTo);
            }

            idCounter = 1;
        }
        [TearDown]
        public virtual void TearDown()
        {
            Log.CloseAndFlush();
        }

        protected static void InitSpotify(
            List<FullTrack> tracks = null,
            List<FullTrack> likedTracks = null,
            List<SimplePlaylist> playlists = null,
            List<SimplePlaylist> likedPlaylists = null,
            Dictionary<string, List<FullTrack>> playlistTracks = null,
            Dictionary<string, (string, List<SimpleTrack>)> albums = null,
            PrivateUser user = null)
        {
            if (user == null)
                user = new PrivateUser { Id = TEST_USER_ID, DisplayName = "TestUserName", Country = "TestUserCountry", Product = "TestUserProduct" };
            var client = new SpotifyClientMock().SetUp(tracks, likedTracks, playlists, likedPlaylists, playlistTracks, albums, user);
            ConnectionManager.Instance.InitSpotify(client).Wait();
        }




        #region db inserts tagging
        protected static List<Tag> InsertTags(int count)
        {
            using var db = ConnectionManager.NewContext();
            var tags = Enumerable.Range(1, count).Select(i => new Tag { Name = $"tag{i}name" }).ToList();
            db.Tags.AddRange(tags);
            db.SaveChanges();

            return tags;
        }
        protected static List<TagGroup> InsertTagGroups(int count)
        {
            var tagGroups = Enumerable.Range(1, count).Select(i => new TagGroup { Name = $"tagGroup{i}name" }).ToList();
            foreach (var tagGroup in tagGroups)
                DatabaseOperations.AddTagGroup(tagGroup);

            return tagGroups;
        }
        protected static List<Artist> InsertArtist(int count)
        {
            using var db = ConnectionManager.NewContext();
            var artists = Enumerable.Range(1, count).Select(i =>
            new Artist
            {
                Id = $"artist{i}Id",
                Name = $"artist{i}Name"
            }).ToList();
            db.Artists.AddRange(artists);
            db.SaveChanges();

            return artists;
        }
        protected static List<Album> InsertAlbums(int count)
        {
            using var db = ConnectionManager.NewContext();
            var albums = Enumerable.Range(1, count).Select(i =>
            new Album
            {
                Id = $"album{i}Id",
                Name = $"album{i}Name"
            }).ToList();
            db.Albums.AddRange(albums);
            db.SaveChanges();

            return albums;
        }
        protected static List<Track> InsertTracks(int count, List<Artist> artists, List<Album> albums, bool isLiked = false)
        {
            using var db = ConnectionManager.NewContext();
            var tracks = Enumerable.Range(1, count).Select(i =>
            new Track
            {
                Id = $"track{i}Id",
                Name = $"track{i}Name",
                //Album = albums[i % albums.Count],
                Artists = new List<Artist> { artists[i % artists.Count] },
                IsLiked = isLiked,
            }).ToList();
            db.AttachRange(artists);
            db.AttachRange(albums);
            db.Tracks.AddRange(tracks);
            db.SaveChanges();
            return tracks;
        }
        protected static List<Playlist> InsertPlaylists(int count)
        {
            using var db = ConnectionManager.NewContext();
            var playlists = Enumerable.Range(1, count).Select(i =>
            new Playlist { Id = $"playlist{i}Id", Name = $"playlist{i}Name" }).ToList();
            db.Playlists.AddRange(playlists);
            db.SaveChanges();

            return playlists;
        }
        #endregion

        #region db inserts generator
        protected static List<GraphGeneratorPage> InsertGraphGeneratorPages(int count)
        {
            using var db = ConnectionManager.NewContext();
            var ggps = Enumerable.Range(1, count).Select(i =>
            new GraphGeneratorPage { Name = $"ggp{i}" }).ToList();
            db.GraphGeneratorPages.AddRange(ggps);
            db.SaveChanges();

            return ggps;
        }
        protected static List<ConcatNode> InsertGraphNodes(int count) => InsertGraphNodes<ConcatNode>(count);
        protected static List<T> InsertGraphNodes<T>(int count, Action<T> onInit = null) where T : GraphNode
        {
            using var db = ConnectionManager.NewContext();
            var nodes = Enumerable.Range(1, count).Select(i =>
            {
                var newNode = (T)Activator.CreateInstance(typeof(T));
                newNode.X = new Random().NextDouble();
                newNode.Y = new Random().NextDouble();
                newNode.GraphGeneratorPage = new GraphGeneratorPage();
                onInit?.Invoke(newNode);
                return newNode;
            }).Cast<T>().ToList();
            db.GraphNodes.AddRange(nodes);
            db.SaveChanges();
            return nodes;
        }
        #endregion

        #region create spotify objects
        protected static FullTrack NewTrack(int i)
        {
            return new FullTrack
            {
                Id = $"Track{i}Id",
                Name = $"Track{i}Name",
                DurationMs = i,
                Album = new SimpleAlbum
                {
                    Id = $"Album{i}Id",
                    Name = $"Album{i}Name",
                    ReleaseDate = "2021",
                    ReleaseDatePrecision = "year",
                },
                Artists = new List<SimpleArtist>
                    {
                        new SimpleArtist
                        {
                            Id = $"Artist{i}Id",
                            Name = $"Artist{i}Name",
                        }
                    },
                IsLocal = false,
                IsPlayable = true,
            };
        }
        protected static SimpleTrack ToSimpleTrack(FullTrack fullTrack)
        {
            return new SimpleTrack
            {
                Id = fullTrack.Id,
                Name = fullTrack.Id,
                DurationMs = fullTrack.DurationMs,
                Artists = fullTrack.Artists.ToList(),
                IsPlayable = fullTrack.IsPlayable,
            };
        }
        protected static SimplePlaylist NewPlaylist(int i)
        {
            return new SimplePlaylist
            {
                Id = $"Playlist{i}Id",
                Name = $"Playlist{i}Name",
            };
        }
        protected static SimpleAlbum NewAlbum(int i)
        {
            return new SimpleAlbum
            {
                Id = $"Album{i}Id",
                Name = $"Album{i}Name",
            };
        }
        #endregion

    }
}
