using CSVFile;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WmsHub.Utilities.Converters;

internal class B2cAdUserFileConverter
{
  [Serializable]
  internal class B2cAdUser
  {
    public string AccountEnabled { get; set; }
    public string AgeGroup { get; set; }
    public string AlternateEmailAddress { get; set; }
    public string AuthenticationAlternativePhoneNumber { get; set; }
    public string AuthenticationEmail { get; set; }
    public string AuthenticationPhoneNumber { get; set; }
    public string City { get; set; }
    public string ConsentProvidedForMinor { get; set; }
    public string Country { get; set; }
    public string Department { get; set; }
    public string DisplayName { get; set; }
    public string GivenName { get; set; }
    public string JobTitle { get; set; }
    public string LegalAgeGroupClassification { get; set; }
    public string Mail { get; set; }
    public string Mobile { get; set; }
    public string ObjectId { get; set; }
    public string PhysicalDeliveryOfficeName { get; set; }
    public string PostalCode { get; set; }
    public string State { get; set; }
    public string StreetAddress { get; set; }
    public string Surname { get; set; }
    public string TelephoneNumber { get; set; }
    public string UsageLocation { get; set; }
    public string UserPrincipalName { get; set; }
    public string UserType { get; set; }
  }

  internal static void ConvertCsv(string filePath)
  {
    if (!File.Exists(filePath))
    {
      throw new FileNotFoundException("Ad file not found.", filePath);
    }

    Log.Information($"Convertng {filePath}");

    string csvText = File.ReadAllText(filePath);
    List<B2cAdUser> csvAdUsers = CSV.Deserialize<B2cAdUser>(csvText).ToList();
    Log.Information($"Found {csvAdUsers.Count} users.");

    string outputFilePath = Path.Combine(
      Path.GetDirectoryName(filePath),
      $"{Path.GetFileNameWithoutExtension(filePath)}.sql");

    if (File.Exists(outputFilePath))
    {
      Log.Information($"Deleting {outputFilePath}.");
      File.Delete(outputFilePath);
    }

    StringBuilder sb = new();
    csvAdUsers.ForEach(x => sb
      .AppendLine($"WHEN '{x.ObjectId}' THEN '{x.DisplayName}'"));

    File.WriteAllText(outputFilePath, sb.ToString());

    Log.Information($"Wrote {csvAdUsers.Count} users to {outputFilePath}");
  }
}
