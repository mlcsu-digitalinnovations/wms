using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
  public partial class ReferralImperialUnits : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<decimal>(
          name: "HeightFeet",
          table: "ReferralsAudit",
          type: "decimal(18,2)",
          nullable: true);

      migrationBuilder.AddColumn<decimal>(
          name: "HeightInches",
          table: "ReferralsAudit",
          type: "decimal(18,2)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "HeightUnits",
          table: "ReferralsAudit",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

      migrationBuilder.AddColumn<decimal>(
          name: "WeightPounds",
          table: "ReferralsAudit",
          type: "decimal(18,2)",
          nullable: true);

      migrationBuilder.AddColumn<decimal>(
          name: "WeightStones",
          table: "ReferralsAudit",
          type: "decimal(18,2)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "WeightUnits",
          table: "ReferralsAudit",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

      migrationBuilder.AddColumn<decimal>(
          name: "HeightFeet",
          table: "Referrals",
          type: "decimal(18,2)",
          nullable: true);

      migrationBuilder.AddColumn<decimal>(
          name: "HeightInches",
          table: "Referrals",
          type: "decimal(18,2)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "HeightUnits",
          table: "Referrals",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

      migrationBuilder.AddColumn<decimal>(
          name: "WeightPounds",
          table: "Referrals",
          type: "decimal(18,2)",
          nullable: true);

      migrationBuilder.AddColumn<decimal>(
          name: "WeightStones",
          table: "Referrals",
          type: "decimal(18,2)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "WeightUnits",
          table: "Referrals",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: true);

      migrationBuilder.Sql(
        "UPDATE dbo.Referrals SET HeightUnits = 'Metric', WeightUnits = 'Metric'");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "HeightFeet",
          table: "ReferralsAudit");

      migrationBuilder.DropColumn(
          name: "HeightInches",
          table: "ReferralsAudit");

      migrationBuilder.DropColumn(
          name: "HeightUnits",
          table: "ReferralsAudit");

      migrationBuilder.DropColumn(
          name: "WeightPounds",
          table: "ReferralsAudit");

      migrationBuilder.DropColumn(
          name: "WeightStones",
          table: "ReferralsAudit");

      migrationBuilder.DropColumn(
          name: "WeightUnits",
          table: "ReferralsAudit");

      migrationBuilder.DropColumn(
          name: "HeightFeet",
          table: "Referrals");

      migrationBuilder.DropColumn(
          name: "HeightInches",
          table: "Referrals");

      migrationBuilder.DropColumn(
          name: "HeightUnits",
          table: "Referrals");

      migrationBuilder.DropColumn(
          name: "WeightPounds",
          table: "Referrals");

      migrationBuilder.DropColumn(
          name: "WeightStones",
          table: "Referrals");

      migrationBuilder.DropColumn(
          name: "WeightUnits",
          table: "Referrals");
    }
  }
}
