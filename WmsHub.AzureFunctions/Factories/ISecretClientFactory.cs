using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace WmsHub.AzureFunctions.Factories;
public interface ISecretClientFactory
{
  SecretClient Create(ChainedTokenCredential chainedTokenCredential, string keyVaultUrl);
}
