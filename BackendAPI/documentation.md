# API documentation

Running "BackendAPI.exe" will start a HTTP server on http://localhost:63848/ with the following endpoints.

## JSON format
All endpoints described here use text/plain as return type. The return value is wrapped into a json object when an endpoint is prefixed with json/.
E.g. If the endpoint GET connection/userid returns *exampleuserid* the endpoint GET json/connection/userid will return *{"result":"exampleuserid"}*

## Connection
The API does need access to the SpotifyAPI. Upon starting the API it will check if a user is already logged in. If no user is logged in it will open a browser window for the user to login. The login for the API is independent of the login for the main app. A user remains logged into the API until the logout endpoint is called.

### GET connection/login
Open a login page in the default browser.

### GET connection/logout
Logs the current user out.

### GET connection/userid
**Returns** string: The userid of the currently logged in user. Null if no user is logged in.

## Tag
### GET tags
**Returns** array of strings: All available tags.

### GET taggroups
**Returns** dictionary of string to array of strings: Taggroups in dictionary form. As taggroups are not yet implemented the dictionary contains only one entry "default" with all tags in it.


## Album
### POST tags/{tag}/album?id={}
Assign a tag to all tracks in an album.
**tag** string: The tag to assign
**id** string: The [SpotifyId](https://developer.spotify.com/documentation/web-api/#spotify-uris-and-ids) of the album

**Returns** array of booleans: A boolean array with a length equivalent to the number of tracks in the album. Each value indicates whether or not the track was successfully tagged. Null if an invalid id was provided.

### DELETE tags/{tag}/album?id={}
Remove a tag from all tracks in an album.
**tag** string: The tag to remove
**id** string: The [SpotifyId](https://developer.spotify.com/documentation/web-api/#spotify-uris-and-ids) of the album

**Returns** array of booleans: A boolean array with a length equivalent to the number of tracks in the album. Each value indicates whether or not the tag was successfully removed. Null if an invalid id was provided.

### GET tags/{tag}/album?id={}
Check if all tracks of the album are tagged with a specific tag.
**tag** string: The tag to check for
**id** string: The [SpotifyId](https://developer.spotify.com/documentation/web-api/#spotify-uris-and-ids) of the album

**Returns** string: "ALL", "SOME" or "NONE"


## Playlist
### POST tags/{tag}/playlist?id={}
Assign a tag to all tracks in a playlist.
**tag** string: The tag to assign
**id** string: The [SpotifyId](https://developer.spotify.com/documentation/web-api/#spotify-uris-and-ids) of the playlist

**Returns** array of booleans: A boolean array with a length equivalent to the number of tracks in the playlist. Each value indicates whether or not the track was successfully tagged. Null if an invalid id was provided.

### DELETE tags/{tag}/playlist?id={}
Remove a tag from all tracks in an playlist.
**tag** string: The tag to remove
**id** string: The [SpotifyId](https://developer.spotify.com/documentation/web-api/#spotify-uris-and-ids) of the playlist

**Returns** array of booleans: A boolean array with a length equivalent to the number of tracks in the playlist. Each value indicates whether or not the tag was successfully removed. Null if an invalid id was provided.

### GET tags/{tag}/playlist?id={}
Check if all tracks of the playlist are tagged with a specific tag.
**tag** string: The tag to check for.
**id** string: The [SpotifyId](https://developer.spotify.com/documentation/web-api/#spotify-uris-and-ids) of the playlist

**Returns** string: "ALL", "SOME" or "NONE"

## Track
### POST tags/{tag}/tracks?id={}&id={}&...
Assign a tag to one or many tracks.
**tag** string: The tag to assign
**id** array of strings: [SpotifyId](https://developer.spotify.com/documentation/web-api/#spotify-uris-and-ids)s of tracks

**Returns** array of booleans: A boolean array with a length equivalent to the number of ids. Each value indicates whether or not the track was successfully tagged. If an invalid id is provided, the corresponding success value is false.

### DELETE tags/{tag}/tracks?id={}&id={}&...
Remove a tag from one or many tracks.
**tag** string: The tag to remove
**id** array of strings: [SpotifyId](https://developer.spotify.com/documentation/web-api/#spotify-uris-and-ids)s of tracks

**Returns** array of booleans: A boolean array with a length equivalent to the number of tracks in the playlist. Each value indicates whether or not the tag was successfully removed. If an invalid id is provided, the corresponding success value is false.

### GET tags/{tag}/tracks?id={}&id={}&...
Check if all provided tracks are tagged with a specific tag.
**tag** string: The tag to check for
**id** array of strings: [SpotifyId](https://developer.spotify.com/documentation/web-api/#spotify-uris-and-ids)s of tracks

**Returns** string: "ALL", "SOME" or "NONE"