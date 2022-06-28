using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class ReInitilise : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "CallsAudit",
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
#if DEBUG_NOAE
            Number = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
            Sent = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Called = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_CallsAudit", x => x.AuditId);
          });

      migrationBuilder.CreateTable(
          name: "Logs",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
            MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Level = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
            TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
            Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Properties = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Logs", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "Referrals",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
            IsActive = table.Column<bool>(type: "bit", nullable: false),
            ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Status = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            StatusReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
#if DEBUG_NOAE
             NhsNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
            ReferringGpPracticeNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#if DEBUG_NOAE
             Ubrn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
             FamilyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
             GivenName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
             Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
             Postcode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
             Address1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
             Address2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
             Town = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
             Telephone = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
             Mobile = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
             Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
             DateOfBirth = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
#endif
            Sex = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Gender = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            DateOfReferral = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            ConsentForFutureContactForEvaluation = table.Column<bool>(type: "bit", nullable: true),
            Ethnicity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            HasDisability = table.Column<bool>(type: "bit", nullable: true),
            DisabilityDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
            HasHypertension = table.Column<bool>(type: "bit", nullable: true),
            HasDiabetesType1 = table.Column<bool>(type: "bit", nullable: true),
            HasDiabetesType2 = table.Column<bool>(type: "bit", nullable: true),
            HeightCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            WeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            CalculatedBmiAtRegistration = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            IsVulnerable = table.Column<bool>(type: "bit", nullable: true),
#if DEBUG_NOAE
            VulnerableDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
#endif
            HasRegisteredSeriousMentalIllness = table.Column<bool>(type: "bit", nullable: true),
            TriagedCompletionLevel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            TriagedWeightedLevel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Referrals", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "ReferralsAudit",
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
            Status = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            StatusReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
#if DEBUG_NOAE
            NhsNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
            ReferringGpPracticeNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#if DEBUG_NOAE
            Ubrn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            FamilyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            GivenName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Postcode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Address1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Address2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Town = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Telephone = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Mobile = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            DateOfBirth = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
#endif
            Sex = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Gender = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            DateOfReferral = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            ConsentForFutureContactForEvaluation = table.Column<bool>(type: "bit", nullable: true),
            Ethnicity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            HasDisability = table.Column<bool>(type: "bit", nullable: true),
            DisabilityDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
            HasHypertension = table.Column<bool>(type: "bit", nullable: true),
            HasDiabetesType1 = table.Column<bool>(type: "bit", nullable: true),
            HasDiabetesType2 = table.Column<bool>(type: "bit", nullable: true),
            HeightCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            WeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            CalculatedBmiAtRegistration = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            IsVulnerable = table.Column<bool>(type: "bit", nullable: true),
#if DEBUG_NOAE
            VulnerableDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
#endif
            HasRegisteredSeriousMentalIllness = table.Column<bool>(type: "bit", nullable: true),
            TriagedCompletionLevel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            TriagedWeightedLevel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ReferralsAudit", x => x.AuditId);
          });

      migrationBuilder.CreateTable(
          name: "RequestResponseLog",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Controller = table.Column<string>(type: "nvarchar(max)", nullable: true),
#if DEBUG_NOAE
            Request = table.Column<string>(type: "nvarchar(max)", nullable: true),
#endif
            RequestAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
            ResponseAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_RequestResponseLog", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "TextMessagesAudit",
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
#if DEBUG_NOAE
            Number = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
            Sent = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Received = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_TextMessagesAudit", x => x.AuditId);
          });

      migrationBuilder.CreateTable(
          name: "Calls",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
            IsActive = table.Column<bool>(type: "bit", nullable: false),
            ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
#if DEBUG_NOAE
            Number = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
            Sent = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Called = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Calls", x => x.Id);
            table.ForeignKey(
                      name: "FK_Calls_Referrals_ReferralId",
                      column: x => x.ReferralId,
                      principalTable: "Referrals",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "TextMessages",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
            IsActive = table.Column<bool>(type: "bit", nullable: false),
            ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
#if DEBUG_NOAE
            Number = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
            Sent = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Received = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_TextMessages", x => x.Id);
            table.ForeignKey(
                      name: "FK_TextMessages_Referrals_ReferralId",
                      column: x => x.ReferralId,
                      principalTable: "Referrals",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

#if !DEBUG_NOAE
      AlwaysEncrypted.AddColumnsForMigration(
        AlwaysEncryptedMigrations.ReInitialise, migrationBuilder);
#endif

      migrationBuilder.CreateIndex(
          name: "IX_Calls_ReferralId",
          table: "Calls",
          column: "ReferralId");

      migrationBuilder.CreateIndex(
          name: "IX_Referrals_Email",
          table: "Referrals",
          column: "Email");

      migrationBuilder.CreateIndex(
          name: "IX_Referrals_FamilyName",
          table: "Referrals",
          column: "FamilyName");

      migrationBuilder.CreateIndex(
          name: "IX_Referrals_Mobile",
          table: "Referrals",
          column: "Mobile");

      migrationBuilder.CreateIndex(
          name: "IX_Referrals_NhsNumber",
          table: "Referrals",
          column: "NhsNumber");

      migrationBuilder.CreateIndex(
          name: "IX_Referrals_Postcode",
          table: "Referrals",
          column: "Postcode");

      migrationBuilder.CreateIndex(
          name: "IX_Referrals_Telephone",
          table: "Referrals",
          column: "Telephone");

      migrationBuilder.CreateIndex(
          name: "IX_Referrals_Ubrn",
          table: "Referrals",
          column: "Ubrn");

      migrationBuilder.CreateIndex(
          name: "IX_TextMessages_ReferralId",
          table: "TextMessages",
          column: "ReferralId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "Calls");

      migrationBuilder.DropTable(
          name: "CallsAudit");

      migrationBuilder.DropTable(
          name: "Logs");

      migrationBuilder.DropTable(
          name: "ReferralsAudit");

      migrationBuilder.DropTable(
          name: "RequestResponseLog");

      migrationBuilder.DropTable(
          name: "TextMessages");

      migrationBuilder.DropTable(
          name: "TextMessagesAudit");

      migrationBuilder.DropTable(
          name: "Referrals");
    }
  }
}
