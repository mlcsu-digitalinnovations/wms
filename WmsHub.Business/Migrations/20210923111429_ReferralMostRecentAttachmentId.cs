using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ReferralMostRecentAttachmentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MostRecentAttachmentId",
                table: "ReferralsAudit",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MostRecentAttachmentId",
                table: "Referrals",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MostRecentAttachmentId",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "MostRecentAttachmentId",
                table: "Referrals");
        }
    }
}
