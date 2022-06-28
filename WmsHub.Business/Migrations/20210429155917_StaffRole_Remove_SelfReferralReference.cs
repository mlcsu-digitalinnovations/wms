using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class StaffRole_Remove_SelfReferralReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reference",
                table: "SelfReferrals");

            migrationBuilder.AddColumn<string>(
                name: "StaffRole",
                table: "ReferralsAudit",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffRole",
                table: "Referrals",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StaffRole",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "StaffRole",
                table: "Referrals");

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "SelfReferrals",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
