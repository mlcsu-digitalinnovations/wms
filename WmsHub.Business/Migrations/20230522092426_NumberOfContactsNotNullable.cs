using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
  public partial class NumberOfContactsNotNullable : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql("UPDATE dbo.Referrals SET NumberOfContacts = 0 WHERE NumberOfContacts IS NULL");
      migrationBuilder.Sql("UPDATE dbo.ReferralsAudit SET NumberOfContacts = 0 WHERE NumberOfContacts IS NULL");

      migrationBuilder.AlterColumn<int>(
          name: "NumberOfContacts",
          table: "ReferralsAudit",
          type: "int",
          nullable: false,
          defaultValue: 0,
          oldClrType: typeof(int),
          oldType: "int",
          oldNullable: true);

      migrationBuilder.AlterColumn<int>(
          name: "NumberOfContacts",
          table: "Referrals",
          type: "int",
          nullable: false,
          defaultValue: 0,
          oldClrType: typeof(int),
          oldType: "int",
          oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<int>(
          name: "NumberOfContacts",
          table: "ReferralsAudit",
          type: "int",
          nullable: true,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<int>(
          name: "NumberOfContacts",
          table: "Referrals",
          type: "int",
          nullable: true,
          oldClrType: typeof(int),
          oldType: "int");
    }
  }
}
