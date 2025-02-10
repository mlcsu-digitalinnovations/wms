using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FluentAssertions;
using Moq;
using WmsHub.AzureFunctions.Factories;

namespace WmsHub.AzureFunctions.Tests.Factories;
public class SecretClientFactoryTests
{

  public class CreateTests : SecretClientFactoryTests
  {
    [Fact]
    public void Should_ReturnSecretClient()
    {
      // Arrange.
      string expectedKeyVaultUrl = "https://test.key.vault";
      SecretClientFactory secretClientFactory = new();

      // Act.
      SecretClient secretClient = secretClientFactory.Create(
        Mock.Of<ChainedTokenCredential>(),
        expectedKeyVaultUrl);

      // Assert.
      secretClient.Should().NotBeNull();
      secretClient.VaultUri.Should().Be(expectedKeyVaultUrl);
    }
  }
}
