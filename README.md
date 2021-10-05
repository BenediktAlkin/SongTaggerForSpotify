# Song Tagger for Spotify

[![Release](https://github.com/BenediktAlkin/SpotifySongTagger/actions/workflows/release.yaml/badge.svg)](https://github.com/BenediktAlkin/SpotifySongTagger/actions/workflows/release.yaml)


## What is song tagging?
Attaching a tag to a song allows you more flexibility when managing songs you like. A tag can be anything you'd like it to be: music genres, language of the lyrics, the event/movie where you discovered a song or how much you like a song. 

## What is the benefit?
Adding tags to songs allows you to organize your library better. You can easily create playlists based on assigned tags. 
Song Tagger for Spotify allows you to create "Playlist Generators" which allow you to combine songs/playlists from your library and modify them based on tags or other metadata (e.g. release date, artist) as you like.

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
  * You follow multiple playlists that regularly add new music? Combine them and filter out songs that you already know.
  * e.g. combine "Release Radar" and "Discover Weekly" (playlists that are created by Spotify specifically for you and refresh every week with new music) together and filter out songs you already liked/tagged
![Alter liked playlists example](https://github.com/BenediktAlkin/SpotifySongTagger/blob/main/Examples/alter%20public%20playlist.png)


## Limitations
* Playlist folders are [not supported](https://developer.spotify.com/documentation/general/guides/working-with-playlists/#folders)
  * Existing playlists can only be viewed unorganized
  * Generated playlists have to be moved manually to a folder if desired (only required once)
* Playing songs from the song tagger application requires an active Spotify player (e.g. [Spotify Player](https://www.spotify.com/us/download/other/) or [Spotify Web Player](https://open.spotify.com/))
* Only Spotify Premium users can play songs from the song tagger application (the currently playing song is still displayed for non-premium users)
* [local files](https://support.spotify.com/us/article/local-files/) are [not supported](https://developer.spotify.com/documentation/general/guides/local-files-spotify-playlists/#limitations)
* [unavailable songs](https://community.spotify.com/t5/iOS-iPhone-iPad/Song-unavailable/td-p/4816227#:~:text=The%20greyed%20out%20tracks%20just,to%20the%20individual%20music%20companies.) are currently not supported