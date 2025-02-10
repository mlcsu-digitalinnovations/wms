using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class EthnicityDisplayOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "EthnicitiesAudit",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "GroupOrder",
                table: "EthnicitiesAudit",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Ethnicities",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "GroupOrder",
                table: "Ethnicities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=1, DisplayOrder=1 WHERE Id='5DC90D60-F03C-3CE6-72F6-A34D4E6F163B'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=1, DisplayOrder=2 WHERE Id='5D2B37FD-24C4-7572-4AEA-D437C6E17318'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=1, DisplayOrder=3 WHERE Id='A1B8C48B-FA12-E001-9F8E-3C9BA9D3D065'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=1, DisplayOrder=4 WHERE Id='75E8313C-BFDF-5ABF-B6DA-D6CA64138CF4'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=2, DisplayOrder=1 WHERE Id='EDFE5D64-E5D8-9D27-F9C5-DC953D351CF7'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=2, DisplayOrder=2 WHERE Id='279DC2CB-6F4B-96BC-AE72-B96BF7A2579A'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=2, DisplayOrder=3 WHERE Id='3185A21D-2FD4-4313-4A59-43DB28A2E89A'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=2, DisplayOrder=4 WHERE Id='4E84EFCD-3DBA-B459-C302-29BCBD9E8E64'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=3, DisplayOrder=1 WHERE Id='5BF8BFAB-DAB1-D472-51CA-9CF0CB056D3F'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=3, DisplayOrder=2 WHERE Id='EFC61F30-F872-FA71-9709-1A416A51982F'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=3, DisplayOrder=3 WHERE Id='3C69F5AE-073F-F180-3CAC-2197EB73E369'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=3, DisplayOrder=4 WHERE Id='76D69A87-D9A7-EAC6-2E2D-A6017D02E04F'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=3, DisplayOrder=5 WHERE Id='CB5CA465-C397-A34F-F32B-729A38932E0E'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=4, DisplayOrder=1 WHERE Id='F6C29207-A3FC-163B-94BC-2CE840AF9396'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=4, DisplayOrder=2 WHERE Id='36FE1D6A-3B04-5A31-FBD9-8D378C2CB86A'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=4, DisplayOrder=3 WHERE Id='E0694F9A-2D9E-BEF6-2F46-6EB9FB7891AD'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=5, DisplayOrder=1 WHERE Id='934A2FA6-F541-60F1-D08D-46F5E647A28D'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=5, DisplayOrder=2 WHERE Id='D15B2787-7926-1EF6-704E-1012F9298AE1'");
            migrationBuilder.Sql("UPDATE dbo.Ethnicities SET GroupOrder=6, DisplayOrder=1 WHERE Id='95B0FEB5-5ECE-98ED-1269-C71E327E98C5'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GroupOrder",
                table: "EthnicitiesAudit",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "EthnicitiesAudit",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GroupOrder",
                table: "Ethnicities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Ethnicities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
