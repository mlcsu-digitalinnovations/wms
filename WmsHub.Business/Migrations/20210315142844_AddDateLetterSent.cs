using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class AddDateLetterSent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateLetterSent",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateLetterSent",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateLetterSent",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "DateLetterSent",
                table: "Referrals");
        }
    }
}
