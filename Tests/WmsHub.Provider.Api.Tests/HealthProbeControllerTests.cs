using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace WmsHub.Provider.Api.Tests
{
  public class HealthProbeControllerTests
    : IClassFixture<WebApplicationFactory<Startup>>
  {
    private readonly WebApplicationFactory<Startup> _factory;

    public HealthProbeControllerTests(
      WebApplicationFactory<Startup> factory)
    {
      _factory = factory;
    }

    [Fact]
    public async Task EmptyRoute_Head_ReturnsSuccess()
    {
      // Arrange
      var client = _factory.CreateClient();

      // Act
      var response = await client.SendAsync(new HttpRequestMessage(
        HttpMethod.Head, "/"));

      // Assert
      response.EnsureSuccessStatusCode();
    }

  }
}
