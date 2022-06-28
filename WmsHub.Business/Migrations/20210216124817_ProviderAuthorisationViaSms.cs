using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class ProviderAuthorisationViaSms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderAuth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SmsKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmsKeyExpiry = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    KeyViaSms = table.Column<bool>(type: "bit", nullable: false),
                    KeyViaEmail = table.Column<bool>(type: "bit", nullable: false),
#if DEBUG_NOAE
                  MobileNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmailContact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpWhitelist = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAuth", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderAuth_Providers_Id",
                        column: x => x.Id,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProviderAuthAudit",
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
                    SmsKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmsKeyExpiry = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    KeyViaSms = table.Column<bool>(type: "bit", nullable: false),
                    KeyViaEmail = table.Column<bool>(type: "bit", nullable: false),
#if DEBUG_NOAE
                  MobileNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmailContact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpWhitelist = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAuthAudit", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Revoked = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RevokedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokensAudit",
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
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Revoked = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RevokedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokensAudit", x => x.AuditId);
                });
#if !DEBUG_NOAE
        AlwaysEncrypted.AddColumnsForMigration(
              AlwaysEncryptedMigrations.ProviderAuthorisationViaSms, migrationBuilder);
#endif
      migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ProviderId",
                table: "RefreshTokens",
                column: "ProviderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderAuth");

            migrationBuilder.DropTable(
                name: "ProviderAuthAudit");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RefreshTokensAudit");
        }
    }
}
