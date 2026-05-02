using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerHQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDenormalizedFinancialFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentProgress",
                table: "Fundraisers");

            migrationBuilder.DropColumn(
                name: "AmountRaised",
                table: "FundraiserAssignments");

            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                table: "PrivateMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEdited",
                table: "PrivateMessages");

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentProgress",
                table: "Fundraisers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountRaised",
                table: "FundraiserAssignments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
