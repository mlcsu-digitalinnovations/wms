using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class MskReferral : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
          name: "ReferringPharmacyODSCode",
          table: "ReferralsAudit",
          newName: "ReferringOrganisationOdsCode");

      migrationBuilder.RenameColumn(
          name: "ReferringPharmacyEmail",
          table: "ReferralsAudit",
          newName: "ReferringOrganisationEmail");

      migrationBuilder.RenameColumn(
          name: "ReferringPharmacyODSCode",
          table: "Referrals",
          newName: "ReferringOrganisationOdsCode");

      migrationBuilder.RenameColumn(
          name: "ReferringPharmacyEmail",
          table: "Referrals",
          newName: "ReferringOrganisationEmail");

      migrationBuilder.AddColumn<string>(
          name: "CreatedByUserId",
          table: "ReferralsAudit",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

#if DEBUG_NOAE
      migrationBuilder.AddColumn<string>(
          name: "ReferringClinicianEmail",
          table: "ReferralsAudit",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);
#endif

      migrationBuilder.AddColumn<string>(
          name: "CreatedByUserId",
          table: "Referrals",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

#if DEBUG_NOAE
      migrationBuilder.AddColumn<string>(
          name: "ReferringClinicianEmail",
          table: "Referrals",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);
#endif

#if !DEBUG_NOAE
      AlwaysEncrypted.AddColumnsForMigration(
        AlwaysEncryptedMigrations.MskReferral, migrationBuilder);
#endif
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "CreatedByUserId",
          table: "ReferralsAudit");

      migrationBuilder.DropColumn(
          name: "ReferringClinicianEmail",
          table: "ReferralsAudit");

      migrationBuilder.DropColumn(
          name: "CreatedByUserId",
          table: "Referrals");

      migrationBuilder.DropColumn(
          name: "ReferringClinicianEmail",
          table: "Referrals");

      migrationBuilder.RenameColumn(
          name: "ReferringOrganisationOdsCode",
          table: "ReferralsAudit",
          newName: "ReferringPharmacyODSCode");

      migrationBuilder.RenameColumn(
          name: "ReferringOrganisationEmail",
          table: "ReferralsAudit",
          newName: "ReferringPharmacyEmail");

      migrationBuilder.RenameColumn(
          name: "ReferringOrganisationOdsCode",
          table: "Referrals",
          newName: "ReferringPharmacyODSCode");

      migrationBuilder.RenameColumn(
          name: "ReferringOrganisationEmail",
          table: "Referrals",
          newName: "ReferringPharmacyEmail");
    }
  }
}
