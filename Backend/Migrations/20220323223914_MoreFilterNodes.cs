using Microsoft.EntityFrameworkCore.Migrations;

namespace Backend.Migrations
{
    public partial class MoreFilterNodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mode",
                table: "GraphNodes");

            migrationBuilder.DropColumn(
                name: "TimeSignature",
                table: "GraphNodes");
        }
    }
}
