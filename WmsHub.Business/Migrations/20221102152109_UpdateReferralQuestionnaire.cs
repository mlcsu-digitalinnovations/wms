using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
  public partial class UpdateReferralQuestionnaire : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_QuestionnairesAudit_Questionnaires_Id",
          table: "QuestionnairesAudit");

      migrationBuilder.DropForeignKey(
          name: "FK_ReferralQuestionnairesAudit_ReferralQuestionnaires_Id",
          table: "ReferralQuestionnairesAudit");

      migrationBuilder.DropIndex(
          name: "IX_ReferralQuestionnairesAudit_Id",
          table: "ReferralQuestionnairesAudit");

      migrationBuilder.DropIndex(
          name: "IX_QuestionnairesAudit_Id",
          table: "QuestionnairesAudit");

      migrationBuilder.DropColumn(
          name: "Status",
          table: "ReferralQuestionnairesAudit");

      migrationBuilder.DropColumn(
          name: "Status",
          table: "ReferralQuestionnaires");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "TemporaryFailure",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "TechnicalFailure",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Started",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Sending",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "PermanentFailure",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Delivered",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Completed",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "TemporaryFailure",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "TechnicalFailure",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Started",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Sending",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "PermanentFailure",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Delivered",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Completed",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: true,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql("UPDATE ReferralQuestionnairesAudit SET TemporaryFailure='0001-01-01' WHERE TemporaryFailure IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnairesAudit SET TechnicalFailure='0001-01-01' WHERE TechnicalFailure IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnairesAudit SET Started='0001-01-01' WHERE Started IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnairesAudit SET Sending='0001-01-01' WHERE Sending IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnairesAudit SET PermanentFailure='0001-01-01' WHERE PermanentFailure IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnairesAudit SET Delivered='0001-01-01' WHERE Delivered IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnairesAudit SET Completed='0001-01-01' WHERE Completed IS NULL");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "TemporaryFailure",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "TechnicalFailure",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Started",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Sending",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "PermanentFailure",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Delivered",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Completed",
          table: "ReferralQuestionnairesAudit",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AddColumn<int>(
          name: "Status",
          table: "ReferralQuestionnairesAudit",
          type: "int",
          nullable: false,
          defaultValue: 0);

      migrationBuilder.Sql("UPDATE ReferralQuestionnaires SET TemporaryFailure='0001-01-01' WHERE TemporaryFailure IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnaires SET TechnicalFailure='0001-01-01' WHERE TechnicalFailure IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnaires SET Started='0001-01-01' WHERE Started IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnaires SET Sending='0001-01-01' WHERE Sending IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnaires SET PermanentFailure='0001-01-01' WHERE PermanentFailure IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnaires SET Delivered='0001-01-01' WHERE Delivered IS NULL");
      migrationBuilder.Sql("UPDATE ReferralQuestionnaires SET Completed='0001-01-01' WHERE Completed IS NULL");

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "TemporaryFailure",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "TechnicalFailure",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Started",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Sending",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "PermanentFailure",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Delivered",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "Completed",
          table: "ReferralQuestionnaires",
          type: "datetimeoffset",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset",
          oldNullable: true);

      migrationBuilder.AddColumn<int>(
          name: "Status",
          table: "ReferralQuestionnaires",
          type: "int",
          nullable: false,
          defaultValue: 0);

      migrationBuilder.CreateIndex(
          name: "IX_ReferralQuestionnairesAudit_Id",
          table: "ReferralQuestionnairesAudit",
          column: "Id");

      migrationBuilder.CreateIndex(
          name: "IX_QuestionnairesAudit_Id",
          table: "QuestionnairesAudit",
          column: "Id");

      migrationBuilder.AddForeignKey(
          name: "FK_QuestionnairesAudit_Questionnaires_Id",
          table: "QuestionnairesAudit",
          column: "Id",
          principalTable: "Questionnaires",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);

      migrationBuilder.AddForeignKey(
          name: "FK_ReferralQuestionnairesAudit_ReferralQuestionnaires_Id",
          table: "ReferralQuestionnairesAudit",
          column: "Id",
          principalTable: "ReferralQuestionnaires",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
    }
  }
}
