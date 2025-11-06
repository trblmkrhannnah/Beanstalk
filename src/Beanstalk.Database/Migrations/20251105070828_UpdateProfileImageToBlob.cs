using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beanstalk.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProfileImageToBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "ProfileImages");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "ProfileImages");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "ProfileImages");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "ProfileImages",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "ProfileImages");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "ProfileImages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "ProfileImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "ProfileImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
