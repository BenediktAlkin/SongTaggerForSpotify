namespace Backend
{
    public static class Constants
    {
        public const string LIKED_SONGS_PLAYLIST_ID = "Liked Songs";
        public const string ALL_SONGS_PLAYLIST_ID = "All";
        public const string UNTAGGED_SONGS_PLAYLIST_ID = "Untagged Songs";
        public static readonly string[] META_PLAYLIST_IDS = { Constants.ALL_SONGS_PLAYLIST_ID, Constants.LIKED_SONGS_PLAYLIST_ID, Constants.UNTAGGED_SONGS_PLAYLIST_ID };

        public const int DEFAULT_TAGGROUP_ID = 1;
        public const string DEFAULT_TAGGROUP_NAME = "default";
    }
}
