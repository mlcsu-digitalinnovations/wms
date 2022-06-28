using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ReferralDischarge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FirstRecordedWeight",
                table: "ReferralsAudit",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FirstRecordedWeightDate",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastRecordedWeight",
                table: "ReferralsAudit",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRecordedWeightDate",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FirstRecordedWeight",
                table: "Referrals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FirstRecordedWeightDate",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastRecordedWeight",
                table: "Referrals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRecordedWeightDate",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstRecordedWeight",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "FirstRecordedWeightDate",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "LastRecordedWeight",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "LastRecordedWeightDate",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "FirstRecordedWeight",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "FirstRecordedWeightDate",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "LastRecordedWeight",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "LastRecordedWeightDate",
                table: "Referrals");
        }
    }
}
