using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class TextMessageBase36DateSentUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TextMessages_Base36DateSent",
                table: "TextMessages");

            migrationBuilder.CreateIndex(
                name: "IX_TextMessages_Base36DateSent",
                table: "TextMessages",
                column: "Base36DateSent",
                unique: true,
                filter: "[Base36DateSent] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TextMessages_Base36DateSent",
                table: "TextMessages");

            migrationBuilder.CreateIndex(
                name: "IX_TextMessages_Base36DateSent",
                table: "TextMessages",
                column: "Base36DateSent");
        }
    }
}
