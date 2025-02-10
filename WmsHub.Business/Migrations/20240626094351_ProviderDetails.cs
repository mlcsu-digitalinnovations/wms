using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
  /// <inheritdoc />
  public partial class ProviderDetails : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "ProviderDetails",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
            IsActive = table.Column<bool>(type: "bit", nullable: false),
            ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Section = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            TriageLevel = table.Column<int>(type: "int", nullable: false),
            Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ProviderDetails", x => x.Id);
            table.ForeignKey(
                      name: "FK_ProviderDetails_Providers_ProviderId",
                      column: x => x.ProviderId,
                      principalTable: "Providers",
                      principalColumn: "Id");
          });

      migrationBuilder.CreateTable(
          name: "ProviderDetailsAudit",
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
            ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Section = table.Column<string>(type: "nvarchar(max)", nullable: false),
            TriageLevel = table.Column<int>(type: "int", nullable: false),
            Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ProviderDetailsAudit", x => x.AuditId);
          });

      migrationBuilder.CreateIndex(
          name: "IX_ProviderDetails_ProviderId",
          table: "ProviderDetails",
          column: "ProviderId");

      SeedData.ProviderDetailsUp(migrationBuilder);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "ProviderDetails");

      migrationBuilder.DropTable(
          name: "ProviderDetailsAudit");
    }
  }
}
