using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerHQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDonationNullableAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_FundraiserAssignments_FundraiserAssignmentId",
                table: "Donations");

            migrationBuilder.AlterColumn<int>(
                name: "FundraiserAssignmentId",
                table: "Donations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_FundraiserAssignments_FundraiserAssignmentId",
                table: "Donations",
                column: "FundraiserAssignmentId",
                principalTable: "FundraiserAssignments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_FundraiserAssignments_FundraiserAssignmentId",
                table: "Donations");

            migrationBuilder.AlterColumn<int>(
                name: "FundraiserAssignmentId",
                table: "Donations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_FundraiserAssignments_FundraiserAssignmentId",
                table: "Donations",
                column: "FundraiserAssignmentId",
                principalTable: "FundraiserAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
