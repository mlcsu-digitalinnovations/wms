using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ProviderSummaries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary2",
                table: "ProvidersAudit",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Summary3",
                table: "ProvidersAudit",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Summary2",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Summary3",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary2",
                table: "ProvidersAudit");

            migrationBuilder.DropColumn(
                name: "Summary3",
                table: "ProvidersAudit");

            migrationBuilder.DropColumn(
                name: "Summary2",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Summary3",
                table: "Providers");
        }
    }
}
