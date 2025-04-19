using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Docvision.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Documents_DocumentId",
                table: "Images");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Documents_DocumentId",
                table: "Images",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Documents_DocumentId",
                table: "Images");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Documents_DocumentId",
                table: "Images",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
