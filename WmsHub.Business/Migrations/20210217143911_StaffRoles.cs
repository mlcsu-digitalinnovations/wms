using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class StaffRoles : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
      name: "StaffRoles",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false,
          defaultValueSql: "newsequentialid()"),
        IsActive = table.Column<bool>(type: "bit", nullable: false),
        ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset",
          nullable: false),
        ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier",
          nullable: false),
        DisplayName = table.Column<string>(type: "nvarchar(max)",
          nullable: true),
        DisplayOrder = table.Column<int>(type: "int", nullable: false,
          defaultValue: 0)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_StaffRole", x => x.Id);
      });

      migrationBuilder.CreateTable(
        name: "StaffRolesAudit",
        columns: table => new
        {
          AuditId = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          AuditAction = table.Column<string>(type: "nvarchar(max)",
            nullable: true),
          AuditDuration = table.Column<int>(type: "int", nullable: false),
          AuditErrorMessage = table.Column<string>(type: "nvarchar(max)",
            nullable: true),
          AuditResult = table.Column<int>(type: "int", nullable: false),
          AuditSuccess = table.Column<bool>(type: "bit", nullable: false),
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          IsActive = table.Column<bool>(type: "bit", nullable: false),
          ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset",
            nullable: false),
          ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier",
            nullable: false),
          DisplayName = table.Column<string>(type: "nvarchar(max)",
            nullable: true),
          DisplayOrder = table.Column<int>(type: "int", nullable: false,
            defaultValue: 0)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_StaffRolesAudit", x => x.AuditId);
        });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "StaffRoles");
      migrationBuilder.DropTable(name: "StaffRolesAudit");
    }
  }
}