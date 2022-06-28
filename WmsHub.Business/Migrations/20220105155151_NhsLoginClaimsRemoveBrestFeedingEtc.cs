using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class NhsLoginClaimsRemoveBrestFeedingEtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCaesareanInPast3Months",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "HasGivenBirthInPast3Months",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "IsBrestFeeding",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "HasCaesareanInPast3Months",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "HasGivenBirthInPast3Months",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "IsBrestFeeding",
                table: "Referrals");

#if DEBUG_NOAE
            migrationBuilder.AddColumn<string>(
                name: "NhsLoginClaimEmail",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhsLoginClaimFamilyName",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhsLoginClaimGivenName",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhsLoginClaimMobile",
                table: "ReferralsAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhsLoginClaimEmail",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhsLoginClaimFamilyName",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhsLoginClaimGivenName",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhsLoginClaimMobile",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

#endif

#if !DEBUG_NOAE
      AlwaysEncrypted.AddColumnsForMigration(
        AlwaysEncryptedMigrations.NhsLoginClaims, migrationBuilder);
#endif
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NhsLoginClaimEmail",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "NhsLoginClaimFamilyName",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "NhsLoginClaimGivenName",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "NhsLoginClaimMobile",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "NhsLoginClaimEmail",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "NhsLoginClaimFamilyName",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "NhsLoginClaimGivenName",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "NhsLoginClaimMobile",
                table: "Referrals");

            migrationBuilder.AddColumn<bool>(
                name: "HasCaesareanInPast3Months",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasGivenBirthInPast3Months",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBrestFeeding",
                table: "ReferralsAudit",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCaesareanInPast3Months",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasGivenBirthInPast3Months",
                table: "Referrals",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBrestFeeding",
                table: "Referrals",
                type: "bit",
                nullable: true);
        }
    }
}
