using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SpotifySongTagger.Utils
{
    public static class UIComposer
    {
        public static TextBlock ComposeRequiresSpotifyPremiumLink()
        {
            var textBlock = new TextBlock { Text = "Requires " };
            var link = new Hyperlink() { NavigateUri = new Uri("https://www.spotify.com/us/premium/") };
            link.RequestNavigate += (sender, e) => Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.ToString(),
                UseShellExecute = true
            });
            link.Inlines.Add(new Run("Spotify Premium"));
            textBlock.Inlines.Add(link);
            return textBlock;
        }
        public static StackPanel ComposeRequiresOpenSpotifyPlayerLink()
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            // desktop link
            var spotifyDesktopLink = new Hyperlink() { NavigateUri = new Uri("https://www.spotify.com/us/download/other/") };
            spotifyDesktopLink.RequestNavigate += (sender, e) => Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.ToString(),
                UseShellExecute = true
            });
            spotifyDesktopLink.Inlines.Add(new Run("Spotify Player"));
            var spotifyDesktopText = new TextBlock();
            spotifyDesktopText.Inlines.Add(spotifyDesktopLink);
            // web player link
            var spotifyWebLink = new Hyperlink() { NavigateUri = new Uri("https://open.spotify.com/") };
            spotifyWebLink.RequestNavigate += (sender, e) => Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.ToString(),
                UseShellExecute = true
            });
            spotifyWebLink.Inlines.Add(new Run("Spotify Web Player"));
            var spotifyWebText = new TextBlock();
            spotifyWebText.Inlines.Add(spotifyWebLink);
            // texts
            var text1 = new TextBlock { Text = "Requires an Open Spotify Player (e.g. " };
            var text2 = new TextBlock { Text = " or " };
            var text3 = new TextBlock { Text = ")" };
            // compose stuff
            stackPanel.Children.Add(text1);
            stackPanel.Children.Add(spotifyDesktopText);
            stackPanel.Children.Add(text2);
            stackPanel.Children.Add(spotifyWebText);
            stackPanel.Children.Add(text3);
            return stackPanel;
        }
    }
}
