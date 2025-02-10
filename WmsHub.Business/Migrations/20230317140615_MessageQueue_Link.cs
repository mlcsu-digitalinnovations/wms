using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class MessageQueue_Link : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Base36DateSentLinkId",
                table: "MessagesQueue",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessagesQueue_Base36DateSentLinkId",
                table: "MessagesQueue",
                column: "Base36DateSentLinkId",
                unique: true,
                filter: "[Base36DateSentLinkId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessagesQueue_Base36DateSentLinkId",
                table: "MessagesQueue");

            migrationBuilder.DropColumn(
                name: "Base36DateSentLinkId",
                table: "MessagesQueue");
        }
    }
}
