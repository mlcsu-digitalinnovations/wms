using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkIdsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReferralQuestionnaires_NotificationKey",
                table: "ReferralQuestionnaires");

            migrationBuilder.DropIndex(
                name: "IX_MessagesQueue_Base36DateSentLinkId",
                table: "MessagesQueue");

            migrationBuilder.RenameColumn(
                name: "Base36DateSent",
                table: "TextMessagesAudit",
                newName: "ServiceUserLinkId");

            migrationBuilder.RenameColumn(
                name: "Base36DateSent",
                table: "TextMessages",
                newName: "ServiceUserLinkId");

            migrationBuilder.RenameIndex(
                name: "IX_TextMessages_Base36DateSent",
                table: "TextMessages",
                newName: "IX_TextMessages_ServiceUserLinkId");

            migrationBuilder.RenameColumn(
                name: "Base36DateSentLinkId",
                table: "MessagesQueue",
                newName: "ServiceUserLinkId");

            migrationBuilder.CreateTable(
                name: "LinkIds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkIds", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO dbo.LinkIds SELECT ServiceUserLinkId, 1 FROM dbo.TextMessages");
            migrationBuilder.Sql("INSERT INTO dbo.LinkIds SELECT NotificationKey, 1 FROM dbo.ReferralQuestionnaires");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralQuestionnaires_NotificationKey",
                table: "ReferralQuestionnaires",
                column: "NotificationKey");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesQueue_ServiceUserLinkId",
                table: "MessagesQueue",
                column: "ServiceUserLinkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinkIds");

            migrationBuilder.DropIndex(
                name: "IX_ReferralQuestionnaires_NotificationKey",
                table: "ReferralQuestionnaires");

            migrationBuilder.DropIndex(
                name: "IX_MessagesQueue_ServiceUserLinkId",
                table: "MessagesQueue");

            migrationBuilder.RenameColumn(
                name: "ServiceUserLinkId",
                table: "TextMessagesAudit",
                newName: "Base36DateSent");

            migrationBuilder.RenameColumn(
                name: "ServiceUserLinkId",
                table: "TextMessages",
                newName: "Base36DateSent");

            migrationBuilder.RenameIndex(
                name: "IX_TextMessages_ServiceUserLinkId",
                table: "TextMessages",
                newName: "IX_TextMessages_Base36DateSent");

            migrationBuilder.RenameColumn(
                name: "ServiceUserLinkId",
                table: "MessagesQueue",
                newName: "Base36DateSentLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralQuestionnaires_NotificationKey",
                table: "ReferralQuestionnaires",
                column: "NotificationKey",
                unique: true,
                filter: "[NotificationKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesQueue_Base36DateSentLinkId",
                table: "MessagesQueue",
                column: "Base36DateSentLinkId",
                unique: true,
                filter: "[Base36DateSentLinkId] IS NOT NULL");
        }
    }
}
