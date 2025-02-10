using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class Referral_DateOfProviderContactedServiceUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateOfProviderContactedServiceUser",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateOfProviderContactedServiceUser",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfProviderContactedServiceUser",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "DateOfProviderContactedServiceUser",
                table: "Referrals");
        }
    }
}
