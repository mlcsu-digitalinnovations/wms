using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class QuestionnaireConsentToShare : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConsentToShare",
                table: "ReferralQuestionnairesAudit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentToShare",
                table: "ReferralQuestionnaires",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentToShare",
                table: "ReferralQuestionnairesAudit");

            migrationBuilder.DropColumn(
                name: "ConsentToShare",
                table: "ReferralQuestionnaires");
        }
    }
}
