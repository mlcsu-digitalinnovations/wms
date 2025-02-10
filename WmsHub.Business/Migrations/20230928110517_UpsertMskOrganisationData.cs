using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WmsHub.Business.Migrations;

/// <inheritdoc />
public partial class UpsertMskOrganisationData : Migration
{
  private readonly Dictionary<string, string> _mskOrgs = new()
      {
        { "RY448", "Hertfordshire Community Hospital Services" },
        { "R0A07", "Wythenshawe Hospital"},
        { "R1CD4", "St. Mary''s Hospital" },
        { "RRE58", "Sir Robert Peel Community Hospital" },
        { "NLX01", "Sirona Care & Health" },
        { "RVY38", "Ormskirk & District General Hospital" },
        { "NR315", "Nottingham Citycare Partnership" },
        { "RWK88", "The Romford Road Centre" },
        { "NMG77", "North Kirklees MSK Service"},
        { "RVWAE", "North Tees NHS Foundation Trust"},
        { "RWX58", "Church Hill House Hospital" },
        { "RBN52", "Mersey and West Lancashire Teaching Hospitals NHS Trust"}
      };

  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  { 

    foreach (KeyValuePair<string, string> org in _mskOrgs)
    {
      migrationBuilder.Sql(
        $"DECLARE @OdsCode AS NVARCHAR (200) = '{org.Key}' " +
        $"DECLARE @SiteName AS NVARCHAR (200) = '{org.Value}' " +
        "SET NOCOUNT ON; " +
        "IF EXISTS " +
        "(SELECT 1 FROM dbo.MskOrganisations WHERE OdsCode = @OdsCode) " +
        "BEGIN " +
        "UPDATE dbo.MskOrganisations " +
        "SET IsActive = 1, " +
        "ModifiedAt = GETDATE(), " +
        $"ModifiedByUserId = CAST('{Guid.Empty}' AS UNIQUEIDENTIFIER), " +
        "SiteName = @SiteName, " +
        "SendDischargeLetters = 0 " +
        "WHERE OdsCode = @OdsCode; " +
        "END " +
        "ELSE " +
        "BEGIN " +
        "INSERT dbo.MskOrganisations " +
        "VALUES (" +
        "NEWID(), " +
        "1, " +
        "GETDATE(), " +
        $"CAST('{Guid.Empty}' AS UNIQUEIDENTIFIER), " +
        "@OdsCode, " +
        "0, " +
        "@SiteName); " +
        "END"
        );
    }

    migrationBuilder.Sql("DELETE FROM dbo.ConfigurationValues " +
      "WHERE Id LIKE 'WmsHub_Referral_Api_MskReferralOptions:MskHubs:%'");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    foreach (KeyValuePair<string, string> org in _mskOrgs)
    {
      migrationBuilder.Sql(
        $"DELETE FROM dbo.MskOrganisations WHERE OdsCode = '{org.Key}'");
    }
  }
}
