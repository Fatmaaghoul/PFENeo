using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Docvision.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisMetadataFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnnotatedImageUrl",
                table: "Images",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "Images",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisModel",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DetectedObjectsCount",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProcessingTimeSec",
                table: "Documents",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnotatedImageUrl",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "AnalysisModel",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DetectedObjectsCount",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ProcessingTimeSec",
                table: "Documents");
        }
    }
}
