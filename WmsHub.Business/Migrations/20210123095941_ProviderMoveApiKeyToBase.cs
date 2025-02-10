using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ProviderMoveApiKeyToBase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "ProvidersAudit",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApiKeyExpires",
                table: "ProvidersAudit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ApiKeyExpires",
                table: "Providers",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "ProvidersAudit");

            migrationBuilder.DropColumn(
                name: "ApiKeyExpires",
                table: "ProvidersAudit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApiKeyExpires",
                table: "Providers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);
        }
    }
}
