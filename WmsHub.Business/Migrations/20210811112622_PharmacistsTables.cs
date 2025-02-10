using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class PharmacistsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pharmacists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                  ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
#if DEBUG_NOAE
                    ReferringPharmacyEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif              
                    KeyCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pharmacists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PharmacistsAudit",
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
                  ReferringPharmacyEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
                    KeyCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacistsAudit", x => x.AuditId);
                });

#if !DEBUG_NOAE
            AlwaysEncrypted.AddColumnsForMigration(
              AlwaysEncryptedMigrations.Pharmacists, migrationBuilder);
#endif
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pharmacists");

            migrationBuilder.DropTable(
                name: "PharmacistsAudit");
        }
    }
}
