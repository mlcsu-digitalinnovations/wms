using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class ReferralCreateDateUpdateData : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
        "UPDATE dbo.Referrals SET CreatedDate = RA.CreatedDate FROM " +
        "dbo.Referrals R JOIN (SELECT Id, MIN(ModifiedAt) AS CreatedDate " +
        "FROM dbo.ReferralsAudit GROUP BY Id) RA ON R.Id = RA.Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
  }
}
