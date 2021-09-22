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
            builder.Entity<ConcatNode>().HasDiscriminator<string>("Discriminator").HasValue("ConcatNode");
            builder.Entity<DeduplicateNode>().HasDiscriminator<string>("Discriminator").HasValue("DeduplicateNode");
            builder.Entity<TagFilterNode>().HasDiscriminator<string>("Discriminator").HasValue("TagFilterNode");
        }


        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<GraphNode> GraphNodes { get; set; }
        public DbSet<OutputNode> OutputNodes { get; set; }
        public DbSet<InputNode> InputNodes { get; set; }
        public DbSet<GraphGeneratorPage> GraphGeneratorPages { get; set; }
    }
}
