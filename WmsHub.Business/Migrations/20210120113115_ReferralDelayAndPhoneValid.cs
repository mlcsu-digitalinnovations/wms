using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ReferralDelayAndPhoneValid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateToDelayUntil",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMobileValid",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTelephoneValid",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateToDelayUntil",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMobileValid",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTelephoneValid",
                table: "Referrals",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateToDelayUntil",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "IsMobileValid",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "IsTelephoneValid",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "DateToDelayUntil",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "IsMobileValid",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "IsTelephoneValid",
                table: "Referrals");
        }
    }
}
