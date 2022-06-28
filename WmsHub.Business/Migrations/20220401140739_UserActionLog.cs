using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class UserActionLog : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "UserActionLogs",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Controller = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#if DEBUG_NOAE
            IpAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#endif
            Method = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
#if DEBUG_NOAE
            Request = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
#endif
            RequestAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_UserActionLogs", x => x.Id);
          });

#if !DEBUG_NOAE
      AlwaysEncrypted.AddColumnsForMigration(
        AlwaysEncryptedMigrations.UserActionLog, migrationBuilder);
#endif
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "UserActionLogs");
    }
  }
}
