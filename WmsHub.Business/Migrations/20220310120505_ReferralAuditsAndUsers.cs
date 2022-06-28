using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
  public partial class ReferralAuditsAndUsers : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateIndex(
          name: "IX_ReferralsAudit_Id",
          table: "ReferralsAudit",
          column: "Id");

      migrationBuilder.CreateIndex(
          name: "IX_ReferralsAudit_ModifiedByUserId",
          table: "ReferralsAudit",
          column: "ModifiedByUserId");

      migrationBuilder.AddForeignKey(
          name: "FK_ReferralsAudit_Referrals_Id",
          table: "ReferralsAudit",
          column: "Id",
          principalTable: "Referrals",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);

      // Removing this because we don't want a foreign key relationship
      // between the ReferralsAudit table and the UsersStore
      //migrationBuilder.AddForeignKey(
      //    name: "FK_ReferralsAudit_UsersStore_ModifiedByUserId",
      //    table: "ReferralsAudit",
      //    column: "ModifiedByUserId",
      //    principalTable: "UsersStore",
      //    principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_ReferralsAudit_Referrals_Id",
          table: "ReferralsAudit");

      // Removing this because we don't want a foreign key relationship
      // between the ReferralsAudit table and the UsersStore
      //migrationBuilder.DropForeignKey(
      //    name: "FK_ReferralsAudit_UsersStore_ModifiedByUserId",
      //    table: "ReferralsAudit");

      migrationBuilder.DropIndex(
                name: "IX_ReferralsAudit_Id",
                table: "ReferralsAudit");

      migrationBuilder.DropIndex(
          name: "IX_ReferralsAudit_ModifiedByUserId",
          table: "ReferralsAudit");
    }
  }
}
