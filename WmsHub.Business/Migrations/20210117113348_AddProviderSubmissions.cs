using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class AddProviderSubmissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Coaching = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Measure = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderSubmissions_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProviderSubmissionsAudit",
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
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Coaching = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Measure = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderSubmissionsAudit", x => x.AuditId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSubmissions_ProviderId",
                table: "ProviderSubmissions",
                column: "ProviderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderSubmissions");

            migrationBuilder.DropTable(
                name: "ProviderSubmissionsAudit");
        }
    }
}
