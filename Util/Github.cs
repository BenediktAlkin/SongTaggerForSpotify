using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Util
{

	public static class Github
	{
		private static JsonSerializerSettings SerializerOptions { get; } = new JsonSerializerSettings
		{
			ContractResolver = new DefaultContractResolver
			{
				NamingStrategy = new SnakeCaseNamingStrategy(),
			}
		};


		public static async Task<Release> CheckForUpdate(string user, string repo, Version version)
		{
			try
			{
				Log.Information($"Checking for updates (current={version})");
				var latest = await GetLatestRelease(user, repo);
				if (latest.Assets.Count > 0)
				{
					if (latest.Version?.CompareTo(version) > 0)
					{
						Log.Information($"New version available {latest.TagName}");
						return latest;
					}
					Log.Information($"No updates available");
				}
			}
			catch (Exception e)
			{
				Log.Error($"Failed to check for updates: {e.Message}");
			}
			return null;
		}
		public static async Task<Release[]> GetReleases(string user, string repo) 
			=> await MakeApiCall<Release[]>($"https://api.github.com/repos/{user}/{repo}/releases", user);
		public static async Task<Release> GetLatestRelease(string user, string repo)
			=> await MakeApiCall<Release>($"https://api.github.com/repos/{user}/{repo}/releases/latest", user);
		private static async Task<T> MakeApiCall<T>(string url, string user)
        {
            try
            {
				using var wc = new WebClient();
				wc.Headers.Add(HttpRequestHeader.UserAgent, user);
				var json = await wc.DownloadStringTaskAsync(url);
				return JsonConvert.DeserializeObject<T>(json, SerializerOptions);
			}catch(Exception e)
            {
				Log.Error($"Failed to make api call to {url}: {e.Message}");
				return default(T);
            }
		}

		public class Release
		{
			public string TagName { get; set; }

			public List<Asset> Assets { get; set; }

			public class Asset
			{
				public string BrowserDownloadUrl { get; set; }

				public string Name { get; set; }
			}

			public Version Version => Version.TryParse(TagName.Replace("v", ""), out Version v) ? v : null;
		}
	}
}
