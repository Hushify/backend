using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hushify.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameTokenToTokenHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "RefreshToken",
                newName: "TokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "RefreshToken",
                newName: "Token");
        }
    }
}
