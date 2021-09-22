using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Util.Tests
{
    public class GithubTests : BaseTests
    {
        private const string USER = "BenediktAlkin";
        private const string REPO = "UpdaterTest";


        [Test]
        public async Task GetRelease()
        {
            var releases = await Github.GetReleases(USER, REPO);
            Assert.Greater(releases.Length, 0);
        }

        [Test]
        public async Task GetLatestRelease()
        {
            var release = await Github.GetLatestRelease(USER, REPO);
            Assert.NotNull(release);
        }

        [Test]
        public async Task CheckForUpdates()
        {
            var latestRelease = await Github.CheckForUpdate(USER, REPO, new Version(1, 0, 0, 0));
            Assert.NotNull(latestRelease);

            var latestRelease2 = await Github.CheckForUpdate(USER, REPO, latestRelease.Version);
            Assert.Null(latestRelease2);
        }
    }
}