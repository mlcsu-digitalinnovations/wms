using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WmsHub.Business.Migrations
{
  public partial class ServiceUserUiSessionCache : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "ServiceUserUiSessionCache",
        columns: table => new
        {
          Id = table.Column<string>(type: "nvarchar(449)", maxLength: 449, nullable: false),
          Value = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
          ExpiresAtTime = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
          SlidingExpirationInSeconds = table.Column<int>(type: "bigint", nullable: true),
          AbsoluteExpiration = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ServiceUserUiSessionCache", x => x.Id);
        });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "ServiceUserUiSessionCache");
    }
  }
}
