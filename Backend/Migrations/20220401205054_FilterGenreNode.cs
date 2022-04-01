using Microsoft.EntityFrameworkCore.Migrations;

namespace Backend.Migrations
{
    public partial class FilterGenreNode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GenreId",
                table: "GraphNodes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GraphNodes_GenreId",
                table: "GraphNodes",
                column: "GenreId");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphNodes_Genres_GenreId",
                table: "GraphNodes",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GraphNodes_Genres_GenreId",
                table: "GraphNodes");

            migrationBuilder.DropIndex(
                name: "IX_GraphNodes_GenreId",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "GenreId",
                table: "GraphNodes");
        }
    }
}
