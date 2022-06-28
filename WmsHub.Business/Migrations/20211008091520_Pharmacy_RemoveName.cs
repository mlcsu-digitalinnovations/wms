using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class Pharmacy_RemoveName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "PharmaciesAudit");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Pharmacies");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PharmaciesAudit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Pharmacies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
