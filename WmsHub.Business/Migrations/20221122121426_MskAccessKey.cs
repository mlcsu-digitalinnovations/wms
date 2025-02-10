using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class MskAccessKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MskAccessKeys",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccessKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MskAccessKeys", x => x.Email);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MskAccessKeys");
        }
    }
}
