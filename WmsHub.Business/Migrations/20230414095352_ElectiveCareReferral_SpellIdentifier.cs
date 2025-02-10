using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class ElectiveCareReferral_SpellIdentifier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpellIdentifier",
                table: "ReferralsAudit",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpellIdentifier",
                table: "Referrals",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpellIdentifier",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "SpellIdentifier",
                table: "Referrals");
        }
    }
}
