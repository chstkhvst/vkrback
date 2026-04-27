using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPNETCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrgProfileModeratedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModeratedByUserId",
                table: "OrganizerProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModeratedByUserId",
                table: "OrganizerProfiles");
        }
    }
}
