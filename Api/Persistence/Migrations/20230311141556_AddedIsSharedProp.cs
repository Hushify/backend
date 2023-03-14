using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hushify.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsSharedProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "Folders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "Files",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "Files");
        }
    }
}
