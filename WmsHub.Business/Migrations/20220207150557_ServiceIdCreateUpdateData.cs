using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class ServiceIdCreateUpdateData : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      // Update all the existing referral's OfferedCompletionLevel field
      // to the TriagedCompletionLevel
      migrationBuilder.Sql(
        "UPDATE dbo.Referrals " +
        "SET ServiceId = '6708596' " +
        "WHERE ServiceId IS NULL AND ReferralSource = 'GpReferral'");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
  }
}
