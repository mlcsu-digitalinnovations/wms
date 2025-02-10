using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class DelayReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
#if DEBUG_NOAE
            migrationBuilder.AddColumn<string>(
                name: "DelayReason",
                table: "ReferralsAudit",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DelayReason",
                table: "Referrals",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
#endif

#if !DEBUG_NOAE
          AlwaysEncrypted.AddColumnsForMigration(
            AlwaysEncryptedMigrations.DelayReason, migrationBuilder);
#endif
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelayReason",
                table: "ReferralsAudit");

            migrationBuilder.DropColumn(
                name: "DelayReason",
                table: "Referrals");
        }
    }
}
