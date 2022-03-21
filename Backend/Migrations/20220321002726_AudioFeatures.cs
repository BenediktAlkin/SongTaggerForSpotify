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

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AudioFeaturesId",
                table: "Tracks",
                column: "AudioFeaturesId",
                unique: true);

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
                name: "FK_Tracks_AudioFeatures_AudioFeaturesId",
                table: "Tracks");

            migrationBuilder.DropTable(
                name: "AudioFeatures");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_AudioFeaturesId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "AudioFeaturesId",
                table: "Tracks");

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
