using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations;

public partial class ReferralQuestionnaire_Table : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
          name: "ReferralQuestionnaires",
          columns: table => new
          {
              Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
              IsActive = table.Column<bool>(type: "bit", nullable: false),
              ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
              ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
              QuestionnaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
              NotificationKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
              Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              Sending = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              Delivered = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              TemporaryFailure = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              TechnicalFailure = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              PermanentFailure = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              Started = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              Completed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              FailureCount = table.Column<int>(type: "int", nullable: false),
              Status = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table =>
          {
              table.PrimaryKey("PK_ReferralQuestionnaires", x => x.Id);
              table.ForeignKey(
            name: "FK_ReferralQuestionnaires_Questionnaires_QuestionnaireId",
            column: x => x.QuestionnaireId,
            principalTable: "Questionnaires",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
              table.ForeignKey(
            name: "FK_ReferralQuestionnaires_Referrals_ReferralId",
            column: x => x.ReferralId,
            principalTable: "Referrals",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
          });

#if DEBUG_NOAE
        migrationBuilder.AddColumn<string>(
          name: "FamilyName",
          table: "ReferralQuestionnaires",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

        migrationBuilder.AddColumn<string>(
          name: "GivenName",
          table: "ReferralQuestionnaires",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

        migrationBuilder.AddColumn<string>(
          name: "Mobile",
          table: "ReferralQuestionnaires",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

        migrationBuilder.AddColumn<string>(
          name: "Email",
          table: "ReferralQuestionnaires",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

        migrationBuilder.AddColumn<string>(
          name: "Answers",
          table: "ReferralQuestionnaires",
          type: "nvarchar(max)",
          nullable: true);
#endif

    migrationBuilder.CreateTable(
          name: "ReferralQuestionnairesAudit",
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
              ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
              QuestionnaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
              NotificationKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
              Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              Sending = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              Delivered = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              TemporaryFailure = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              TechnicalFailure = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              PermanentFailure = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              Started = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              Completed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
              FailureCount = table.Column<int>(type: "int", nullable: false),
              Status = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table =>
          {
              table.PrimaryKey("PK_ReferralQuestionnairesAudit", x => x.AuditId);
              table.ForeignKey(
            name: "FK_ReferralQuestionnairesAudit_ReferralQuestionnaires_Id",
            column: x => x.Id,
            principalTable: "ReferralQuestionnaires",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
          });

#if DEBUG_NOAE
        migrationBuilder.AddColumn<string>(
          name: "FamilyName",
          table: "ReferralQuestionnairesAudit",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

        migrationBuilder.AddColumn<string>(
          name: "GivenName",
          table: "ReferralQuestionnairesAudit",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

        migrationBuilder.AddColumn<string>(
          name: "Mobile",
          table: "ReferralQuestionnairesAudit",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

        migrationBuilder.AddColumn<string>(
          name: "Email",
          table: "ReferralQuestionnairesAudit",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

        migrationBuilder.AddColumn<string>(
          name: "Answers",
          table: "ReferralQuestionnairesAudit",
          type: "nvarchar(max)",
          nullable: true);
#endif

#if !DEBUGNOAE
    AlwaysEncrypted.AddColumnsForMigration(
      AlwaysEncryptedMigrations.ReferralQuestionnaire, migrationBuilder);
#endif

        migrationBuilder.CreateIndex(
          name: "IX_ReferralQuestionnaires_NotificationKey",
          table: "ReferralQuestionnaires",
          column: "NotificationKey",
          unique: true,
          filter: "[NotificationKey] IS NOT NULL");

        migrationBuilder.CreateIndex(
          name: "IX_ReferralQuestionnaires_QuestionnaireId",
          table: "ReferralQuestionnaires",
          column: "QuestionnaireId");

        migrationBuilder.CreateIndex(
          name: "IX_ReferralQuestionnaires_ReferralId",
          table: "ReferralQuestionnaires",
          column: "ReferralId",
          unique: true);

        migrationBuilder.CreateIndex(
          name: "IX_ReferralQuestionnairesAudit_Id",
          table: "ReferralQuestionnairesAudit",
          column: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
          name: "ReferralQuestionnairesAudit");

        migrationBuilder.DropTable(
          name: "ReferralQuestionnaires");
    }
}
