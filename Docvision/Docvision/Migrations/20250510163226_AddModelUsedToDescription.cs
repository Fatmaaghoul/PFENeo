using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Docvision.Migrations
{
    /// <inheritdoc />
    public partial class AddModelUsedToDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelUsed",
                table: "Descriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModelConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelectedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelConfigurations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelConfigurations");

            migrationBuilder.DropColumn(
                name: "ModelUsed",
                table: "Descriptions");
        }
    }
}
