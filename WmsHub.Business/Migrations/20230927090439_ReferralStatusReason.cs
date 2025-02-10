using Microsoft.EntityFrameworkCore.Migrations;
using System;


namespace WmsHub.Business.Migrations;

/// <inheritdoc />
public partial class ReferralStatusReason : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "ProviderRejectionReasons");

    migrationBuilder.DropTable(
        name: "ProviderRejectionReasonsAudit");

    migrationBuilder.CreateTable(
        name: "ReferralStatusReasons",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
          IsActive = table.Column<bool>(type: "bit", nullable: false),
          ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
          ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
          Groups = table.Column<int>(type: "int", nullable: false)
        },
        constraints: table => table.PrimaryKey("PK_ReferralStatusReasons", x => x.Id));

    migrationBuilder.CreateTable(
        name: "ReferralStatusReasonsAudit",
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
          Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
          Groups = table.Column<int>(type: "int", nullable: false)
        },
        constraints: table => table.PrimaryKey("PK_ReferralStatusReasonsAudit", x => x.AuditId));

    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('9d073603-f2a9-48b0-a3b3-6800fbd8cb76',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Language barrier',15)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('93f2d86a-b41f-ed11-ae83-501ac5963972',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','NHS number matches existing referral - patient is not eligible to be re-referred as yet',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('a2b71183-fec1-460b-abdc-463b4c1f01db',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user contact details unavailable/invalid',3)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('38ef3f83-b229-46e6-9e59-7c8ff0db730d',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user does not have a diagnosis of diabetes type 1, type 2, or hypertension',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('c34c37f1-fa77-4652-bf76-5383d2bb7813',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user does not have any digital capability or capacity',5)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('652df478-8275-4e03-bfe0-fef7c4a4a32d',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user does not meet eligibility criteria ',2)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('0fb61ec5-e3d5-4451-a927-d65b1d75735f',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user failed to respond after several contact attempts by RMC',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('f3db7d3b-fa59-4468-beef-c939f5570d2a',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user failed to respond despite several contact attempts by provider',2)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('604ba70d-ea85-41a7-9fb0-beee6e778fb4',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has a learning disability',15)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('67bab31a-3928-4d25-9e20-0e37e84c3482',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has not responded within 42 days of provider initiating first contact',3)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('745b8181-cb80-4752-aedb-5134e28cbd66',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to change provider - service provision not met',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('28c62be0-8a83-4313-ab96-9b581351ec08',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to change provider - technical reasons',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('0b4e4038-da6d-400d-934b-154d59217261',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to withdraw from the programme',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('d5d9c0f4-e77d-41f5-a0d1-1d5052f254ab',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to withdraw from the programme - bereavement',8)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('7c9c8e6d-930a-4a23-a642-d6c5affbb01f',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to withdraw from the programme - medical',8)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('b738d646-6ce4-4b8c-a893-e1c0b4c7c0da',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to withdraw from the programme - no motivation',12)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('cba77064-4044-4476-b93e-f3dd5651971b',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has requested to withdraw from the programme - no reason',12)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('457e1cd9-c0bb-4fcd-bc4c-d7cead5a4b17',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user has selected the wrong provider',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('0da3d7b4-5d5f-4bcf-b25c-2bad70f724b9',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user is already on a tier 2 weight management programme',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('7611fd48-117b-4a34-9159-b5f67855151d',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user is deceased',8)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('62719502-8d2a-4e09-aa94-038cae644657',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user was expecting to be referred to a different programme',4)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('18470477-97c4-4471-96b2-1282ce60d0d0',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user was unaware of the referral and does not wish to engage with the programme',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('e9470731-50b7-4629-8095-6489f12350ed',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user would like to join a face-to-face programme instead',5)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('21ed78cf-3a51-4242-bece-2bbb30a24fbb',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user''s BMI has fallen below 30',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('bff0c030-5cec-4682-aaeb-d17d0b89bc6d',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user''s BMI is below the threshold for their ethnicity',1)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('d7ace043-9fa5-4506-8b43-cdcaf30152e6',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user''s physical health capacity',15)");
    migrationBuilder.Sql($"INSERT INTO dbo.ReferralStatusReasons VALUES ('c070e614-f707-485d-992f-7f879b2fc3e5',1,'{DateTimeOffset.Now:u}','{Guid.Empty}','Service user''s registration information indicates they meet one of the exclusion criteria',1)");

  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "ReferralStatusReasons");

    migrationBuilder.DropTable(
        name: "ReferralStatusReasonsAudit");

    migrationBuilder.CreateTable(
        name: "ProviderRejectionReasons",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
          Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
          Group = table.Column<int>(type: "int", nullable: true),
          IsActive = table.Column<bool>(type: "bit", nullable: false),
          IsRmcReason = table.Column<bool>(type: "bit", nullable: true),
          ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
          ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
        },
        constraints: table => table.PrimaryKey("PK_ProviderRejectionReasons", x => x.Id));

    migrationBuilder.CreateTable(
        name: "ProviderRejectionReasonsAudit",
        columns: table => new
        {
          AuditId = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          AuditAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
          AuditDuration = table.Column<int>(type: "int", nullable: false),
          AuditErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
          AuditResult = table.Column<int>(type: "int", nullable: false),
          AuditSuccess = table.Column<bool>(type: "bit", nullable: false),
          Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
          Group = table.Column<int>(type: "int", nullable: true),
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          IsActive = table.Column<bool>(type: "bit", nullable: false),
          IsRmcReason = table.Column<bool>(type: "bit", nullable: true),
          ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
          ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
        },
        constraints: table => table.PrimaryKey("PK_ProviderRejectionReasonsAudit", x => x.AuditId));
  }
}
