using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WmsHub.Business.Migrations
{
    public partial class ElectiveCarePostErrors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectiveCarePostErrors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    TrustOdsCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrustUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectiveCarePostErrors", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectiveCarePostErrors");
        }
    }
}
