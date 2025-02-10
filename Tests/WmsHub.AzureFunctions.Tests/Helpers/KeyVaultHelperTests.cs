using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FluentAssertions;
using Moq;
using System.Security.Cryptography.X509Certificates;
using WmsHub.AzureFunctions.Factories;
using WmsHub.AzureFunctions.Helpers;
using WmsHub.Tests.Helper;

namespace WmsHub.AzureFunctions.Tests.Helpers;

public class KeyVaultHelperTests : ABaseTests
{
  public class GetCertificateFromKeyVault : KeyVaultHelperTests
  {

    [Fact]
    public void When_CertificateNameIsNull_ThrowsArgumentException()
    {
      // Arrange.
      string certificateName = null!;
      string keyVaultUrl = "https://mockvault.vault.azure.net";

      // Act.
      Action act = () => KeyVaultHelper.GetCertificateFromKeyVault(
        certificateName,
        keyVaultUrl,
        new SecretClientFactory());

      // Assert.
      act.Should().Throw<ArgumentException>().WithMessage("Value cannot be null.*");
    }

    [Fact]
    public void When_KeyVaultUrlIsNull_ThrowsArgumentException()
    {
      // Arrange.
      string certificateName = "mockCertificate";
      string keyVaultUrl = null!;

      // Act.
      Action act = () => KeyVaultHelper.GetCertificateFromKeyVault(
        certificateName,
        keyVaultUrl,
        new SecretClientFactory());

      // Assert.
      act.Should().Throw<ArgumentException>().WithMessage("Value cannot be null*");
    }

    [Fact]
    public void When_InputsAreValid_ReturnsCertificate()
    {
      // Arrange.
      string certificateName = "mockCertificate";
      string keyVaultUrl = "https://mockvault.vault.azure.net";

      // Mock the SecretClient.
      Mock<SecretClient> mockSecretClient = new();
      mockSecretClient
        .Setup(client => client.GetSecret(certificateName, null, default))
        .Returns(Response.FromValue(
          new KeyVaultSecret(certificateName, MockCertificate.Base64Value),
          Mock.Of<Response>()));

      // Mock the SecretClientFactory.
      Mock<ISecretClientFactory> mockFactory = new();
      mockFactory
        .Setup(factory => factory.Create(It.IsAny<ChainedTokenCredential>(), keyVaultUrl))
        .Returns(mockSecretClient.Object);

      // Act.
      X509Certificate2 result = KeyVaultHelper.GetCertificateFromKeyVault(
        certificateName,
        keyVaultUrl,
        mockFactory.Object);

      // Assert.
      result.Should().BeEquivalentTo(new MockCertificate());
    }
  }
}