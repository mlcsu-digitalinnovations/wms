using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class Ethnicities_MinimumBmi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinimumBmi",
                table: "EthnicitiesAudit",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumBmi",
                table: "Ethnicities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.Sql(
              "UPDATE EthnicitiesAudit SET MinimumBmi = 30 WHERE TriageName = 'White'");
            migrationBuilder.Sql(
              "UPDATE EthnicitiesAudit SET MinimumBmi = 27.5 WHERE NOT TriageName = 'White'");
            migrationBuilder.Sql(
              "UPDATE Ethnicities SET MinimumBmi = 30 WHERE TriageName = 'White'");
            migrationBuilder.Sql(
              "UPDATE Ethnicities SET MinimumBmi = 27.5 WHERE NOT TriageName = 'White'");
    }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumBmi",
                table: "EthnicitiesAudit");

            migrationBuilder.DropColumn(
                name: "MinimumBmi",
                table: "Ethnicities");
        }
    }
}
