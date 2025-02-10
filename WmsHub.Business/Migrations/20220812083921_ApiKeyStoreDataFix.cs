using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class ApiKeyStoreDataFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.Sql(@"UPDATE ApiKeyStore Set Expires = NULL Where keyUser = 'Provider_admin'");

          migrationBuilder.InsertData(
            table: "ApiKeyStore",
            columns: new[]
            {
              "Id",
              "IsActive",
              "ModifiedAt",
              "ModifiedByUserId",
              "Key",
              "KeyUser",
              "Domain",
              "Domains",
              "Sid",
              "Expires"
            },
            values: new object[,]
            {
              {
                Guid.Parse("B766CD55-8631-4791-A185-132302478F39"),
                true,
                DateTimeOffset.Now,
                Guid.Empty,
                "5C46B8A6-EA76-4141-A2F2-ABC",
                "Provider_admin",
                130,
                "**Test-only**||Expiry is Null:True",
                "B766CD55-8631-4791-A185-132302478F39",
                null
              },
              {
                Guid.Parse("FC203440-D9F8-4CAA-939B-AD1E38FF2759"),
                true,
                DateTimeOffset.Now,
                Guid.Empty,
                "F2B0337B-ED6A-4768-900C-9",
                "Provider_admin",
                130,
                "**Test-only**||Expiry is Future:True",
                "FC203440-D9F8-4CAA-939B-AD1E38FF2759",
                DateTimeOffset.Now.AddYears(1)
              },
              {
                Guid.Parse("256A4132-9555-46CF-8661-9C9D97DEFE3F"),
                true,
                DateTimeOffset.Now,
                Guid.Empty,
                "D597A4ED-16E3-4B0B-83B3-3",
                "Provider_admin",
                130,
                "**Test-only**||Expiry is Past:False",
                "256A4132-9555-46CF-8661-9C9D97DEFE3F",
                DateTimeOffset.Now.AddDays(-30)
              }
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.Sql(@"DELETE ApiKeyStore WHERE Id IN 
        ('B766CD55-8631-4791-A185-132302478F39',
        'FC203440-D9F8-4CAA-939B-AD1E38FF2759',
        '256A4132-9555-46CF-8661-9C9D97DEFE3F')");
        }
    }
}
