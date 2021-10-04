using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;

namespace Backend
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options, bool dropDb = false) : base(options)
        {
            if (dropDb)
                Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // if a inherited type of an abstract base class has no additional properties 
            // no discriminator is set to distinguish between the classes
            // add all inherited types here because some inherited types use the same 
            // additional properties which results in the "second" one being identified as 
            // graph node (e.g. AssignTagNode and FilterTagNode both have TagId as additional property
            // resulting in AssignTagNode being identified as GraphNode)
            
            void RegisterInheritedType<T>() where T: class =>
                builder.Entity<T>().HasDiscriminator<string>("Discriminator").HasValue(typeof(T).Name);
                

            RegisterInheritedType<AssignTagNode>();
            RegisterInheritedType<ConcatNode>();
            RegisterInheritedType<DeduplicateNode>();
            RegisterInheritedType<FilterArtistNode>();
            RegisterInheritedType<FilterTagNode>();
            RegisterInheritedType<FilterYearNode>();
            RegisterInheritedType<IntersectNode>();
            RegisterInheritedType<LikedPlaylistInputNode>();
            RegisterInheritedType<MetaPlaylistInputNode>();
            RegisterInheritedType<PlaylistOutputNode>();
            RegisterInheritedType<RemoveNode>();

            // "All" and "Untagged Songs" need to be in db for PlaylistInputNode to store reference
            foreach(var metaPlaylistId in Constants.META_PLAYLIST_IDS)
                builder.Entity<Playlist>().HasData(new Playlist { Id = metaPlaylistId, Name = metaPlaylistId });
        }


        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<GraphNode> GraphNodes { get; set; }
        public DbSet<PlaylistOutputNode> PlaylistOutputNodes { get; set; }
        public DbSet<GraphGeneratorPage> GraphGeneratorPages { get; set; }
    }
}
