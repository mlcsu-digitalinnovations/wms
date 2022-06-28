using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class Referral_ProviderSubmissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReferralId",
                table: "ProviderSubmissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSubmissions_ReferralId",
                table: "ProviderSubmissions",
                column: "ReferralId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderSubmissions_Referrals_ReferralId",
                table: "ProviderSubmissions",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderSubmissions_Referrals_ReferralId",
                table: "ProviderSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_ProviderSubmissions_ReferralId",
                table: "ProviderSubmissions");

            migrationBuilder.DropColumn(
                name: "ReferralId",
                table: "ProviderSubmissions");

        }
    }
}
