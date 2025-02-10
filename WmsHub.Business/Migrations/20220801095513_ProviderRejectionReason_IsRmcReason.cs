using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class ProviderRejectionReason_IsRmcReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRmcReason",
                table: "ProviderRejectionReasonsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRmcReason",
                table: "ProviderRejectionReasons",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRmcReason",
                table: "ProviderRejectionReasonsAudit");

            migrationBuilder.DropColumn(
                name: "IsRmcReason",
                table: "ProviderRejectionReasons");


        }
    }
}
