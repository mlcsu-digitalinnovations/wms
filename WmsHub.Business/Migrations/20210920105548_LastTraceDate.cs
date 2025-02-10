using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class LastTraceDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastTraceDate",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TraceCount",
                table: "ReferralsAudit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastTraceDate",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TraceCount",
                table: "Referrals",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastTraceDate",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "TraceCount",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "LastTraceDate",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "TraceCount",
                table: "Referrals");
        }
    }
}
