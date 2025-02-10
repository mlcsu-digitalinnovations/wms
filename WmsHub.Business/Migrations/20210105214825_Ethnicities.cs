using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WmsHub.Business.Migrations
{
    public partial class Ethnicities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ethnicities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TriageName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ethnicities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EthnicitiesAudit",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditDuration = table.Column<int>(type: "int", nullable: false),
                    AuditErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditResult = table.Column<int>(type: "int", nullable: false),
                    AuditSuccess = table.Column<bool>(type: "bit", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TriageName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EthnicitiesAudit", x => x.AuditId);
                });

            migrationBuilder.InsertData(
                table: "Ethnicities",
                columns: new[] { "Id", "DisplayName", "GroupName", "IsActive", "ModifiedAt", "ModifiedByUserId", "OldName", "TriageName" },
                values: new object[,]
                {
                          { new Guid("5dc90d60-f03c-3ce6-72f6-a34d4e6f163b"), "English, Welsh, Scottish, Northern Irish or British", "White", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "British or mixed British", "White" },
                          { new Guid("934a2fa6-f541-60f1-d08d-46f5e647a28d"), "Arab", "Other ethnic group", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Other - ethnic category", "Other" },
                          { new Guid("e0694f9a-2d9e-bef6-2f46-6eb9fb7891ad"), "Any other Black, African or Caribbean background", "Black, African, Caribbean or Black British", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Other Black background", "Black" },
                          { new Guid("36fe1d6a-3b04-5a31-fbd9-8d378c2cb86a"), "Caribbean", "Black, African, Caribbean or Black British", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Caribbean", "Black" },
                          { new Guid("f6c29207-a3fc-163b-94bc-2ce840af9396"), "African", "Black, African, Caribbean or Black British", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "African", "Black" },
                          { new Guid("cb5ca465-c397-a34f-f32b-729a38932e0e"), "Any other Asian background", "Asian or Asian British", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Other Asian background", "Asian" },
                          { new Guid("efc61f30-f872-fa71-9709-1a416a51982f"), "Chinese", "Asian or Asian British", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Chinese", "Asian" },
                          { new Guid("5bf8bfab-dab1-d472-51ca-9cf0cb056d3f"), "Bangladeshi", "Asian or Asian British", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Pakistani or British Pakistani", "Asian" },
                          { new Guid("d15b2787-7926-1ef6-704e-1012f9298ae1"), "Any other ethnic group", "Other ethnic group", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Other - ethnic category", "Other" },
                          { new Guid("76d69a87-d9a7-eac6-2e2d-a6017d02e04f"), "Pakistani", "Asian or Asian British", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Pakistani or British Pakistani", "Asian" },
                          { new Guid("4e84efcd-3dba-b459-c302-29bcbd9e8e64"), "Any other Mixed or Multiple ethnic background", "Mixed or Multiple ethnic groups", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Other Mixed background", "Mixed" },
                          { new Guid("279dc2cb-6f4b-96bc-ae72-b96bf7a2579a"), "White and Asian", "Mixed or Multiple ethnic groups", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "White and Asian", "Mixed" },
                          { new Guid("edfe5d64-e5d8-9d27-f9c5-dc953d351cf7"), "White and Black African", "Mixed or Multiple ethnic groups", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "White and Black African", "Mixed" },
                          { new Guid("3185a21d-2fd4-4313-4a59-43db28a2e89a"), "White and Black Caribbean", "Mixed or Multiple ethnic groups", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "White and Black Caribbean", "Mixed" },
                          { new Guid("75e8313c-bfdf-5abf-b6da-d6ca64138cf4"), "Any other White background", "White", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Other White background", "White" },
                          { new Guid("a1b8c48b-fa12-e001-9f8e-3c9ba9d3d065"), "Gypsy or Irish Traveller", "White", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Other White background", "White" },
                          { new Guid("5d2b37fd-24c4-7572-4aea-d437c6e17318"), "Irish", "White", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Irish", "White" },
                          { new Guid("3c69f5ae-073f-f180-3cac-2197eb73e369"), "Indian", "Asian or Asian British", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "Indian or British Indian", "Asian" },
                          { new Guid("95b0feb5-5ece-98ed-1269-c71e327e98c5"), "I do not wish to Disclose my Ethnicity", "I do not wish to Disclose my Ethnicity", true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, "Other" }
                });
    }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ethnicities");

            migrationBuilder.DropTable(
                name: "EthnicitiesAudit");
        }
    }
}
