# Song Tagger for Spotify

[![Release](https://github.com/BenediktAlkin/SpotifySongTagger/actions/workflows/release.yaml/badge.svg)](https://github.com/BenediktAlkin/SpotifySongTagger/actions/workflows/release.yaml)

## Setup instructions
Download either the installer (SongTaggerForSpotify-Installer.msi) or the portable version (SongTaggerForSpotify-Portable.zip) from [latest release](https://github.com/BenediktAlkin/SongTaggerForSpotify/releases). 
* Installer: run the installer --> allow unknown publisher --> click through the setup program --> run "Song Tagger for Spotify" from your desktop or start menu
* Portable: extract the zip folder --> run SpotifySongTagger.exe

If you don't have a .NET 5 runtime installed you will be asked to install it. You can download it [here](https://dotnet.microsoft.com/download/dotnet/5.0/runtime) (under "Run desktop apps" select "Download x64").

## Mac/Linux support
Currently Mac/Linux are not supported, the app only runs on Windows.

## What is song tagging?
Attaching a tag to a song allows you more flexibility when managing songs you like. A tag can be anything you'd like it to be: music genres, language of the lyrics, the event/movie where you discovered a song or how much you like a song. 
For example my music library looks like [this](https://raw.githubusercontent.com/BenediktAlkin/SongTaggerForSpotify/main/Examples/QuotaExtensionApplication/SongTagger.png).

## What is the benefit?
Adding tags to songs allows you to organize your library better. With Song Tagger for Spotify you can create "Playlist Generators" which lets you to combine songs/playlists from your library and modify them based on tags or other metadata (e.g. release date, artist) as you like.

Some [examples](https://github.com/BenediktAlkin/SpotifySongTagger#Examples) are shown below. 


## Examples
* Import tags based on existing playlists 
  * Gets you started with Song Tagger for Spotify without manually tagging your whole library.
  * e.g. assign every song from the "Chill" playlist the "chill" tag   
![Import tags example](https://github.com/BenediktAlkin/SpotifySongTagger/blob/main/Examples/import%20tags%20from%20playlist.png)

* Create dynamic playlists based on tags 
  * Your library consists of a broad range of music? Create specific playlists from your library without manually updating them everytime you add a new song to your library.
  * e.g. create a 90s, 90s rock, 90s pop and 90s dance playlist from your whole library
![Tag based playlists example](https://github.com/BenediktAlkin/SpotifySongTagger/blob/main/Examples/90s.png)

* Alter liked playlists
  * You like a playlist but dislike some songs in it? Assign a tag to those songs and create your own version of the playlist. Added/removed songs from the original playlist will be synchronized to your cloned version (this is a major advantage over just copying the playlist and changing it).
  * e.g. filter overplayed songs from a friend's playlist
![Alter liked playlists example](https://github.com/BenediktAlkin/SpotifySongTagger/blob/main/Examples/alter%20public%20playlist.png)

* Simplify discovering new music
  * You use multiple playlists to discover new music? Combine them and filter out songs that you already know.
  * e.g. combine "Release Radar" and "Discover Weekly" (playlists that are created by Spotify specifically for you and refresh every week with new music) together and filter out songs you already liked/tagged
![Combine discover example](https://github.com/BenediktAlkin/SpotifySongTagger/blob/main/Examples/combine%20discover.png)
  * e.g. combine Spotify's "Top 50" playlists, remove duplicates and songs you already know
![Combine charts example](https://github.com/BenediktAlkin/SpotifySongTagger/blob/main/Examples/combine%20charts.png)

## Song Metadata
Spotify provides some metadata for every song (BPM, danceability, energy, ...). You can take a look at the metadata of your library.
![View song metadata](https://github.com/BenediktAlkin/SpotifySongTagger/blob/main/Examples/QuotaExtensionApplication/MetadataViewer.png)

You can also use this metadata in the Playlist Generator to organize your music library.
![Create new playlists based on song metadata](https://github.com/BenediktAlkin/SpotifySongTagger/blob/main/Examples/high_energy.png)

## Backup/Restore/Delete your Tags/PlaylistGenerators
All data is stored in a local database called "<SPOTIFY_USERNAME>.sqlite" which is located by default either in "C:\Users\\<WINDOWS_USER>\AppData\Roaming\Song Tagger for Spotify" (if you installed the program) or in the directory of your portable version.

You can change the path where your database file is located in the app. The database file name is tied to your spotify username and **can't** be changed.
Changing the path where your database file is located will:
* copy the current database into the new directory if there does not already exist a file called "<SPOTIFY_USERNAME>.sqlite" in the new directory
* use the existing database file in the new directory if there already exists a file called "<SPOTIFY_USERNAME>.sqlite" in the new directory. The old database file will remain in the old directory.

Only the database file of the logged in user will be copied (important if you use the app with multiple users).

You can easily use Song Tagger for Spotify on multiple devices by setting up some filesharing service (OneDrive, Google Drive, Dropbox, ...) and changing the database path to some synchronized folder.

## Limitations
* Playlist folders are not supported by the [Spotify API](https://developer.spotify.com/documentation/general/guides/working-with-playlists/#folders)
  * Liked playlists can only be viewed unorganized. They can still be tagged normally but do not appear in any folder structure (e.g. if you have a folder called "Discover" with the playlist "Discover Weekly" it will be shown only as "Discover Weekly" instead of "Discover/Discover Weekly")
  * Generated playlists have to be moved manually to a folder if desired (only required once)
    * The first run of a playlist generator will always create the playlist in the root directory of your Spotify library
    * When you move it to a folder, subsequent runs of a playlist generator will update the playlist normally but will not change the location.
* Playing songs from the song tagger application requires an active Spotify player (e.g. [Spotify Player](https://www.spotify.com/us/download/other/) or [Spotify Web Player](https://open.spotify.com/))
* Only Spotify Premium users can play songs from the song tagger application (the currently playing song is still displayed for non-premium users)
* [local files](https://support.spotify.com/us/article/local-files/) are not supported by the [Spotify API](https://developer.spotify.com/documentation/general/guides/local-files-spotify-playlists/#limitations)
* [unavailable songs](https://community.spotify.com/t5/iOS-iPhone-iPad/Song-unavailable/td-p/4816227#:~:text=The%20greyed%20out%20tracks%20just,to%20the%20individual%20music%20companies.) are currently not supported

## API
A HTTP API is available and documented [here](https://github.com/BenediktAlkin/SongTaggerForSpotify/blob/main/BackendAPI/documentation.md).

## Tagging in Spotify Desktop
It is also possible to tag stuff straight from the Spotify context menu. 
Check [Tagify](https://github.com/vokinpirks/Tagify) out.

## Donate
Song Tagger for Spotify is free and open source! 

If you like this project, please consider supporting it.

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/donate?hosted_button_id=9RBNSGWNNQ57C)
