using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Docvision.Migrations
{
    /// <inheritdoc />
    public partial class analyzeee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "istraiter",
                table: "Documents",
                newName: "isAnalysed");

            migrationBuilder.AddColumn<string>(
                name: "Objects",
                table: "Images",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Objects",
                table: "Images");

            migrationBuilder.RenameColumn(
                name: "isAnalysed",
                table: "Documents",
                newName: "istraiter");
        }
    }
}
