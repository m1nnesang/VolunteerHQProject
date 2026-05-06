using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerHQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PrivateMessages_SenderId",
                table: "PrivateMessages");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateMessages_SenderId_ReceiverId_SentAt",
                table: "PrivateMessages",
                columns: new[] { "SenderId", "ReceiverId", "SentAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PrivateMessages_SenderId_ReceiverId_SentAt",
                table: "PrivateMessages");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateMessages_SenderId",
                table: "PrivateMessages",
                column: "SenderId");
        }
    }
}
