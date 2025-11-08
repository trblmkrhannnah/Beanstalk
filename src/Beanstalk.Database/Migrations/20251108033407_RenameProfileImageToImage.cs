using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beanstalk.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameProfileImageToImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_ProfileImages_SelectedImageId",
                table: "UserProfiles");

            migrationBuilder.RenameTable(
                name: "ProfileImages",
                newName: "Images");

            migrationBuilder.RenameIndex(
                name: "IX_ProfileImages_ProfileId",
                table: "Images",
                newName: "IX_Images_ProfileId");

            migrationBuilder.AddColumn<byte[]>(
                name: "ThumbnailData",
                table: "Images",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Images_SelectedImageId",
                table: "UserProfiles",
                column: "SelectedImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_UserProfiles_ProfileId",
                table: "Images",
                column: "ProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Images_SelectedImageId",
                table: "UserProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_UserProfiles_ProfileId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ThumbnailData",
                table: "Images");

            migrationBuilder.RenameTable(
                name: "Images",
                newName: "ProfileImages");

            migrationBuilder.RenameIndex(
                name: "IX_Images_ProfileId",
                table: "ProfileImages",
                newName: "IX_ProfileImages_ProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_ProfileImages_SelectedImageId",
                table: "UserProfiles",
                column: "SelectedImageId",
                principalTable: "ProfileImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileImages_UserProfiles_ProfileId",
                table: "ProfileImages",
                column: "ProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
