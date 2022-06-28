using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Xunit;
using WmsHub.Business;

namespace WmsHub.BusinessIntelligence.Api.Tests
{
  [CollectionDefinition("Service collection")]
  public class ServiceCollection : ICollectionFixture<ServiceFixture>
  {
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
  }

  public class ServiceFixture
  {
    public IMapper Mapper { get; set; }
    public DbContextOptions<DatabaseContext> Options { get; private set; }

    public ServiceFixture()
    {
      var mapperConfiguration = new MapperConfiguration(cfg =>
        cfg.AddMaps(new[] {
          "WmsHub.Business",
          "WmsHub.BusinessIntelligence.Api"
        })
      );

      Mapper = mapperConfiguration.CreateMapper();


      Options = new DbContextOptionsBuilder<DatabaseContext>()
        .UseInMemoryDatabase(databaseName: "WmsHub_BI")
        .Options;

      CreateDatabase();
    }

    private void CreateDatabase()
    {
      using var dbContext = new DatabaseContext(Options);

      //dbContext.SaveChanges();
    }
  }
}