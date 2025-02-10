using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations;

public partial class RefactorEthnicityEntity : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.RenameColumn(
        name: "OldName",
        table: "EthnicitiesAudit",
        newName: "Census2001");

    migrationBuilder.RenameColumn(
        name: "OldName",
        table: "Ethnicities",
        newName: "Census2001");

    migrationBuilder.AddColumn<string>(
        name: "NhsDataDictionary2001Description",
        table: "EthnicitiesAudit",
        type: "nvarchar(max)",
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "NhsDataDictionary2001Code",
        table: "EthnicitiesAudit",
        type: "nvarchar(max)",
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "NhsDataDictionary2001Description",
        table: "Ethnicities",
        type: "nvarchar(max)",
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "NhsDataDictionary2001Code",
        table: "Ethnicities",
        type: "nvarchar(max)",
        nullable: true);

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'White - British', " +
      "NhsDataDictionary2001Code = 'A', " +
      "Census2001 = 'British' " + 
      "WHERE Id='5DC90D60-F03C-3CE6-72F6-A34D4E6F163B'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'White - Irish', " +
      "NhsDataDictionary2001Code = 'B' " +
      "WHERE Id='5D2B37FD-24C4-7572-4AEA-D437C6E17318'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = NULL, " +
      "NhsDataDictionary2001Code = NULL, " +
      "Census2001 = NULL " +
      "WHERE Id='A1B8C48B-FA12-E001-9F8E-3C9BA9D3D065'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'White - Any other White background', " +
      "NhsDataDictionary2001Code = 'C', " +
      "Census2001 = 'Any other White background' " + 
      "WHERE Id='75E8313C-BFDF-5ABF-B6DA-D6CA64138CF4'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Mixed - White and Black Caribbean', " +
      "NhsDataDictionary2001Code = 'D' " +
      "WHERE Id='3185A21D-2FD4-4313-4A59-43DB28A2E89A'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Mixed - White and Black African', " +
      "NhsDataDictionary2001Code = 'E' " +
      "WHERE Id='EDFE5D64-E5D8-9D27-F9C5-DC953D351CF7'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Mixed - White and Asian', " +
      "NhsDataDictionary2001Code = 'F' " +
      "WHERE Id='279DC2CB-6F4B-96BC-AE72-B96BF7A2579A'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Mixed - Any other mixed background', " +
      "NhsDataDictionary2001Code = 'G', " +
      "Census2001 = 'Any other Mixed background' " +
      "WHERE Id='4E84EFCD-3DBA-B459-C302-29BCBD9E8E64'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Asian or Asian British - Indian', " +
      "NhsDataDictionary2001Code = 'H', " +
      "Census2001 = 'Indian' " +
      "WHERE Id='3C69F5AE-073F-F180-3CAC-2197EB73E369'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Asian or Asian British - Pakistani', " +
      "NhsDataDictionary2001Code = 'J', " +
      "Census2001 = 'Pakistani' " +
      "WHERE Id='76D69A87-D9A7-EAC6-2E2D-A6017D02E04F'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Asian or Asian British - Bangladeshi', " +
      "NhsDataDictionary2001Code = 'K', " +
      "Census2001 = 'Bangladeshi' " +
      "WHERE Id='5BF8BFAB-DAB1-D472-51CA-9CF0CB056D3F'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Other Ethnic Groups - Chinese', " +
      "NhsDataDictionary2001Code = 'R' " +
      "WHERE Id='EFC61F30-F872-FA71-9709-1A416A51982F'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Asian or Asian British - Any other Asian background', " +
      "NhsDataDictionary2001Code = 'L', " +
      "Census2001 = 'Any other Asian background' " +
      "WHERE Id='CB5CA465-C397-A34F-F32B-729A38932E0E'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Black or Black British - African', " +
      "NhsDataDictionary2001Code = 'N'" +
      " WHERE Id='F6C29207-A3FC-163B-94BC-2CE840AF9396'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Black or Black British - Caribbean', " +
      "NhsDataDictionary2001Code = 'M' " +
      "WHERE Id='36FE1D6A-3B04-5A31-FBD9-8D378C2CB86A'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Black or Black British - Any other Black background', " +
      "NhsDataDictionary2001Code = 'P', " +
      "Census2001 = 'Any other Black background' " +
      "WHERE Id='E0694F9A-2D9E-BEF6-2F46-6EB9FB7891AD'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = NULL, " +
      "NhsDataDictionary2001Code = NULL, " +
      "Census2001 = NULL " +
      "WHERE Id='934A2FA6-F541-60F1-D08D-46F5E647A28D'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Other Ethnic Groups - Any other ethnic group', " +
      "NhsDataDictionary2001Code = 'S', " +
      "Census2001 = 'Any other' " +
      "WHERE Id='D15B2787-7926-1EF6-704E-1012F9298AE1'");

    migrationBuilder.Sql(
      "UPDATE dbo.Ethnicities " +
      "SET NhsDataDictionary2001Description = 'Not stated', " +
      "NhsDataDictionary2001Code = 'Z', " +
      "GroupName = 'I do not wish to disclose my ethnicity', " +
      "DisplayName = 'I do not wish to disclose my ethnicity' " +
      "WHERE Id='95B0FEB5-5ECE-98ED-1269-C71E327E98C5'");
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
        name: "NhsDataDictionary2001Description",
        table: "EthnicitiesAudit");

    migrationBuilder.DropColumn(
        name: "NhsDataDictionary2001Code",
        table: "EthnicitiesAudit");

    migrationBuilder.DropColumn(
        name: "NhsDataDictionary2001Description",
        table: "Ethnicities");

    migrationBuilder.DropColumn(
        name: "NhsDataDictionary2001Code",
        table: "Ethnicities");

    migrationBuilder.RenameColumn(
        name: "Census2001",
        table: "EthnicitiesAudit",
        newName: "OldName");

    migrationBuilder.RenameColumn(
        name: "Census2001",
        table: "Ethnicities",
        newName: "OldName");
  }
}
