using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ServiceIdCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DocumentVersion",
                table: "ReferralsAudit",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceId",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceSystem",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentVersion",
                table: "Referrals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceId",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceSystem",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentVersion",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "SourceSystem",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "DocumentVersion",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "SourceSystem",
                table: "Referrals");
        }
    }
}
