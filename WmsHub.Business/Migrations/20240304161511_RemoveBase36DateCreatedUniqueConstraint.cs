using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBase36DateCreatedUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TextMessages_Base36DateSent",
                table: "TextMessages");

            migrationBuilder.CreateIndex(
                name: "IX_TextMessages_Base36DateSent",
                table: "TextMessages",
                column: "Base36DateSent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
