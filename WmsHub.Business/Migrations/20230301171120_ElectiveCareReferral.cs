using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class ElectiveCareReferral : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DatePlacedOnWaitingList",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpcsCodes",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceEthnicity",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SurgeryInLessThanEighteenWeeks",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeeksOnWaitingList",
                table: "ReferralsAudit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DatePlacedOnWaitingList",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpcsCodes",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceEthnicity",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SurgeryInLessThanEighteenWeeks",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeeksOnWaitingList",
                table: "Referrals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ElectiveCareReferrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectiveCareReferrals", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectiveCareReferrals");

            migrationBuilder.DropColumn(
                name: "DatePlacedOnWaitingList",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "OpcsCodes",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "SourceEthnicity",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "SurgeryInLessThanEighteenWeeks",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "WeeksOnWaitingList",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "DatePlacedOnWaitingList",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "OpcsCodes",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "SourceEthnicity",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "SurgeryInLessThanEighteenWeeks",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "WeeksOnWaitingList",
                table: "Referrals");
        }
    }
}
