using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hushify.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangedUploadStatusToFileStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadStatus",
                table: "Files",
                newName: "FileStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileStatus",
                table: "Files",
                newName: "UploadStatus");
        }
    }
}
