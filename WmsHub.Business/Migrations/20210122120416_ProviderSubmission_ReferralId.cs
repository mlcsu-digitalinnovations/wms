using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ProviderSubmission_ReferralId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderSubmissions_Referrals_ReferralId",
                table: "ProviderSubmissions");

            migrationBuilder.AddColumn<Guid>(
                name: "ReferralId",
                table: "ProviderSubmissionsAudit",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferralId",
                table: "ProviderSubmissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderSubmissions_Referrals_ReferralId",
                table: "ProviderSubmissions",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderSubmissions_Referrals_ReferralId",
                table: "ProviderSubmissions");

            migrationBuilder.DropColumn(
                name: "ReferralId",
                table: "ProviderSubmissionsAudit");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferralId",
                table: "ProviderSubmissions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderSubmissions_Referrals_ReferralId",
                table: "ProviderSubmissions",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
