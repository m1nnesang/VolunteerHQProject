using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerHQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_Organization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrganizationMemberships_UserId",
                table: "OrganizationMemberships");

            migrationBuilder.AddColumn<string>(
                name: "Experience",
                table: "JoinRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMemberships_UserId_OrganizationId",
                table: "OrganizationMemberships",
                columns: new[] { "UserId", "OrganizationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrganizationMemberships_UserId_OrganizationId",
                table: "OrganizationMemberships");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "JoinRequests");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMemberships_UserId",
                table: "OrganizationMemberships",
                column: "UserId");
        }
    }
}
