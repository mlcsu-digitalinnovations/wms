using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class AddProviders : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<Guid>(
          name: "ProviderId",
          table: "ReferralsAudit",
          type: "uniqueidentifier",
          nullable: true);

      migrationBuilder.AddColumn<Guid>(
          name: "ProviderId",
          table: "Referrals",
          type: "uniqueidentifier",
          nullable: true);

      migrationBuilder.CreateTable(
          name: "Providers",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
            IsActive = table.Column<bool>(type: "bit", nullable: false),
            ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            Summary = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            Website = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Level1 = table.Column<bool>(type: "bit", nullable: false),
            Level2 = table.Column<bool>(type: "bit", nullable: false),
            Level3 = table.Column<bool>(type: "bit", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Providers", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "ProvidersAudit",
          columns: table => new
          {
            AuditId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            AuditAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
            AuditDuration = table.Column<int>(type: "int", nullable: false),
            AuditErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
            AuditResult = table.Column<int>(type: "int", nullable: false),
            AuditSuccess = table.Column<bool>(type: "bit", nullable: false),
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            IsActive = table.Column<bool>(type: "bit", nullable: false),
            ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Website = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Level1 = table.Column<bool>(type: "bit", nullable: false),
            Level2 = table.Column<bool>(type: "bit", nullable: false),
            Level3 = table.Column<bool>(type: "bit", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ProvidersAudit", x => x.AuditId);
          });

      migrationBuilder.CreateIndex(
          name: "IX_Referrals_ProviderId",
          table: "Referrals",
          column: "ProviderId");

      migrationBuilder.AddForeignKey(
          name: "FK_Referrals_Providers_ProviderId",
          table: "Referrals",
          column: "ProviderId",
          principalTable: "Providers",
          principalColumn: "Id",
          onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_Referrals_Providers_ProviderId",
          table: "Referrals");

      migrationBuilder.DropTable(
          name: "Providers");

      migrationBuilder.DropTable(
          name: "ProvidersAudit");

      migrationBuilder.DropIndex(
          name: "IX_Referrals_ProviderId",
          table: "Referrals");

      migrationBuilder.DropColumn(
          name: "ProviderId",
          table: "ReferralsAudit");

      migrationBuilder.DropColumn(
          name: "ProviderId",
          table: "Referrals");
    }
  }
}
