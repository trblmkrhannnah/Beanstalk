using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beanstalk.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddThemePalettes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ThemePaletteId",
                table: "UserProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ThemePalettes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BackgroundGradient1 = table.Column<string>(type: "text", nullable: false),
                    BackgroundGradient2 = table.Column<string>(type: "text", nullable: false),
                    TitleColor = table.Column<string>(type: "text", nullable: false),
                    ContentColor = table.Column<string>(type: "text", nullable: false),
                    ContainerBackground = table.Column<string>(type: "text", nullable: false),
                    ContainerForeground = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemePalettes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_ThemePaletteId",
                table: "UserProfiles",
                column: "ThemePaletteId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_ThemePalettes_ThemePaletteId",
                table: "UserProfiles",
                column: "ThemePaletteId",
                principalTable: "ThemePalettes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_ThemePalettes_ThemePaletteId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "ThemePalettes");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_ThemePaletteId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ThemePaletteId",
                table: "UserProfiles");
        }
    }
}
