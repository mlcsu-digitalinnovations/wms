using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ReferralAttachmentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ReferralAttachmentId",
                table: "ReferralsAudit",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReferralAttachmentId",
                table: "Referrals",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferralAttachmentId",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "ReferralAttachmentId",
                table: "Referrals");
        }
    }
}
