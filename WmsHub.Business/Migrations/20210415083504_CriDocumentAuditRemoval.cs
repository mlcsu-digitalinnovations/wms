using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class CriDocumentAuditRemoval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
              "ALTER TABLE [ReferralCriAudit] " +
              "DROP COLUMN IF EXISTS [ClinicalInfoPdfBase64];");
            /******************************************************
             Due to the AlwaysEncrypted being changed from 
            AddEncryptedColumnWithAudit to AddEncryptedColumn means the 
            following code will always fail if running on new system.  
            Therefore the above script replaces as there is no 
            DropColumnIfExists*/
            /*migrationBuilder.DropColumn(
                name: "ClinicalInfoPdfBase64",
                table: "ReferralCriAudit");
            **************************************************************/

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateOfCriId",
                table: "ReferralCriAudit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateOfCriId",
                table: "ReferralCri",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateOfCriId",
                table: "ReferralCriAudit");

            migrationBuilder.DropColumn(
                name: "UpdateOfCriId",
                table: "ReferralCri");

            migrationBuilder.AddColumn<byte[]>(
                name: "ClinicalInfoPdfBase64",
                table: "ReferralCriAudit",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
