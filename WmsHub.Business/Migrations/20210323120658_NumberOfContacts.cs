using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class NumberOfContacts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MethodOfContact",
                table: "ReferralsAudit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfContacts",
                table: "ReferralsAudit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MethodOfContact",
                table: "Referrals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfContacts",
                table: "Referrals",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MethodOfContact",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "NumberOfContacts",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "MethodOfContact",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "NumberOfContacts",
                table: "Referrals");
        }
    }
}
