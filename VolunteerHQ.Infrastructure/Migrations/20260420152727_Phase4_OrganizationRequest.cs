using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerHQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_OrganizationRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "OrganizationRequests");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "OrganizationRequests");

            migrationBuilder.DropColumn(
                name: "SecondName",
                table: "OrganizationRequests");

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserId",
                table: "OrganizationRequests",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "OrganizationRequests");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "OrganizationRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "OrganizationRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondName",
                table: "OrganizationRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
