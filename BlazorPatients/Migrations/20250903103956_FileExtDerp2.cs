using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorPatients.Migrations
{
    /// <inheritdoc />
    public partial class FileExtDerp2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Images",
                newName: "FileExt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileExt",
                table: "Images",
                newName: "FileName");
        }
    }
}
