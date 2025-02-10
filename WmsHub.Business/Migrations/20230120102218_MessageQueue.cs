using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class MessageQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessagesQueue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiKeyType = table.Column<string>(type: "nvarchar(max)", nullable: false),
#if DEBUG_NOAE
                    PersonalisationJson = table.Column<string>(type: "nvarchar(4000)", nullable: true),
#endif
                    ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SendResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
#if DEBIG_NOAE
                    SendTo = table.Column<string>(type: "nvarchar(200)", nullable: true),
#endif
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_MessagesQueue", x => x.Id);
                });
#if !DEBUG_NOAE
      AlwaysEncrypted.AddColumnsForMigration(
        AlwaysEncryptedMigrations.MessagesQueue, migrationBuilder);
#endif
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessagesQueue");
        }
    }
}
