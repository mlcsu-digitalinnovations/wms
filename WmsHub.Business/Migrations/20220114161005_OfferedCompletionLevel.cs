using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class OfferedCompletionLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OfferedCompletionLevel",
                table: "ReferralsAudit",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferedCompletionLevel",
                table: "Referrals",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfferedCompletionLevel",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "OfferedCompletionLevel",
                table: "Referrals");
        }
    }
}
