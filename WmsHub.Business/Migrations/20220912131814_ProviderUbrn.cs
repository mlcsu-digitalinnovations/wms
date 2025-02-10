using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class ProviderUbrn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ErsUbrn",
                table: "ReferralsAudit",
                newName: "ProviderUbrn");

            migrationBuilder.RenameColumn(
                name: "ErsUbrn",
                table: "Referrals",
                newName: "ProviderUbrn");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProviderUbrn",
                table: "ReferralsAudit",
                newName: "ErsUbrn");

            migrationBuilder.RenameColumn(
                name: "ProviderUbrn",
                table: "Referrals",
                newName: "ErsUbrn");
        }
    }
}
