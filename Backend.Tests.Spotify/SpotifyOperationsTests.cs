using Backend.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Tests.Spotify
{
    public class SpotifyOperationsTests : BaseTests
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            ConnectionManager.TryInitFromSavedToken().Wait();
        }

        public record TrackValues(string Id, string TrackName, int DurationInSeconds, string AlbumName, string[] ArtistNames);
        public static void AssertTrack(Track track, TrackValues trackValues)
        {
            // track fields
            Assert.AreEqual(trackValues.Id, track.Id);
            Assert.AreEqual(trackValues.TrackName, track.Name);
            // display in spotify client is inconsistent (sometimes it is rounded, sometimes not)
            Assert.IsTrue(Math.Abs(track.DurationMs / 1000 - trackValues.DurationInSeconds) <= 1);
            Assert.IsFalse(track.IsLiked);

            // artist fields
            Assert.AreEqual(trackValues.ArtistNames.Length, track.Artists.Count);
            foreach (var artist in track.Artists)
                Assert.IsTrue(trackValues.ArtistNames.Contains(artist.Name));

            // album fields
            Assert.AreEqual(trackValues.AlbumName, track.Album.Name);
        }



        public static readonly List<TrackValues> GetTrackTestCases = new()
        {
            new TrackValues("72PFP54TZ9Tpj9dYQcb46D", "NIGHTRIDER", 2 * 60 + 36, "NIGHTRIDER", new[] { "Arizona Zervas" }),
            new TrackValues("0u49ctvusRpTB7Zr0q3igQ", "Puppastemning", 2 * 60 + 7, "Puppastemning", new[] { "Spårtsklubben" }),
        };
        [Test]
        [TestCaseSource(nameof(GetTrackTestCases))]
        public async Task GetTrack(TrackValues trackValues)
        {
            var track = await SpotifyOperations.GetTrack(trackValues.Id);
            AssertTrack(track, trackValues);
        }
        [Test]
        public async Task GetTrack_InvalidId()
        {
            Assert.IsNull(await SpotifyOperations.GetTrack("asdf"));
        }

        public static readonly List<TestCaseData> GetAlbumTracksTestCases = new()
        {
            new(
                "2spmDdYRIU22MFnLwdrRnr",
                "Start Over (Frank Pole Remix)",
                new TrackValues[]{
                    new("6lNN3selw7hsqK8fejqhD8", "Start Over - Frank Pole Remix", 2*60 + 28, "Start Over (Frank Pole Remix)", new[]{ "Ellis", "Laura Brehm", "Frank Pole" }),
                }),
            new(
                "7qtQYJc0H6s3CK4c7Gp8GR",
                "Party Rock",
                new TrackValues[]{
                    new("6H1Y824JIWiZenL9FFvUli", "Rock The BeaT", 0*60+54, "Party Rock", new[]{"LMFAO" }),
                    new("1vaUxfrERQIQfJ1wBTmeRE", "I'm In Miami Bitch", 3*60+47, "Party Rock", new[]{"LMFAO" }),
                    new("1AYi8wbamIha0MYzNie76B", "Get Crazy", 3*60+45, "Party Rock", new[]{"LMFAO" }),
                    new("5EJHJJt2VBC9YO4zag5apt", "Lil' Hipster Girl", 3*60+22, "Party Rock", new[]{"LMFAO" }),
                    new("2sPva3d85R7yKf60y7QZpD", "La La La", 3*60+30, "Party Rock", new[]{"LMFAO" }),
                    new("6MOfmymxyl9ijzbnQlvye0", "What Happens At The Party", 5*60+55, "Party Rock", new[]{"LMFAO" }),
                    new("6GyPh2TPJIQ6mToiebnBHJ", "Leaving U 4 The Groove", 3*60+32, "Party Rock", new[]{"LMFAO" }),
                    new("70OC1IMKp3ChAc7NRTgLgv", "I Don't Wanna Be", 3*60+38, "Party Rock", new[]{"LMFAO" }),
                    new("1V4jC0vJ5525lEF1bFgPX2", "Shots", 3*60+42, "Party Rock", new[]{"LMFAO", "Lil Jon" }),
                    new("5DppEsBHoSfg6NqpCsH6ip", "Bounce", 4*60+03, "Party Rock", new[]{"LMFAO" }),
                    new("4miHuTeLxZ0oycAJYxqXfG", "I Shake, I Move", 3*60+04, "Party Rock", new[]{"LMFAO" }),
                    new("3GeLgbCptOAJ6RqtWvaOOX", "I Am Not A Whore", 3*60+15, "Party Rock", new[]{"LMFAO" }),
                    new("0Rdfu7NQubmGmYz90usRCU", "Yes", 3*60+03, "Party Rock", new[]{"LMFAO" }),
                    new("2tok19xM2tiPffZtllhPdC", "Scream My Name", 4*60+18, "Party Rock", new[]{"LMFAO" }),
                }),
        };
        [Test]
        [TestCaseSource(nameof(GetAlbumTracksTestCases))]
        public async Task GetAlbumTracks(string id, string albumName, TrackValues[] tracksValues)
        {
            var tracks = await SpotifyOperations.GetAlbumTracks(id);
            Assert.AreEqual(tracksValues.Length, tracks.Count);
            foreach (var track in tracks)
            {
                var trackValues = tracksValues.FirstOrDefault(tvs => tvs.Id == track.Id);
                Assert.IsNotNull(trackValues);
                AssertTrack(track, trackValues);
            }
        }
        [Test]
        public async Task GetAlbumTracks_InvalidId()
        {
            Assert.AreEqual(0, (await SpotifyOperations.GetAlbumTracks("asdf")).Count);
        }

        public static readonly List<TestCaseData> GetPlaylistTracksTestCases = new()
        {
            new(
                "5SaUvAgJ689IhvIsJvuewf",
                new TrackValues[]{
                    new("3FAclTFfvUuQYnEsptbK8w", "Back To Black", 4*60+01, "Back To Black (Deluxe Edition)", new[]{"Amy Winehouse" }),
                    new("6OK5fKp0CGJJAu2sGegPui", "La Receta", 3*60+44, "Aquí Y Ahora", new[]{"Los Aslándticos" }),
                    new("4TsmezEQVSZNNPv5RJ65Ov", "Pon de Replay", 4*60+06, "Music Of The Sun", new[]{"Rihanna" }),
                    new("2VOomzT6VavJOGBeySqaMc", "Disturbia", 3*60+58, "Good Girl Gone Bad: Reloaded", new[]{"Rihanna" }),
                    new("0qqUNsGRZVT9dlru6Wzphn", "Drikkestopp", 3*60+06, "Drikkestopp", new[]{"Kevin Boine" }),
                    new("0JiY190vktuhSGN6aqJdrt", "So What", 3*60+35, "Funhouse (Expanded Edition)", new[]{"P!nk" }),
                }
            ),
        };
        [Test]
        [TestCaseSource(nameof(GetPlaylistTracksTestCases))]
        public async Task GetPlaylistTracks(string id, TrackValues[] tracksValues)
        {
            var tracks = await SpotifyOperations.GetPlaylistTracks(id);
            Assert.AreEqual(tracksValues.Length, tracks.Count);
            foreach (var track in tracks)
            {
                var trackValues = tracksValues.FirstOrDefault(tvs => tvs.Id == track.Id);
                Assert.IsNotNull(trackValues);
                AssertTrack(track, trackValues);
            }
        }
        [Test]
        public async Task GetPlaylistTracks_InvalidId()
        {
            Assert.AreEqual(0, (await SpotifyOperations.GetPlaylistTracks("asdf")).Count);
        }
    }
}