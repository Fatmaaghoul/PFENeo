using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Docvision.Migrations
{
    /// <inheritdoc />
    public partial class extraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "triter",
                table: "Documents",
                newName: "istraiter");

            migrationBuilder.AddColumn<bool>(
                name: "isExtracted",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isExtracted",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "istraiter",
                table: "Documents",
                newName: "triter");
        }
    }
}
