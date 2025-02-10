using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace WmsHub.AzureFunctions.Factories;

/// <summary>
/// Facilitates the mocking of <see cref="SecretClient"/>  for unit tests.
/// </summary>
public class SecretClientFactory : ISecretClientFactory
{
  /// <summary>
  /// Creates a SecretClient.
  /// </summary>
  /// <param name="chainedTokenCredential">Credentials to access the key vault.</param>
  /// <param name="keyVaultUrl">The key vault to bind the secret client to.</param>
  /// <returns>A new instance of <see cref="SecretClient"/> for the specified key vault.</returns>
  public SecretClient Create(ChainedTokenCredential chainedTokenCredential, string keyVaultUrl) 
    => new(new Uri(keyVaultUrl), chainedTokenCredential);
}
