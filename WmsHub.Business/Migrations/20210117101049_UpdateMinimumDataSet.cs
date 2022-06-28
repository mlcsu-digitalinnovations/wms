using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class UpdateMinimumDataSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Referrals");

            migrationBuilder.RenameColumn(
                name: "Town",
                table: "ReferralsAudit",
                newName: "Address3");

            migrationBuilder.RenameColumn(
                name: "HasDisability",
                table: "ReferralsAudit",
                newName: "HasAPhysicalDisability");

            migrationBuilder.RenameColumn(
                name: "DisabilityDescription",
                table: "ReferralsAudit",
                newName: "ReferringGpPracticeName");

            migrationBuilder.RenameColumn(
                name: "Town",
                table: "Referrals",
                newName: "Address3");

            migrationBuilder.RenameColumn(
                name: "HasDisability",
                table: "Referrals",
                newName: "HasAPhysicalDisability");

            migrationBuilder.RenameColumn(
                name: "DisabilityDescription",
                table: "Referrals",
                newName: "ReferringGpPracticeName");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateCompletedProgramme",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateOfBmiAtRegistration",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateOfProviderSelection",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateStartedProgramme",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasALearningDisability",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgrammeOutcome",
                table: "ReferralsAudit",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateCompletedProgramme",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateOfBmiAtRegistration",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateOfProviderSelection",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateStartedProgramme",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasALearningDisability",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgrammeOutcome",
                table: "Referrals",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCompletedProgramme",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "DateOfBmiAtRegistration",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "DateOfProviderSelection",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "DateStartedProgramme",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "HasALearningDisability",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "ProgrammeOutcome",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "DateCompletedProgramme",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "DateOfBmiAtRegistration",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "DateOfProviderSelection",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "DateStartedProgramme",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "HasALearningDisability",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ProgrammeOutcome",
                table: "Referrals");

            migrationBuilder.RenameColumn(
                name: "ReferringGpPracticeName",
                table: "ReferralsAudit",
                newName: "DisabilityDescription");

            migrationBuilder.RenameColumn(
                name: "HasAPhysicalDisability",
                table: "ReferralsAudit",
                newName: "HasDisability");

            migrationBuilder.RenameColumn(
                name: "Address3",
                table: "ReferralsAudit",
                newName: "Town");

            migrationBuilder.RenameColumn(
                name: "ReferringGpPracticeName",
                table: "Referrals",
                newName: "DisabilityDescription");

            migrationBuilder.RenameColumn(
                name: "HasAPhysicalDisability",
                table: "Referrals",
                newName: "HasDisability");

            migrationBuilder.RenameColumn(
                name: "Address3",
                table: "Referrals",
                newName: "Town");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
