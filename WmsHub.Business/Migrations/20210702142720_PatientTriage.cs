using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class PatientTriage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<string>(
            //    name: "DelayReason",
            //    table: "ReferralsAudit",
            //    type: "nvarchar(2000)",
            //    maxLength: 2000,
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "DelayReason",
            //    table: "Referrals",
            //    type: "nvarchar(2000)",
            //    maxLength: 2000,
            //    nullable: true);

            migrationBuilder.CreateTable(
                name: "PatientTriages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TriageSection = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descriptions = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    CheckSum = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientTriages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientTriagesAudit",
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
                    TriageSection = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descriptions = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    CheckSum = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientTriagesAudit", x => x.AuditId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientTriages");

            migrationBuilder.DropTable(
                name: "PatientTriagesAudit");

            //migrationBuilder.DropColumn(
            //    name: "DelayReason",
            //    table: "ReferralsAudit");

            //migrationBuilder.DropColumn(
            //    name: "DelayReason",
            //    table: "Referrals");
        }
    }
}
