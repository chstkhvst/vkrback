using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPNETCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BanReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_AspNetUsers_UserId",
                table: "UserReports");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserReports",
                newName: "ModerId");

            migrationBuilder.RenameIndex(
                name: "IX_UserReports_UserId",
                table: "UserReports",
                newName: "IX_UserReports_ModerId");

            migrationBuilder.AddColumn<string>(
                name: "BanReason",
                table: "Bans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_AspNetUsers_ModerId",
                table: "UserReports",
                column: "ModerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_AspNetUsers_ModerId",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "BanReason",
                table: "Bans");

            migrationBuilder.RenameColumn(
                name: "ModerId",
                table: "UserReports",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserReports_ModerId",
                table: "UserReports",
                newName: "IX_UserReports_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_AspNetUsers_UserId",
                table: "UserReports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
