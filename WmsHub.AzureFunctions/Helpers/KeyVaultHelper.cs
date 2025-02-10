using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Security.Cryptography.X509Certificates;
using WmsHub.AzureFunctions.Factories;

namespace WmsHub.AzureFunctions.Helpers;

public class KeyVaultHelper
{
  public static X509Certificate2 GetCertificateFromKeyVault(
    string certificateName,
    string keyVaultUrl,
    ISecretClientFactory secretClientFactory)
  {
    ArgumentException.ThrowIfNullOrEmpty(certificateName, nameof(certificateName));
    ArgumentException.ThrowIfNullOrEmpty(keyVaultUrl, nameof(keyVaultUrl));

    ChainedTokenCredential chainedTokenCredential = new(
      new VisualStudioCredential(),
      new DefaultAzureCredential());

    SecretClient secretClient = secretClientFactory.Create(chainedTokenCredential, keyVaultUrl);

    Azure.Response<KeyVaultSecret> keyVaultSecret = secretClient.GetSecret(certificateName);

    byte[] certBytes = Convert.FromBase64String(keyVaultSecret.Value.Value);

    X509Certificate2 certificate = new(certBytes, string.Empty, X509KeyStorageFlags.MachineKeySet);

    return certificate;
  }
}