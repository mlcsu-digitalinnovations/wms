using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WmsHub.Business.Migrations
{
  public partial class TestProviderData : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.InsertData(
          table: "Providers",
          columns: new[] { "Id", "Name", "Summary", "Website", "Logo", "Level1",
            "Level2", "Level3", "IsActive", "ModifiedAt", "ModifiedByUserId",
            "ApiKey", "ApiKeyExpires"},
          values: new object[,]
          {
            { new Guid("e778091e-e5b7-47b0-90d2-e28d6236a5f5"),
              "StoVoKor",
              "StoVokor Summary",
              "StoVokor Website",
              "",
              true,
              true,
              true,
              true,
              DateTimeOffset.Now,
              Guid.Empty,
              "450d1ed79e1657efcd81d9f2917620e77ccba748d0ecaa9fcc13578ab531b5d5",
              "2048-06-14 14:58:12Z"},
            { new Guid("ebe236fa-d293-41ec-9b34-9f1bfae992ce"),
              "Mordroc Castle Fitness",
              "Mordroc Castle Fitness Summary",
              "Mordroc Castle Fitness Website",
              "",
              true,
              true,
              true,
              true,
              DateTimeOffset.Now,
              Guid.Empty,
              "f5491fa139df469af2a9f488dee29ada79fe132628f079bba45dc86158f8ce0c",
              "2048-06-14 14:55:45Z"},
            { new Guid("b1530c62-9ab2-4043-9c79-f82a2a32897b"),
              "Blue Sun",
              "Blue Sun Summary",
              "Blue Sun Website",
              "",
              true,
              true,
              true,
              true,
              DateTimeOffset.Now,
              Guid.Empty,
              "58f4ef47b86da7f2b55ddddf3301c9b2a41b8386c438c603fcb9259a4169bf42",
              "2048-06-14 14:58:28Z"},
            { new Guid("af432ecf-6bf2-461d-ad5b-80701103699b"),
              "Skyrim Runners",
              "Skyrim Runners Summary",
              "Skyrim Runners Website",
              "",
              true,
              true,
              true,
              true,
              DateTimeOffset.Now,
              Guid.Empty,
              "050e4f64a801bcf26c3b46550d410ca53a6b1116e2bec083fe32c157247e6668",
              "2048-06-14 14:58:28Z"},
            { new Guid("1ea6450d-7506-4a9c-944c-91f328cb2083"),
              "Royston Vasey",
              "Royston Vasey Summary",
              "Royston Vasey Website",
              "",
              true,
              true,
              true,
              true,
              DateTimeOffset.Now,
              Guid.Empty,
              "7dc651107de8d6e22b037384876e197c217b3125b391f0ae7f0b4193207ed7bc",
              "2048-06-14 14:58:28Z"}
          });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql("DELETE FROM dbo.Providers WHERE Id IN (" +
        "'e778091e-e5b7-47b0-90d2-e28d6236a5f5'," +
        "'ebe236fa-d293-41ec-9b34-9f1bfae992ce'," +
        "'b1530c62-9ab2-4043-9c79-f82a2a32897b'," +
        "'af432ecf-6bf2-461d-ad5b-80701103699b'," +
        "'1ea6450d-7506-4a9c-944c-91f328cb2083')");
    }
  }
}
