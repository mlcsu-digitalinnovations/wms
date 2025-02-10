using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
  public partial class GpReferrals : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "GpReferrals",
        columns: table => new
        {
          Id = table.Column<int>(type: "int", nullable: false)
              .Annotation("SqlServer:Identity", "1, 1"),
          ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_GpReferrals", x => x.Id);
        });

#if !DEBUG_NOAE
      AlwaysEncrypted.AddColumnsForMigration(
        AlwaysEncryptedMigrations.GpReferral, migrationBuilder);
#endif
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
        name: "GpReferrals");

      migrationBuilder.DropColumn(
        name: "ErsUbrn",
        table: "ReferralsAudit");

      migrationBuilder.DropColumn(
        name: "ErsUbrn",
        table: "Referrals");
    }
  }
}
