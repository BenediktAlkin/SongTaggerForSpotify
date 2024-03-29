﻿using Backend.Entities;
using Backend.Entities.GraphNodes;
using Backend.Entities.GraphNodes.AudioFeaturesFilters;
using Microsoft.EntityFrameworkCore;
using System;

namespace Backend
{
    public class DatabaseContext : DbContext
    {
        // parameterless constructor for migrations
        public DatabaseContext() : base(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite().Options) { }
        public DatabaseContext(DbContextOptions<DatabaseContext> options, bool ensureCreated = false, bool dropDb = false) : base(options)
        {
            if (dropDb)
                Database.EnsureDeleted();

            if (ensureCreated)
            {
                // first versions used Database.EnsureCreated instead of migrations
                // to preserve compatibility --> create a __EFMigrationsHistory table
                // if it does not exist and set it to the InitialCreate migration
                Database.OpenConnection();
                using var cmd = Database.GetDbConnection().CreateCommand();
                cmd.CommandText = "select count() from sqlite_schema where name == '__EFMigrationsHistory'";
                var migrationHistoryExists = (Int64)cmd.ExecuteScalar() == (Int64)1;
                cmd.CommandText = "select count() from sqlite_schema";
                var dbWasCreated = (Int64)cmd.ExecuteScalar() > (Int64)0;

                if(dbWasCreated && !migrationHistoryExists)
                {
                    cmd.CommandText = 
                        "CREATE TABLE '__EFMigrationsHistory' (" +
                        "'MigrationId' TEXT NOT NULL CONSTRAINT 'PK___EFMigrationsHistory' PRIMARY KEY," +
                        "'ProductVersion' TEXT NOT NULL" +
                        ")";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = 
                        "INSERT INTO '__EFMigrationsHistory' ('MigrationId', 'ProductVersion') " +
                        "VALUES ('20211110225321_InitialCreate', '5.0.11')";
                    cmd.ExecuteNonQuery();
                }

                Database.Migrate();
            }
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // if a inherited type of an abstract base class has no additional properties 
            // no discriminator is set to distinguish between the classes
            // add all inherited types here because some inherited types use the same 
            // additional properties which results in the "second" one being identified as 
            // graph node (e.g. AssignTagNode and FilterTagNode both have TagId as additional property
            // resulting in AssignTagNode being identified as GraphNode)

            void RegisterInheritedType<T>() where T : class =>
                builder.Entity<T>().HasDiscriminator<string>("Discriminator").HasValue(typeof(T).Name);

            RegisterInheritedType<AssignTagNode>();
            RegisterInheritedType<ConcatNode>();
            RegisterInheritedType<DeduplicateNode>();
            RegisterInheritedType<FilterArtistNode>();
            RegisterInheritedType<FilterTagNode>();
            RegisterInheritedType<FilterUntaggedNode>();
            RegisterInheritedType<FilterYearNode>();
            RegisterInheritedType<IntersectNode>();
            RegisterInheritedType<PlaylistInputLikedNode>();
            RegisterInheritedType<PlaylistInputMetaNode>();
            RegisterInheritedType<PlaylistOutputNode>();
            RegisterInheritedType<FilterAcousticnessNode>();
            RegisterInheritedType<FilterDanceabilityNode>();
            RegisterInheritedType<FilterDurationMsNode>();
            RegisterInheritedType<FilterEnergyNode>();
            RegisterInheritedType<FilterInstrumentalnessNode>();
            RegisterInheritedType<FilterKeyNode>();
            RegisterInheritedType<FilterLivenessNode>();
            RegisterInheritedType<FilterLoudnessNode>();
            RegisterInheritedType<FilterModeNode>();
            RegisterInheritedType<FilterSpeechinessNode>();
            RegisterInheritedType<FilterTempoNode>();
            RegisterInheritedType<FilterTimeSignatureNode>();
            RegisterInheritedType<FilterValenceNode>();
            RegisterInheritedType<FilterGenreNode>();

            // "All" and "Untagged Songs" need to be in db for PlaylistInputNode to store reference
            foreach (var metaPlaylistId in Constants.META_PLAYLIST_IDS)
                builder.Entity<Playlist>().HasData(new Playlist { Id = metaPlaylistId, Name = metaPlaylistId });

            // insert default TagGroup
            builder.Entity<TagGroup>().HasData(new TagGroup 
            { 
                Id = Constants.DEFAULT_TAGGROUP_ID, 
                Name = Constants.DEFAULT_TAGGROUP_NAME, 
                Order = Constants.DEFAULT_TAGGROUP_ID 
            });
            // every tag has the default TagGroup
            builder.Entity<Tag>().Property(t => t.TagGroupId).HasDefaultValue(Constants.DEFAULT_TAGGROUP_ID);

            // on delete cascade for TagGroup
            builder.Entity<Tag>()
                .HasOne(t => t.TagGroup)
                .WithMany(tg => tg.Tags)
                .HasForeignKey(t => t.TagGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // required because it is n:m relation
            builder.Entity<GraphNode>()
                .HasMany(gn => gn.Outputs)
                .WithMany(gn => gn.Inputs);

            // GraphNodes has ON DELETE RESTRICT constraints --> change to ON DELETE SET NULL
            builder.Entity<PlaylistInputNode>()
                .HasOne(gn => gn.Playlist)
                .WithMany(p => p.PlaylistInputNodes)
                .HasForeignKey(gn => gn.PlaylistId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<RemoveNode>()
                .HasOne(gn => gn.BaseSet)
                .WithMany(gn => gn.RemoveNodeBaseSets)
                .HasForeignKey(gn => gn.BaseSetId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<RemoveNode>()
                .HasOne(gn => gn.RemoveSet)
                .WithMany(gn => gn.RemoveNodeRemoveSets)
                .HasForeignKey(gn => gn.RemoveSetId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<AssignTagNode>()
                .HasOne(gn => gn.Tag)
                .WithMany(t => t.AssignTagNodes)
                .HasForeignKey(gn => gn.TagId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<FilterTagNode>()
                .HasOne(gn => gn.Tag)
                .WithMany(t => t.FilterTagNodes)
                .HasForeignKey(gn => gn.TagId)
                .OnDelete(DeleteBehavior.SetNull);
        }


        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagGroup> TagGroups { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<AudioFeatures> AudioFeatures { get; set; }
        public DbSet<GraphNode> GraphNodes { get; set; }
        public DbSet<GraphGeneratorPage> GraphGeneratorPages { get; set; }


        public DbSet<AssignTagNode> AssignTagNodes { get; set; }
        public DbSet<ConcatNode> ConcatNodes { get; set; }
        public DbSet<DeduplicateNode> DeduplicateNodes { get; set; }
        public DbSet<FilterArtistNode> FilterArtistNodes { get; set; }
        public DbSet<FilterTagNode> FilterTagNodes { get; set; }
        public DbSet<FilterUntaggedNode> FilterUntaggedNodes { get; set; }
        public DbSet<FilterYearNode> FilterYearNodes { get; set; }
        public DbSet<IntersectNode> IntersectNodes { get; set; }
        public DbSet<PlaylistInputLikedNode> PlaylistInputLikedNodes { get; set; }
        public DbSet<PlaylistInputMetaNode> PlaylistInputMetaNodes { get; set; }
        public DbSet<PlaylistInputNode> PlaylistInputNodes { get; set; }
        public DbSet<PlaylistOutputNode> PlaylistOutputNodes { get; set; }
        public DbSet<RemoveNode> RemoveNodes { get; set; }

        public DbSet<FilterRangeNode> FilterRangeNodes { get; set; }
        public DbSet<FilterKeyNode> FilterKeyNodes { get; set; }
        public DbSet<FilterModeNode> FilterModeNodes { get; set; }
        public DbSet<FilterTimeSignatureNode> FilterTimeSignatureNodes { get; set; }
        public DbSet<FilterGenreNode> FilterGenreNodes { get; set; }

    }
}
