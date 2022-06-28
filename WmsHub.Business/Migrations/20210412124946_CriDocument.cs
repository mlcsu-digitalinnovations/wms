using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class CriDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CriId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReferralCri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
#if DEBUG_NOAE
                    ClinicalInfoPdfBase64 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
#endif
                    ClinicalInfoLastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralCri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReferralCriAudit",
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
                    ClinicalInfoPdfBase64 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
#endif
                    ClinicalInfoLastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralCriAudit", x => x.AuditId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_CriId",
                table: "Referrals",
                column: "CriId");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_ReferralCri_CriId",
                table: "Referrals",
                column: "CriId",
                principalTable: "ReferralCri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

#if !DEBUG_NOAE
            AlwaysEncrypted.AddColumnsForMigration(
              AlwaysEncryptedMigrations.CriDocument, migrationBuilder);
#endif
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_ReferralCri_CriId",
                table: "Referrals");

            migrationBuilder.DropTable(
                name: "ReferralCri");

            migrationBuilder.DropTable(
                name: "ReferralCriAudit");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_CriId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "CriId",
                table: "Referrals");
        }
    }
}
