using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class PharmacyTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pharmacies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
#if DEBUG_NOAE
            Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
                  OdsCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TemplateVersion = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pharmacies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PharmaciesAudit",
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
#if DEBUG_NOAE
            Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
                  OdsCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateVersion = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmaciesAudit", x => x.AuditId);
                });

#if !DEBUG_NOAE
            AlwaysEncrypted.AddColumnsForMigration(
              AlwaysEncryptedMigrations.PharmacyTemplates, migrationBuilder);
#endif

      migrationBuilder.CreateIndex(
                name: "IX_Pharmacies_OdsCode",
                table: "Pharmacies",
                column: "OdsCode",
                unique: true,
                filter: "[OdsCode] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pharmacies");

            migrationBuilder.DropTable(
                name: "PharmaciesAudit");
        }
    }
}
