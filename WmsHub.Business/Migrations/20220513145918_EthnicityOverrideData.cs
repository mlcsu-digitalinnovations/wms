using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WmsHub.Business.Migrations
{
  public partial class EthnicityOverrideData : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.InsertData(
          table: "EthnicityOverrides",
          columns: new[]
          {
            "Id", 
            "IsActive", 
            "ModifiedAt", 
            "ModifiedByUserId", 
            "EthnicityId",
            "ReferralSource", 
            "DisplayName", 
            "GroupName"
          },
          values: new object[,]
          {
            {
              Guid.NewGuid(), 
              true,
              DateTimeOffset.Now,
              Guid.Empty,
              Guid.Parse("95b0feb5-5ece-98ed-1269-c71e327e98c5"),
              Enums.ReferralSource.Msk.ToString(),
              "The patient does not want to disclose their ethnicity",
              "The patient does not want to disclose their ethnicity"
            },
            {
              Guid.NewGuid(),
              true,
              DateTimeOffset.Now,
              Guid.Empty,
              Guid.Parse("95b0feb5-5ece-98ed-1269-c71e327e98c5"),
              Enums.ReferralSource.Pharmacy.ToString(),
              "The patient does not want to disclose their ethnicity",
              "The patient does not want to disclose their ethnicity"
            }
          }
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
  }
}
