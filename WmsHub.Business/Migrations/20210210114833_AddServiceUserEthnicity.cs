using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class AddServiceUserEthnicity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceUserEthnicity",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceUserEthnicityGroup",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceUserEthnicity",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceUserEthnicityGroup",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceUserEthnicity",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "ServiceUserEthnicityGroup",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "ServiceUserEthnicity",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ServiceUserEthnicityGroup",
                table: "Referrals");
        }
    }
}
