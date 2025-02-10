using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class ProviderRejectionReasonGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Group",
                table: "ProviderRejectionReasonsAudit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Group",
                table: "ProviderRejectionReasons",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Group",
                table: "ProviderRejectionReasonsAudit");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "ProviderRejectionReasons");
        }
    }
}
