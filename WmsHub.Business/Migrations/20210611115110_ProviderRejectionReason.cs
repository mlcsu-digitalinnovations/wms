using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ProviderRejectionReason : Migration
    {
      protected override void Up(MigrationBuilder migrationBuilder)
      {
        migrationBuilder.CreateTable(
          name: "ProviderRejectionReasons",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false,
              defaultValueSql: "newsequentialid()"),
            Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100,
              nullable: true),
            Description = table.Column<string>(type: "nvarchar(500)",
              maxLength: 500, nullable: true),
            IsActive = table.Column<bool>(type: "bit", nullable: false),
            ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset",
              nullable: false),
            ModifiedByUserId =
              table.Column<Guid>(type: "uniqueidentifier", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ProviderRejectionReasons", x => x.Id);
          });

        migrationBuilder.InsertData(
          table: "ProviderRejectionReasons",
          columns: new[]
          {
            "Id", "Title", "Description", "IsActive", "ModifiedAt",
            "ModifiedByUserId"
          },
          values: new object[,]
          {
            {
              new Guid("7A400DC4-6DD1-4EF5-B839-CCBA2C5B7746"),
              "WrongProviderSelected",
              "Service User has inadvertently selected the wrong Provider.",
              true,
              new DateTimeOffset(
                new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                new TimeSpan(0, 0, 0, 0, 0)),
              new Guid("00000000-0000-0000-0000-000000000000")
            },
            {
              new Guid("8E801FA9-EE15-4637-AD3E-F5881A2CE373"),
              "InvalidBMI",
              "Service Users BMI is below BMI ≥ 30kg/m² (adjusted to " +
              "27.5kg/m² in people black Asian and minority ethnic groups).",
              true,
              new DateTimeOffset(
                new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                new TimeSpan(0, 0, 0, 0, 0)),
              new Guid("00000000-0000-0000-0000-000000000000")
            },
            {
              new Guid("C18C7833-8985-4114-B5E5-21B637E5A933"),
              "ExclusionCriteria",
              "Service User registration information indicated they meet one " +
              "of the exclusion criteria as listed in the Service " +
              "Specification.",
              true,
              new DateTimeOffset(
                new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                new TimeSpan(0, 0, 0, 0, 0)),
              new Guid("00000000-0000-0000-0000-000000000000")
            },
            {
              new Guid("3837323D-0E7D-4BD7-9297-3300CD124CD8"),
              "ServiceUserNotReady",
              "Service User has stated they are not ready to take up the " +
              "service.",
              true,
              new DateTimeOffset(
                new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                new TimeSpan(0, 0, 0, 0, 0)),
              new Guid("00000000-0000-0000-0000-000000000000")
            },
          }
        );
      }

      protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderRejectionReasons");
        }
    }
}
