using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beanstalk.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteLinkCommentAndAutoDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoDeleteWhenExpired",
                table: "InviteLinks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "InviteLinks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoDeleteWhenExpired",
                table: "InviteLinks");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "InviteLinks");
        }
    }
}
