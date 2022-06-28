using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class ConfigurationValues : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "ConfigurationValues",
          columns: table => new
          {
            Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
#if DEBUG_NOAE
            Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
#endif
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ConfigurationValues", x => x.Id);
          });

#if !DEBUG_NOAE
      AlwaysEncrypted.AddColumnsForMigration(
        AlwaysEncryptedMigrations.ConfigurationValues, migrationBuilder);
#endif

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "ConfigurationValues");
    }
  }
}
