using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class OfferedCompletionLevelUpdateData : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      // Update all the existing referral's OfferedCompletionLevel field
      // to the TriagedCompletionLevel
      migrationBuilder.Sql(
        "UPDATE dbo.Referrals " +
        "SET OfferedCompletionLevel = TriagedCompletionLevel " +
        "WHERE OfferedCompletionLevel IS NULL " +
        "AND TriagedCompletionLevel IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
  }
}
