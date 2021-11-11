using Microsoft.EntityFrameworkCore.Migrations;

namespace Backend.Migrations
{
    public partial class TagGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TagGroupId",
                table: "Tags",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "TagGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagGroups", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TagGroups",
                columns: new[] { "Id", "Name", "Order" },
                values: new object[] { 1, "default", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagGroupId",
                table: "Tags",
                column: "TagGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_TagGroups_TagGroupId",
                table: "Tags",
                column: "TagGroupId",
                principalTable: "TagGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_TagGroups_TagGroupId",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "TagGroups");

            migrationBuilder.DropIndex(
                name: "IX_Tags_TagGroupId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagGroupId",
                table: "Tags");
        }
    }
}
