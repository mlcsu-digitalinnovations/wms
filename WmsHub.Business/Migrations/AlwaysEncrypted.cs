using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WmsHub.Business.Migrations
{
  public enum AlwaysEncryptedMigrations
  {
    ReInitialise,
    ProviderAuthorisationViaSms,
    UsersStore,
    CriDocument,
    PracticeTemplates,
    DelayReason,
    PharmacyReferral,
    Pharmacists,
    PharmacyTemplates,
    NhsLoginClaims,
    UserActionLog,
    ConfigurationValues,
    MskReferral
  }

  public static class AlwaysEncrypted
  {
    public static void AddColumnsForMigration(
      AlwaysEncryptedMigrations migration,
      MigrationBuilder migrationBuilder)
    {
      switch (migration)
      {
        case AlwaysEncryptedMigrations.ReInitialise:
          ReInitialiseMigrationColumns(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.ProviderAuthorisationViaSms:
          ProviderAuthViaSmsMigrationColumns(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.UsersStore:
          UserStoreMigrationsColumns(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.CriDocument:
          CriDocumentMigrationsColumns(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.PracticeTemplates:
          PracticeTemplatesMigrationsColumns(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.DelayReason:
          DelayReasonMigrationColumn(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.PharmacyReferral:
          PharmacyReferralMigrationColumn(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.Pharmacists:
          PharmacistsMigrationColumn(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.PharmacyTemplates:
          PharmacyTemplatesMigrationColumns(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.NhsLoginClaims:
          NhsLoginClaimsMigrationsColumns(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.UserActionLog:
          UserActionLogMigrationsColumns(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.ConfigurationValues:
          ConfigurationValuesMigrationsColumns(migrationBuilder);
          break;
        case AlwaysEncryptedMigrations.MskReferral:
          MskReferralMigrationsColumns(migrationBuilder);
          break;
      }
    }

    private static void PharmacistsMigrationColumn(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Pharmacists", "ReferringPharmacyEmail",
        "[nvarchar](200)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);
    }

    private static void PharmacyReferralMigrationColumn(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "ReferringPharmacyEmail", 
        "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);
    }

    private static void DelayReasonMigrationColumn(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "DelayReason", "[nvarchar](2000)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);
    }

    private static void PracticeTemplatesMigrationsColumns(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Practices", "Email", "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Practices", "Name", "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);
    }

    private static void PharmacyTemplatesMigrationColumns(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Pharmacies", "Email", "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Pharmacies", "Name", "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);
    }

    private static void ReInitialiseMigrationColumns(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Calls", "Number", "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "NhsNumber", "[nvarchar](200)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "Ubrn", "[nvarchar](200)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "FamilyName", "[nvarchar](200)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "GivenName", "[nvarchar](200)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "Title", "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "Postcode", "[nvarchar](200)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "Address1", "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "Address2", "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "Town", "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "Telephone", "[nvarchar](200)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "Mobile", "[nvarchar](200)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "Email", "[nvarchar](200)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "DateOfBirth", "[datetimeoffset]",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "VulnerableDescription", 
        "[nvarchar](max)", EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);

      AlwaysEncryptedHelper.AddEncryptedColumn(
        migrationBuilder, "dbo", "RequestResponseLog", "Request",
        "[nvarchar](max)", EncryptionType.RANDOMIZED, DatabaseContext.CEK,
        true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "TextMessages", "Number", "[nvarchar](200)",
        EncryptionType.RANDOMIZED, DatabaseContext.CEK, true);
    }

    private static void ProviderAuthViaSmsMigrationColumns(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "ProviderAuth", "MobileNumber",
        "[nvarchar](200)", EncryptionType.RANDOMIZED, DatabaseContext.CEK,
        true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "ProviderAuth", "EmailContact",
        "[nvarchar](200)", EncryptionType.RANDOMIZED, DatabaseContext.CEK,
        true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "ProviderAuth", "IpWhitelist",
        "[nvarchar](200)", EncryptionType.RANDOMIZED, DatabaseContext.CEK,
        true);
    }

    private static void UserStoreMigrationsColumns(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "UsersStore", "OwnerName",
        "[nvarchar](200)", EncryptionType.RANDOMIZED, DatabaseContext.CEK,
        true);
    }

    private static void CriDocumentMigrationsColumns(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumn(
        migrationBuilder, "dbo", "ReferralCri", "ClinicalInfoPdfBase64",
        "varbinary(max)", EncryptionType.RANDOMIZED, DatabaseContext.CEK,
        true);
    }

    private static void NhsLoginClaimsMigrationsColumns(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "NhsLoginClaimEmail",
        "[nvarchar](200)", EncryptionType.DETERMINISTIC, DatabaseContext.CEK,
        true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "NhsLoginClaimFamilyName",
        "[nvarchar](200)", EncryptionType.DETERMINISTIC, DatabaseContext.CEK,
        true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "NhsLoginClaimGivenName",
        "[nvarchar](200)", EncryptionType.DETERMINISTIC, DatabaseContext.CEK,
        true);

      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "NhsLoginClaimMobile",
        "[nvarchar](200)", EncryptionType.DETERMINISTIC, DatabaseContext.CEK,
        true);
    }

    private static void UserActionLogMigrationsColumns(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumn(
        migrationBuilder, "dbo", "UserActionLogs", "IpAddress",
        "[nvarchar](200)", EncryptionType.DETERMINISTIC, DatabaseContext.CEK,
        true);
      AlwaysEncryptedHelper.AddEncryptedColumn(
        migrationBuilder, "dbo", "UserActionLogs", "Request",
        "[nvarchar](4000)", EncryptionType.RANDOMIZED, DatabaseContext.CEK,
        true);
    }
    private static void ConfigurationValuesMigrationsColumns(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumn(
        migrationBuilder, "dbo", "ConfigurationValues", "Value",
        "[nvarchar](4000)", EncryptionType.RANDOMIZED, DatabaseContext.CEK,
        true);
    }

    private static void MskReferralMigrationsColumns(
      MigrationBuilder migrationBuilder)
    {
      AlwaysEncryptedHelper.AddEncryptedColumnWithAudit(
        migrationBuilder, "dbo", "Referrals", "ReferringClinicianEmail",
        "[nvarchar](200)",
        EncryptionType.DETERMINISTIC, DatabaseContext.CEK, true);
    }
  }
}