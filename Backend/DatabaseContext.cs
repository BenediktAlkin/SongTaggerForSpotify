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
            builder.Entity<AssignTagNode>().HasDiscriminator<string>("Discriminator").HasValue(nameof(AssignTagNode));
            builder.Entity<ConcatNode>().HasDiscriminator<string>("Discriminator").HasValue(nameof(ConcatNode));
            builder.Entity<DeduplicateNode>().HasDiscriminator<string>("Discriminator").HasValue(nameof(DeduplicateNode));
            builder.Entity<FilterArtistNode>().HasDiscriminator<string>("Discriminator").HasValue(nameof(FilterArtistNode));
            builder.Entity<FilterTagNode>().HasDiscriminator<string>("Discriminator").HasValue(nameof(FilterTagNode));
            builder.Entity<PlaylistInputNode>().HasDiscriminator<string>("Discriminator").HasValue(nameof(PlaylistInputNode));
            builder.Entity<PlaylistOutputNode>().HasDiscriminator<string>("Discriminator").HasValue(nameof(PlaylistOutputNode));
        }


        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<GraphNode> GraphNodes { get; set; }
        public DbSet<PlaylistOutputNode> PlaylistOutputNodes { get; set; }
        public DbSet<PlaylistInputNode> PlaylistInputNodes { get; set; }
        public DbSet<GraphGeneratorPage> GraphGeneratorPages { get; set; }
    }
}
