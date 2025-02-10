using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.AzureFunctions.Options;
internal class UdalExtractOptions
{
  public static string SectionKey => nameof(UdalExtractOptions);

  [ExcludeFromCodeCoverage(Justification = "Nothing to test - remove if class can be tested.")]
  [Required]
  public required AgemPipeOptions AgemPipe { get; set; }

  [ExcludeFromCodeCoverage(Justification = "Nothing to test - remove if class can be tested.")]
  [Required]
  public required BiApiOptions BiApi { get; set; }

  [ExcludeFromCodeCoverage(Justification = "Nothing to test - remove if class can be tested.")]
  [Required]
  public required MeshMailboxApiOptions MeshMailboxApi { get; set; }

  internal class AgemPipeOptions
  {

    [Required]
    public required string CliPath { get; set; }

    [Required]
    public string ColumnActions { get; set; } = "0 pseudo";

    [Required]
    public required string ConfigName { get; set; }

    [Required]
    public string EncryptedFilename { get; set; } = $"dwmp_udal_enc_{DateTime.Now:yyyyMMddHHmmss}";

    public string EncryptedFilePath => Path.Combine(ExtractFilePath, EncryptedFilename);

    [Required]
    public required string ExtractFilePath { get; set; }

    [Required]
    public string FileToEncryptFilename { get; set; } = $"dwmp_udal_{DateTime.Now:yyyyMMddHHmmss}";

    public string FileToEncryptFilePath => Path.Combine(ExtractFilePath, FileToEncryptFilename);

    [Required]
    public bool FileToEncryptKeepAfterEncryption { get; set; } = false;

    [Required]
    public string OutputIndicatingSuccess { get; set; } = "Processing\r\nComplete\r\n";

    [Required]
    public required string WorkingDirectory { get; set; }
  }

  internal class BiApiOptions
  {
    [Required]
    public required string ApiKey { get; set; }

    // Defaults to include data from the last 8 days so there is a day overlap from the last extract.
    public DateTime? From { get; set; } = DateTime.Now.Date.AddDays(-8);

    public Uri RequestUri
    {
      get
      {
        QueryString queryString = new();
        
        if (From.HasValue)
        {
          queryString = queryString.Add("fromDate", $"{From:s}");
        }

        if (To.HasValue)
        {
          queryString = queryString.Add("toDate", $"{To:s}");
        }

        Uri requestUri = new($"{Url}{queryString.Value}");
        return requestUri;
      }
    }

    // Defaults to include data from today.
    public DateTime? To { get; set; } = DateTime.Now.Date.AddDays(1);

    [Required]
    public required string Url { get; set; }
  }

  [ExcludeFromCodeCoverage(Justification = "Nothing to test - remove if class can be tested.")]
  internal class MeshMailboxApiOptions
  {
    [Required]
    public required string CertificateName { get; set; }

    [Required]
    public required string KeyVaultUrl { get; set; }
  }
}