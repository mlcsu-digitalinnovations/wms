using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class EthnicityOverride : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "EthnicityOverrides",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
            IsActive = table.Column<bool>(type: "bit", nullable: false),
            ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            EthnicityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ReferralSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            GroupName = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_EthnicityOverrides", x => x.Id);
            table.ForeignKey(
                      name: "FK_EthnicityOverrides_Ethnicities_EthnicityId",
                      column: x => x.EthnicityId,
                      principalTable: "Ethnicities",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "EthnicityOverridesAudit",
          columns: table => new
          {
            AuditId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            AuditAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
            AuditDuration = table.Column<int>(type: "int", nullable: false),
            AuditErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
            AuditResult = table.Column<int>(type: "int", nullable: false),
            AuditSuccess = table.Column<bool>(type: "bit", nullable: false),
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            IsActive = table.Column<bool>(type: "bit", nullable: false),
            ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            EthnicityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ReferralSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            GroupName = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_EthnicityOverridesAudit", x => x.AuditId);
          });

      migrationBuilder.CreateIndex(
          name: "IX_EthnicityOverrides_EthnicityId",
          table: "EthnicityOverrides",
          column: "EthnicityId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "EthnicityOverrides");

      migrationBuilder.DropTable(
          name: "EthnicityOverridesAudit");
    }
  }
}
