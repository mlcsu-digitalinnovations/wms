using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WmsHub.Business.Migrations
{
  public partial class NewStaffRoles_ExistingReferralSource : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql("DELETE FROM dbo.StaffRoles");

      migrationBuilder.InsertData(
        table: "StaffRoles",
        columns: new[] {  "IsActive", "ModifiedAt", "ModifiedByUserId",
              "DisplayName", "DisplayOrder", },
        values: new object[,]
        {
            { true,
              DateTimeOffset.Now,
              new Guid("00000000-0000-0000-0000-000000000000"),
              "Administrative and clerical", 100 },
            { true,
              DateTimeOffset.Now,
              new Guid("00000000-0000-0000-0000-000000000000"),
              "Allied Health Professional e.g. physiotherapist", 200 },
            { true,
              DateTimeOffset.Now,
              new Guid("00000000-0000-0000-0000-000000000000"),
              "Ambulance staff", 300 },
            { true,
              DateTimeOffset.Now,
              new Guid("00000000-0000-0000-0000-000000000000"),
              "Doctor", 400 },
            { true,
              DateTimeOffset.Now,
              new Guid("00000000-0000-0000-0000-000000000000"),
              "Estates and porters", 500 },
            { true,
              DateTimeOffset.Now,
              new Guid("00000000-0000-0000-0000-000000000000"),
              "Healthcare Assistant/Support worker",
              600 },
            { true,
              DateTimeOffset.Now,
              new Guid("00000000-0000-0000-0000-000000000000"),
              "Healthcare scientists", 700 },
            { true,
              DateTimeOffset.Now,
              new Guid("00000000-0000-0000-0000-000000000000"),
              "Managerial", 800 },
            { true,
              DateTimeOffset.Now,
              new Guid("00000000-0000-0000-0000-000000000000"),
              "Nursing and midwifery", 900 },
            { true,
              DateTimeOffset.Now,
              new Guid("00000000-0000-0000-0000-000000000000"),
              "Other", 1000 }
      });

      migrationBuilder.Sql(
        "UPDATE dbo.Referrals SET ReferralSource = 'GpReferral' " +
        "WHERE ReferralSource IS NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.InsertData(
        table: "StaffRoles",
        columns: new[] {  "IsActive", "ModifiedAt", "ModifiedByUserId",
              "DisplayName", "DisplayOrder", },
        values: new object[,]
        {
              { true,
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0,
                DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                new Guid("00000000-0000-0000-0000-000000000000"),
                "Doctor change request", 1 },
              { true,
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0,
                DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                new Guid("00000000-0000-0000-0000-000000000000"),
                "Nurse, health visitor or midwife", 2 },
              { true,
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0,
                DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                new Guid("00000000-0000-0000-0000-000000000000"),
                "Ambulance staff", 3 },
              { true,
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0,
                DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                new Guid("00000000-0000-0000-0000-000000000000"),
                "Scientific, therapeutic and technical staff", 4 },
              { true,
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0,
                DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                new Guid("00000000-0000-0000-0000-000000000000"),
                "Support to clinical staff", 5 },
              { true,
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0,
                DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                new Guid("00000000-0000-0000-0000-000000000000"),
                "NHS central functions, including NHS " +
                  "England/Improvement, CCG",
                6 },
              { true,
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0,
                DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                new Guid("00000000-0000-0000-0000-000000000000"),
                "NHS estates", 7 },
              { true,
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0,
                DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                new Guid("00000000-0000-0000-0000-000000000000"),
                "Manager", 8 },
              { true,
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0,
                DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                new Guid("00000000-0000-0000-0000-000000000000"),
                "Administrative staff", 9 },
              { true,
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0,
                DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                new Guid("00000000-0000-0000-0000-000000000000"),
                "Other", 10 }
      });
    }
  }
}
