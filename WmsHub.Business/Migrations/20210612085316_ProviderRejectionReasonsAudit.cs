using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class ProviderRejectionReasonsAudit : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "ProviderRejectionReasonsAudit",
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
            Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ProviderRejectionReasonsAudit", x => x.AuditId);
          });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "ProviderRejectionReasonsAudit");
    }
  }
}
