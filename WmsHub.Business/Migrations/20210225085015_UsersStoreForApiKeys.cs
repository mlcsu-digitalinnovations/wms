using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class UsersStoreForApiKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsersStore",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
#if DEBUG_NOAE                    
                    OwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
#endif
                  Domain = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                  Scope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ForceExpiry = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersStore", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersStoreAudit",
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
                    ApiKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
#if DEBUG_NOAE
                  OwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
#endif
                    Domain = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ForceExpiry = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersStoreAudit", x => x.AuditId);
                });

#if !DEBUG_NOAE
            AlwaysEncrypted.AddColumnsForMigration(
              AlwaysEncryptedMigrations.UsersStore, migrationBuilder);
#endif
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsersStore");

            migrationBuilder.DropTable(
                name: "UsersStoreAudit");
        }
    }
}
