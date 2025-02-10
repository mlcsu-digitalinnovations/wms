using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using WmsHub.Common.Exceptions;

namespace WmsHub.Common.Helpers
{
  public static class Certificates
  {
    public static X509Certificate2 GetCertificateByThumbprint(
      string thumbprint, string storeName = "MY")
    {
      if (string.IsNullOrWhiteSpace(thumbprint))
        throw new ArgumentNullOrWhiteSpaceException(nameof(thumbprint));

      // refactored this to get the certificate from the current users
      // or local machine store personal certificate folder. 
      // You must install the pfx file with
      // an exportable private key or it won't work.
      X509Certificate2 certificate = null;
      var storeLocations = new StoreLocation[] {
        StoreLocation.CurrentUser,
        StoreLocation.LocalMachine
      };

      foreach (var storeLocation in storeLocations)
      {
        X509Store store = new X509Store(storeName, storeLocation);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

        X509Certificate2Collection collection = store.Certificates.Find(
          X509FindType.FindByThumbprint,
          thumbprint,
          true);

        if (collection.Count == 1)
        {
          certificate = collection[0];
          break;
        }
        else if (collection.Count > 1)
        {
          throw new ArgumentException(nameof(thumbprint),
            $"The thumbprint {thumbprint} matched more than one certificate.");
        }
      }

      return certificate;
    }

    public static X509Certificate2 LoadCertificateFromFile(
      string filePath, string password)
    {
      if (string.IsNullOrWhiteSpace(filePath))
        throw new ArgumentNullOrWhiteSpaceException(nameof(filePath));

      if (string.IsNullOrWhiteSpace(password))
        throw new ArgumentNullOrWhiteSpaceException(nameof(password));

      if (!File.Exists(filePath))
        throw new FileNotFoundException(null, filePath);

      X509Certificate2 certificate = new X509Certificate2(filePath, password);

      return certificate;
    }
  }
}
