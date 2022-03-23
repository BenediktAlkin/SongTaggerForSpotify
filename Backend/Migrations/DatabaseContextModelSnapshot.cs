﻿// <auto-generated />
using System;
using Backend;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("ArtistGenre", b =>
                {
                    b.Property<string>("ArtistsId")
                        .HasColumnType("TEXT");

                    b.Property<int>("GenresId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ArtistsId", "GenresId");

                    b.HasIndex("GenresId");

                    b.ToTable("ArtistGenre");
                });

            modelBuilder.Entity("ArtistTrack", b =>
                {
                    b.Property<string>("ArtistsId")
                        .HasColumnType("TEXT");

                    b.Property<string>("TracksId")
                        .HasColumnType("TEXT");

                    b.HasKey("ArtistsId", "TracksId");

                    b.HasIndex("TracksId");

                    b.ToTable("ArtistTrack");
                });

            modelBuilder.Entity("Backend.Entities.Album", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("ReleaseDatePrecision")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Albums");
                });

            modelBuilder.Entity("Backend.Entities.Artist", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("Backend.Entities.AudioFeatures", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<float>("Acousticness")
                        .HasColumnType("REAL");

                    b.Property<float>("Danceability")
                        .HasColumnType("REAL");

                    b.Property<float>("Energy")
                        .HasColumnType("REAL");

                    b.Property<float>("Instrumentalness")
                        .HasColumnType("REAL");

                    b.Property<int>("Key")
                        .HasColumnType("INTEGER");

                    b.Property<float>("Liveness")
                        .HasColumnType("REAL");

                    b.Property<float>("Loudness")
                        .HasColumnType("REAL");

                    b.Property<int>("Mode")
                        .HasColumnType("INTEGER");

                    b.Property<float>("Speechiness")
                        .HasColumnType("REAL");

                    b.Property<float>("Tempo")
                        .HasColumnType("REAL");

                    b.Property<int>("TimeSignature")
                        .HasColumnType("INTEGER");

                    b.Property<float>("Valence")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("AudioFeatures");
                });

            modelBuilder.Entity("Backend.Entities.Genre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("Backend.Entities.GraphGeneratorPage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("GraphGeneratorPages");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.GraphNode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("GraphGeneratorPageId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("X")
                        .HasColumnType("REAL");

                    b.Property<double>("Y")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("GraphGeneratorPageId");

                    b.ToTable("GraphNodes");

                    b.HasDiscriminator<string>("Discriminator").HasValue("GraphNode");
                });

            modelBuilder.Entity("Backend.Entities.Playlist", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Playlists");

                    b.HasData(
                        new
                        {
                            Id = "All",
                            Name = "All"
                        },
                        new
                        {
                            Id = "Liked Songs",
                            Name = "Liked Songs"
                        },
                        new
                        {
                            Id = "Untagged Songs",
                            Name = "Untagged Songs"
                        });
                });

            modelBuilder.Entity("Backend.Entities.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("TagGroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(1);

                    b.HasKey("Id");

                    b.HasIndex("TagGroupId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Backend.Entities.TagGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Order")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TagGroups");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "default",
                            Order = 1
                        });
                });

            modelBuilder.Entity("Backend.Entities.Track", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("AlbumId")
                        .HasColumnType("TEXT");

                    b.Property<string>("AudioFeaturesId")
                        .HasColumnType("TEXT");

                    b.Property<int>("DurationMs")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsLiked")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AlbumId");

                    b.HasIndex("AudioFeaturesId")
                        .IsUnique();

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("GraphNodeGraphNode", b =>
                {
                    b.Property<int>("InputsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OutputsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("InputsId", "OutputsId");

                    b.HasIndex("OutputsId");

                    b.ToTable("GraphNodeGraphNode");
                });

            modelBuilder.Entity("PlaylistTrack", b =>
                {
                    b.Property<string>("PlaylistsId")
                        .HasColumnType("TEXT");

                    b.Property<string>("TracksId")
                        .HasColumnType("TEXT");

                    b.HasKey("PlaylistsId", "TracksId");

                    b.HasIndex("TracksId");

                    b.ToTable("PlaylistTrack");
                });

            modelBuilder.Entity("TagTrack", b =>
                {
                    b.Property<int>("TagsId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TracksId")
                        .HasColumnType("TEXT");

                    b.HasKey("TagsId", "TracksId");

                    b.HasIndex("TracksId");

                    b.ToTable("TagTrack");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AssignTagNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.Property<int?>("TagId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("AssignTagNode_TagId");

                    b.HasIndex("TagId");

                    b.HasDiscriminator().HasValue("AssignTagNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterKeyNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.Property<int?>("Key")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue("FilterKeyNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.ConcatNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.HasDiscriminator().HasValue("ConcatNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.DeduplicateNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.HasDiscriminator().HasValue("DeduplicateNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.FilterArtistNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.Property<string>("ArtistId")
                        .HasColumnType("TEXT");

                    b.HasIndex("ArtistId");

                    b.HasDiscriminator().HasValue("FilterArtistNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.FilterRangeNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.Property<int?>("ValueFrom")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ValueTo")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue("FilterRangeNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.FilterTagNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.Property<int?>("TagId")
                        .HasColumnType("INTEGER");

                    b.HasIndex("TagId");

                    b.HasDiscriminator().HasValue("FilterTagNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.FilterUntaggedNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.HasDiscriminator().HasValue("FilterUntaggedNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.IntersectNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.HasDiscriminator().HasValue("IntersectNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.PlaylistInputNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.Property<string>("PlaylistId")
                        .HasColumnType("TEXT");

                    b.HasIndex("PlaylistId");

                    b.HasDiscriminator().HasValue("PlaylistInputNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.PlaylistOutputNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.Property<string>("GeneratedPlaylistId")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlaylistName")
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue("PlaylistOutputNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.RemoveNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.GraphNode");

                    b.Property<int?>("BaseSetId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("RemoveSetId")
                        .HasColumnType("INTEGER");

                    b.HasIndex("BaseSetId");

                    b.HasIndex("RemoveSetId");

                    b.HasDiscriminator().HasValue("RemoveNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterAcousticnessNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterAcousticnessNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterDanceabilityNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterDanceabilityNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterDurationMsNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterDurationMsNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterEnergyNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterEnergyNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterInstrumentalnessNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterInstrumentalnessNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterLivenessNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterLivenessNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterLoudnessNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterLoudnessNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterModeNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterModeNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterSpeechinessNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterSpeechinessNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterTempoNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterTempoNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterTimeSignatureNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterTimeSignatureNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AudioFeaturesFilters.FilterValenceNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterValenceNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.FilterYearNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.FilterRangeNode");

                    b.HasDiscriminator().HasValue("FilterYearNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.PlaylistInputLikedNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.PlaylistInputNode");

                    b.HasDiscriminator().HasValue("PlaylistInputLikedNode");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.PlaylistInputMetaNode", b =>
                {
                    b.HasBaseType("Backend.Entities.GraphNodes.PlaylistInputNode");

                    b.HasDiscriminator().HasValue("PlaylistInputMetaNode");
                });

            modelBuilder.Entity("ArtistGenre", b =>
                {
                    b.HasOne("Backend.Entities.Artist", null)
                        .WithMany()
                        .HasForeignKey("ArtistsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Entities.Genre", null)
                        .WithMany()
                        .HasForeignKey("GenresId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ArtistTrack", b =>
                {
                    b.HasOne("Backend.Entities.Artist", null)
                        .WithMany()
                        .HasForeignKey("ArtistsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Entities.Track", null)
                        .WithMany()
                        .HasForeignKey("TracksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.GraphNode", b =>
                {
                    b.HasOne("Backend.Entities.GraphGeneratorPage", "GraphGeneratorPage")
                        .WithMany("GraphNodes")
                        .HasForeignKey("GraphGeneratorPageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GraphGeneratorPage");
                });

            modelBuilder.Entity("Backend.Entities.Tag", b =>
                {
                    b.HasOne("Backend.Entities.TagGroup", "TagGroup")
                        .WithMany("Tags")
                        .HasForeignKey("TagGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TagGroup");
                });

            modelBuilder.Entity("Backend.Entities.Track", b =>
                {
                    b.HasOne("Backend.Entities.Album", "Album")
                        .WithMany("Tracks")
                        .HasForeignKey("AlbumId");

                    b.HasOne("Backend.Entities.AudioFeatures", "AudioFeatures")
                        .WithOne("Track")
                        .HasForeignKey("Backend.Entities.Track", "AudioFeaturesId");

                    b.Navigation("Album");

                    b.Navigation("AudioFeatures");
                });

            modelBuilder.Entity("GraphNodeGraphNode", b =>
                {
                    b.HasOne("Backend.Entities.GraphNodes.GraphNode", null)
                        .WithMany()
                        .HasForeignKey("InputsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Entities.GraphNodes.GraphNode", null)
                        .WithMany()
                        .HasForeignKey("OutputsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PlaylistTrack", b =>
                {
                    b.HasOne("Backend.Entities.Playlist", null)
                        .WithMany()
                        .HasForeignKey("PlaylistsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Entities.Track", null)
                        .WithMany()
                        .HasForeignKey("TracksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TagTrack", b =>
                {
                    b.HasOne("Backend.Entities.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Entities.Track", null)
                        .WithMany()
                        .HasForeignKey("TracksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.AssignTagNode", b =>
                {
                    b.HasOne("Backend.Entities.Tag", "Tag")
                        .WithMany("AssignTagNodes")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.FilterArtistNode", b =>
                {
                    b.HasOne("Backend.Entities.Artist", "Artist")
                        .WithMany()
                        .HasForeignKey("ArtistId");

                    b.Navigation("Artist");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.FilterTagNode", b =>
                {
                    b.HasOne("Backend.Entities.Tag", "Tag")
                        .WithMany("FilterTagNodes")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.PlaylistInputNode", b =>
                {
                    b.HasOne("Backend.Entities.Playlist", "Playlist")
                        .WithMany("PlaylistInputNodes")
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Playlist");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.RemoveNode", b =>
                {
                    b.HasOne("Backend.Entities.GraphNodes.GraphNode", "BaseSet")
                        .WithMany("RemoveNodeBaseSets")
                        .HasForeignKey("BaseSetId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Backend.Entities.GraphNodes.GraphNode", "RemoveSet")
                        .WithMany("RemoveNodeRemoveSets")
                        .HasForeignKey("RemoveSetId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("BaseSet");

                    b.Navigation("RemoveSet");
                });

            modelBuilder.Entity("Backend.Entities.Album", b =>
                {
                    b.Navigation("Tracks");
                });

            modelBuilder.Entity("Backend.Entities.AudioFeatures", b =>
                {
                    b.Navigation("Track");
                });

            modelBuilder.Entity("Backend.Entities.GraphGeneratorPage", b =>
                {
                    b.Navigation("GraphNodes");
                });

            modelBuilder.Entity("Backend.Entities.GraphNodes.GraphNode", b =>
                {
                    b.Navigation("RemoveNodeBaseSets");

                    b.Navigation("RemoveNodeRemoveSets");
                });

            modelBuilder.Entity("Backend.Entities.Playlist", b =>
                {
                    b.Navigation("PlaylistInputNodes");
                });

            modelBuilder.Entity("Backend.Entities.Tag", b =>
                {
                    b.Navigation("AssignTagNodes");

                    b.Navigation("FilterTagNodes");
                });

            modelBuilder.Entity("Backend.Entities.TagGroup", b =>
                {
                    b.Navigation("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}
