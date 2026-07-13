using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectronicLibrary.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetThumbnail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailBlobName",
                table: "Assets",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailBlobName",
                table: "Assets");
        }
    }
}
