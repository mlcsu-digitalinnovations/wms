using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class AddBase36DateSent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Base36DateSent",
                table: "TextMessagesAudit",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Base36DateSent",
                table: "TextMessages",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Base36DateSent",
                table: "TextMessagesAudit");

            migrationBuilder.DropColumn(
                name: "Base36DateSent",
                table: "TextMessages");
        }
    }
}
