using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTextMessageAddDobAttemptsColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DobAttempts",
                table: "TextMessagesAudit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DobAttempts",
                table: "TextMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DobAttempts",
                table: "TextMessagesAudit");

            migrationBuilder.DropColumn(
                name: "DobAttempts",
                table: "TextMessages");
        }
    }
}
