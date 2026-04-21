using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPNETCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReportsWithMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_AspNetUsers_UserId",
                table: "UserReports");

            migrationBuilder.DropIndex(
                name: "IX_UserReports_UserId",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserReports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserReports",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_UserId",
                table: "UserReports",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_AspNetUsers_UserId",
                table: "UserReports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
