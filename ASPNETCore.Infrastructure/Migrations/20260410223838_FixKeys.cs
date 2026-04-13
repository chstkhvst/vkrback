using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPNETCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_AspNetUsers_ModerId",
                table: "UserReports");

            migrationBuilder.RenameColumn(
                name: "ModerId",
                table: "UserReports",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserReports_ModerId",
                table: "UserReports",
                newName: "IX_UserReports_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "ModeratedByUserId",
                table: "UserReports",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ModeratedByUserId",
                table: "UserReports",
                column: "ModeratedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_AspNetUsers_ModeratedByUserId",
                table: "UserReports",
                column: "ModeratedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_AspNetUsers_UserId",
                table: "UserReports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerProfiles_VolunteerRanks_VolunteerRankId",
                table: "VolunteerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_VolunteerProfiles_VolunteerRankId",
                table: "VolunteerProfiles");

            migrationBuilder.DropColumn(
                name: "VolunteerRankId",
                table: "VolunteerProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_AspNetUsers_ModeratedByUserId",
                table: "UserReports");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_AspNetUsers_UserId",
                table: "UserReports");

            migrationBuilder.DropIndex(
                name: "IX_UserReports_ModeratedByUserId",
                table: "UserReports");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserReports",
                newName: "ModerId");

            migrationBuilder.RenameIndex(
                name: "IX_UserReports_UserId",
                table: "UserReports",
                newName: "IX_UserReports_ModerId");

            migrationBuilder.AlterColumn<string>(
                name: "ModeratedByUserId",
                table: "UserReports",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_AspNetUsers_ModerId",
                table: "UserReports",
                column: "ModerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddColumn<int>(
                name: "VolunteerRankId",
                table: "VolunteerProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerProfiles_VolunteerRankId",
                table: "VolunteerProfiles",
                column: "VolunteerRankId");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerProfiles_VolunteerRanks_VolunteerRankId",
                table: "VolunteerProfiles",
                column: "VolunteerRankId",
                principalTable: "VolunteerRanks",
                principalColumn: "Id");
        }
    }
}
