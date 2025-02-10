using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    /// <inheritdoc />
    public partial class RefactorReferralAttachmentIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MostRecentAttachmentId",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "MostRecentAttachmentId",
                table: "Referrals");

            migrationBuilder.AlterColumn<string>(
                name: "ReferralAttachmentId",
                table: "ReferralsAudit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MostRecentAttachmentDate",
                table: "ReferralsAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReferralAttachmentId",
                table: "Referrals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MostRecentAttachmentDate",
                table: "Referrals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Creation",
                table: "ErsMockReferrals",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MostRecentAttachmentDate",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "MostRecentAttachmentDate",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "Creation",
                table: "ErsMockReferrals");

            migrationBuilder.AlterColumn<long>(
                name: "ReferralAttachmentId",
                table: "ReferralsAudit",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MostRecentAttachmentId",
                table: "ReferralsAudit",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ReferralAttachmentId",
                table: "Referrals",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MostRecentAttachmentId",
                table: "Referrals",
                type: "bigint",
                nullable: true);
        }
    }
}
