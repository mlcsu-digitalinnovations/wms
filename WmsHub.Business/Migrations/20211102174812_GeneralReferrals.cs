using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class GeneralReferrals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasActiveEatingDisorder",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasArthritisOfHip",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasArthritisOfKnee",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCaesareanInPast3Months",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasGivenBirthInPast3Months",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasHadBariatricSurgery",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBrestFeeding",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPregnant",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasActiveEatingDisorder",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasArthritisOfHip",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasArthritisOfKnee",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCaesareanInPast3Months",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasGivenBirthInPast3Months",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasHadBariatricSurgery",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBrestFeeding",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPregnant",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GeneralReferrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralReferrals", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralReferrals");

            migrationBuilder.DropColumn(
                name: "HasActiveEatingDisorder",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "HasArthritisOfHip",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "HasArthritisOfKnee",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "HasCaesareanInPast3Months",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "HasGivenBirthInPast3Months",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "HasHadBariatricSurgery",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "IsBrestFeeding",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "IsPregnant",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "HasActiveEatingDisorder",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "HasArthritisOfHip",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "HasArthritisOfKnee",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "HasCaesareanInPast3Months",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "HasGivenBirthInPast3Months",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "HasHadBariatricSurgery",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "IsBrestFeeding",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "IsPregnant",
                table: "Referrals");
        }
    }
}
