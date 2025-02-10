using CSVFile;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using WmsHub.Business.Models;

namespace WmsHub.Utilities.Converters
{
  internal class AdUserFileConverter
  {
    [Serializable]
    internal class CsvAdUser
    {
      public Guid Id { get; set; }
      public string UserPrincipalName { get; set; }
      public string DisplayName { get; set; }
      public string ObjectType { get; set; }
      public string UserType { get; set; }
      public bool IsUser { get; set; }
      public bool IsGroup { get; set; }
      public bool IsGuest { get; set; }

      
      internal string GetOwnerName()
      {
        string result;
        TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;

        result = DisplayName
          .Replace("1", "")
          .Replace(" (MLCSU)", "")
          .Replace(" (CCG) SOTCCG", "")
          .Replace(" (CCG) SESCCG", "")
          .Replace(" (CCG)", "")
          .Replace(" (NHS MIDLANDS AND LANCASHIRE COMMISSIONING SUPPORT UNIT)", 
            "");

        if (result.Contains(", "))
        {
          // Expecting name in format SURNAME, Forename
          string[] splits = result.Split(", ");

          string forename = textInfo.ToTitleCase(textInfo.ToLower(splits[1]));
          string surname = textInfo.ToTitleCase(textInfo.ToLower(splits[0]));

          result = $"{forename} {surname}";
        }
        else if (result.Contains("."))
        {
          string[] splits = result.Split(".");

          string forename = textInfo.ToTitleCase(textInfo.ToLower(splits[0]));
          string surname = textInfo.ToTitleCase(textInfo.ToLower(splits[1]));

          result = $"{forename} {surname}";
        }
        else
        {
          result = textInfo.ToTitleCase(textInfo.ToLower(result));
        }

        return result;
      }
    }

    internal static void ConvertCsv(string filePath)
    {
      if (!File.Exists(filePath))
      {
        throw new FileNotFoundException("Ad file not found.", filePath);
      }
      Log.Information($"Convertng {filePath}");

      string csvText = File.ReadAllText(filePath);
      List<CsvAdUser> csvAdUsers = CSV.Deserialize<CsvAdUser>(csvText).ToList();
      Log.Information($"Found {csvAdUsers.Count} users.");

      List<UserStore> userStoreUsers = new(csvAdUsers.Count);
      csvAdUsers.ForEach(csvAdUser => userStoreUsers.Add(new()
      {
        ApiKey = "NONE",
        Domain = "Rmc.Ui",
        Expires = null,
        ForceExpiry = false,
        Id = csvAdUser.Id,
        IsActive = true,
        OwnerName = csvAdUser.GetOwnerName(),
        Scope = null
      }));

      string json = JsonConvert.SerializeObject(userStoreUsers);
      string outputFilePath = Path.Combine(
        Path.GetDirectoryName(filePath),
        $"{Path.GetFileNameWithoutExtension(filePath)}.json");

      if (File.Exists(outputFilePath))
      {
        Log.Information($"Deleting {outputFilePath}.");
        File.Delete(outputFilePath);
      }

      File.WriteAllText(outputFilePath, json);
      Log.Information(
        $"Wrote {userStoreUsers.Count} users to {outputFilePath}");
    }
  }
}
