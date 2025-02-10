using Microsoft.EntityFrameworkCore.Migrations;
using System;
#nullable disable

namespace WmsHub.Business.Migrations
{
  /// <inheritdoc />
  public partial class MskOrganisationsSiteName : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
          name: "SiteName",
          table: "MskOrganisationsAudit",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: false,
          defaultValue: "");

      migrationBuilder.AddColumn<string>(
          name: "SiteName",
          table: "MskOrganisations",
          type: "nvarchar(200)",
          maxLength: 200,
          nullable: false,
          defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "SiteName",
          table: "MskOrganisationsAudit");

      migrationBuilder.DropColumn(
          name: "SiteName",
          table: "MskOrganisations");
    }
  }
}
