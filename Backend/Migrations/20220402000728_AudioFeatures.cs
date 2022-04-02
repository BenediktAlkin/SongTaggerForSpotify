using Microsoft.EntityFrameworkCore.Migrations;

namespace Backend.Migrations
{
    public partial class AudioFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "YearTo",
                table: "GraphNodes",
                newName: "ValueTo");

            migrationBuilder.RenameColumn(
                name: "YearFrom",
                table: "GraphNodes",
                newName: "ValueFrom");

            migrationBuilder.AddColumn<string>(
                name: "AudioFeaturesId",
                table: "Tracks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GenreId",
                table: "GraphNodes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Key",
                table: "GraphNodes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mode",
                table: "GraphNodes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeSignature",
                table: "GraphNodes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AudioFeatures",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Acousticness = table.Column<float>(type: "REAL", nullable: false),
                    Danceability = table.Column<float>(type: "REAL", nullable: false),
                    Energy = table.Column<float>(type: "REAL", nullable: false),
                    Instrumentalness = table.Column<float>(type: "REAL", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Liveness = table.Column<float>(type: "REAL", nullable: false),
                    Loudness = table.Column<float>(type: "REAL", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    Speechiness = table.Column<float>(type: "REAL", nullable: false),
                    Tempo = table.Column<float>(type: "REAL", nullable: false),
                    TimeSignature = table.Column<int>(type: "INTEGER", nullable: false),
                    Valence = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArtistGenre",
                columns: table => new
                {
                    ArtistsId = table.Column<string>(type: "TEXT", nullable: false),
                    GenresId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistGenre", x => new { x.ArtistsId, x.GenresId });
                    table.ForeignKey(
                        name: "FK_ArtistGenre_Artists_ArtistsId",
                        column: x => x.ArtistsId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistGenre_Genres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AudioFeaturesId",
                table: "Tracks",
                column: "AudioFeaturesId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GraphNodes_GenreId",
                table: "GraphNodes",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistGenre_GenresId",
                table: "ArtistGenre",
                column: "GenresId");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphNodes_Genres_GenreId",
                table: "GraphNodes",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_AudioFeatures_AudioFeaturesId",
                table: "Tracks",
                column: "AudioFeaturesId",
                principalTable: "AudioFeatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GraphNodes_Genres_GenreId",
                table: "GraphNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_AudioFeatures_AudioFeaturesId",
                table: "Tracks");

            migrationBuilder.DropTable(
                name: "ArtistGenre");

            migrationBuilder.DropTable(
                name: "AudioFeatures");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_AudioFeaturesId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_GraphNodes_GenreId",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "AudioFeaturesId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "GenreId",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "TimeSignature",
                table: "GraphNodes");

            migrationBuilder.RenameColumn(
                name: "ValueTo",
                table: "GraphNodes",
                newName: "YearTo");

            migrationBuilder.RenameColumn(
                name: "ValueFrom",
                table: "GraphNodes",
                newName: "YearFrom");
        }
    }
}
