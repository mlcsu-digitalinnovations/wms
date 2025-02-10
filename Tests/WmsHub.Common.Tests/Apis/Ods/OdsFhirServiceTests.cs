using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using WmsHub.Common.Apis.Ods.Fhir;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Common.Tests.Apis.Ods;

public class OdsFhirServiceTests : ATheoryData
{
  private readonly IOdsFhirService _odsFhirService;

  public OdsFhirServiceTests()
  {
    ServiceCollection services = new();
    ConfigurationBuilder configurationBuilder = new();
    
    IConfiguration configuration = configurationBuilder.Build();

    services.AddOdsFhirService(configuration);

    ServiceProvider serviceProvider = services.BuildServiceProvider();

    _odsFhirService = serviceProvider.GetRequiredService<IOdsFhirService>();
  }

  public class OrganisationCodeExistsIntegrationTests : OdsFhirServiceTests
  {
    [Fact]
    public async Task CodeDoesNotExist_False()
    {
      // Arrange
      string odsCode = "XXX";

      // Act
      bool exists = await _odsFhirService.OrganisationCodeExistsAsync(odsCode);

      // Assert
      exists.Should().BeFalse();
    }

    [Fact]
    public async Task CodeExists_True()
    {
      // Arrange
      string odsCode = "RLY";

      // Act
      bool exists = await _odsFhirService.OrganisationCodeExistsAsync(odsCode);

      // Assert
      exists.Should().BeTrue();
    }
  }
}
