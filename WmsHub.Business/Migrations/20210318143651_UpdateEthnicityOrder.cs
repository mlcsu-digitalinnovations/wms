using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class UpdateEthnicityOrder : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=2, " +
        "DisplayOrder=1 WHERE Id='3185A21D-2FD4-4313-4A59-43DB28A2E89A'");
      migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=2, " +
        "DisplayOrder=2 WHERE Id='EDFE5D64-E5D8-9D27-F9C5-DC953D351CF7'");
      migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=2, " +
        "DisplayOrder=3 WHERE Id='279DC2CB-6F4B-96BC-AE72-B96BF7A2579A'");
      migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=2, " +
        "DisplayOrder=4 WHERE Id='4E84EFCD-3DBA-B459-C302-29BCBD9E8E64'");

      migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=3, " +
        "DisplayOrder=1 WHERE Id='3C69F5AE-073F-F180-3CAC-2197EB73E369'");
      migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=3, " +
        "DisplayOrder=2 WHERE Id='76D69A87-D9A7-EAC6-2E2D-A6017D02E04F'");
      migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=3, " +
        "DisplayOrder=3 WHERE Id='5BF8BFAB-DAB1-D472-51CA-9CF0CB056D3F'");
      migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=3, " +
        "DisplayOrder=4 WHERE Id='EFC61F30-F872-FA71-9709-1A416A51982F'");
      migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=3, " +
        "DisplayOrder=5 WHERE Id='CB5CA465-C397-A34F-F32B-729A38932E0E'");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
  }
}
