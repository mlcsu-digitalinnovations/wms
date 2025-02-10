namespace WmsHub.Tests.Helper;

/// <summary>
/// 
/// </summary>
public class MockCertificate
{
  /// <summary>
  /// The certificate's issuer, containing the certificate's attributes.
  /// </summary>  
  public string Issuer =>
    "E=mlcsu.digitalinnovations@nhs.net, " +
    "CN=Mock Certificate, " +
    "OU=DIU, " +
    "O=MLCSU, " +
    "L=Stoke-On-Trent, " +
    "S=Staffordshire, " +
    "C=UK";

  /// <summary>
  /// The certificate's serial number.
  /// </summary>
  public string SerialNumber => "552462EF441086B91A82CF0367129C9B382E0835";

  /// <summary>
  /// The certificate's subject, containing the certificate's attributes.
  /// </summary>
  public string Subject => Issuer;

  /// <summary>
  /// The certificate's thumbprint.
  /// </summary>
  public string Thumbprint => "8592BBAE78656D66F6671BFB442DB0D08584F570";

  /// <summary>
  /// The certificate encoded into base 64.
  /// </summary>
  public const string Base64Value = "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" +
    "cgQIRgyMzeP1vCcCAggA";
}
