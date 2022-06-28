using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class PharmacyReferrals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConsentForGpAndNhsNumberLookup",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferringPharmacyODSCode",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentForGpAndNhsNumberLookup",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferringPharmacyODSCode",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

#if DEBUG_NOAE
            migrationBuilder.AddColumn<string>(
              name: "ReferringPharmacyEmail",
              table: "ReferralsAudit",
              type: "nvarchar(200)",
              maxLength: 200,
              nullable: true);

           migrationBuilder.AddColumn<string>(
                name: "ReferringPharmacyEmail",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
#endif

      migrationBuilder.CreateTable(
                name: "PharmacyReferrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyReferrals", x => x.Id);
                });

#if !DEBUG_NOAE
            AlwaysEncrypted.AddColumnsForMigration(
              AlwaysEncryptedMigrations.PharmacyReferral, migrationBuilder);
#endif
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PharmacyReferrals");

            migrationBuilder.DropColumn(
                name: "ConsentForGpAndNhsNumberLookup",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "ReferringPharmacyEmail",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "ReferringPharmacyODSCode",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "ConsentForGpAndNhsNumberLookup",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ReferringPharmacyEmail",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ReferringPharmacyODSCode",
                table: "Referrals");
        }
    }
}
