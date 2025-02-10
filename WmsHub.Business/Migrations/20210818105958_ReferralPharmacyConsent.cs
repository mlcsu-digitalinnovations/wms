using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ReferralPharmacyConsent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConsentForReferrerUpdatedWithOutcome",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentForReferrerUpdatedWithOutcome",
                table: "Referrals",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentForReferrerUpdatedWithOutcome",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "ConsentForReferrerUpdatedWithOutcome",
                table: "Referrals");
        }
    }
}
