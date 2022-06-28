using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ApiKey_Database_Store : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeyStore",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KeyUser = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Domain = table.Column<int>(type: "int", nullable: false),
                    Domains = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sid = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeyStore", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeyStoreAudit",
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
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KeyUser = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Domain = table.Column<int>(type: "int", nullable: false),
                    Domains = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sid = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeyStoreAudit", x => x.AuditId);
                });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeyStore");

            migrationBuilder.DropTable(
                name: "ApiKeyStoreAudit");
        }
    }
}
