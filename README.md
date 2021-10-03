Song tagger for Spotify

[![Release](https://github.com/BenediktAlkin/SpotifySongTagger/actions/workflows/release.yaml/badge.svg)](https://github.com/BenediktAlkin/UpdaterTest/actions/workflows/release.yaml)

Limitations
* Playlist folders are [not supported](https://developer.spotify.com/documentation/general/guides/working-with-playlists/#folders)
  * Existing playlists can only be viewed unorganized
  * Generated playlists have to be moved manually to a folder if desired (only required once)
* Playing songs from the song tagger application requires an active Spotify player (e.g. [Spotify Player](https://www.spotify.com/us/download/other/) or [Spotify Web Player](https://open.spotify.com/))
* Only Spotify Premium users can play songs from the song tagger application (the currently playing song is still displayed for non-premium users)
* [local files](https://support.spotify.com/us/article/local-files/) are [not supported](https://developer.spotify.com/documentation/general/guides/local-files-spotify-playlists/#limitations)
* [unavailable songs](https://community.spotify.com/t5/iOS-iPhone-iPad/Song-unavailable/td-p/4816227#:~:text=The%20greyed%20out%20tracks%20just,to%20the%20individual%20music%20companies.) are currently not supported