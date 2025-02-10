using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Entities;

namespace WmsHub.Utilities.ConfigurationValues;

public class ConfigurationValuesService
{  
  private readonly string _masterFilePath;
  private readonly string _masterFileName;
  private readonly bool _reportDifferences = false;
  private readonly string _reportFileName;
  private readonly string _scriptFilePath;
  private string _scriptFileName;

  private string MasterFilePath => 
    Path.Combine(_masterFilePath, _masterFileName);

  private readonly DatabaseContext _context;
  private const string VALUE_SAME_IN_BOTH_TEMPLATE =
    "Id: {0}\r\nValue: {1}\r\n Value same in both";
  private const string VALUE_NOT_SAME_IN_BOTH_TEMPLATE =
    "Id: {0}\r\nValue in Json: {1}\r\nValue in Local Database: {2}\r\n" +
    "[Copy to Master: M], [Copy to Local Database: L], [Ignore: I]";
  private const string EXISTS_IN_MASTER_TEMPLATE =
    "Id: {0}\r\nValue in Json: {1}\r\n" +
    "[Add to Local Database: A], [Delete from Master: D], [Ignore: I]";
  private const string EXISTS_IN_LOCAL_TEMPLATE =
    "Id: {0}\r\nValue in Local Database: {1}\r\n" +
    "[Add to Master: A], [Delete from Local Database: D], [Ignore: I]";
  private const string MASTER_JSON_FILE_NOT_EXISTS =
    "Master json file does not exists\r\n" +
    "[Create master json file: C], [Ignore: I]";
  private const string LOCAL = "Local";
  private const string MASTER = "Master";

  public ConfigurationValuesService(
    IConfiguration configuration,
    DatabaseContext context)
  {
    _context = context;

    _masterFilePath = configuration.GetValue<string>("MasterJsonFilePath")
      ?? string.Empty;
    _masterFileName = configuration.GetValue<string>("MasterJsonFileName")
      ?? string.Empty;

    _reportFileName = configuration.GetValue<string>("ReportFileName")
      ?? "ConfigurationValuesComparison.csv";
    _scriptFilePath = configuration.GetValue<string>("ScriptFilePath") 
      ?? @"C:\Temp\DWMP\";
    _scriptFileName = configuration.GetValue<string>("ScriptFileName") 
      ?? "ConfigurationValuesScript.sql";

    if (string.Equals(
      configuration["reportdifferences"] ?? "TRUE", 
      "TRUE", 
      StringComparison.OrdinalIgnoreCase))
    {
      _reportDifferences = true;
    }

    if (string.IsNullOrWhiteSpace(_masterFileName))
    {
      throw new Exception("MasterJsonFileName required in configuration");
    }
    if (string.IsNullOrWhiteSpace(_masterFilePath))
    {
      throw new Exception("MasterJsonFilePath required in configuration");
    }
  }

  public async Task ExecuteAsync()
  {
    if (!File.Exists(MasterFilePath))
    {
      if (await CreateOrIgnoreMasterJsonFileCreation())
      {
        Console.WriteLine(
          $"Master Json file created: {MasterFilePath}");
        return;
      }
    }

    List<ConfigurationValue> masterJson =
      await GetConfigurationValuesFromJson();

    Console.WriteLine(
      $"Found {masterJson.Count} entries in the master Json file.");

    List<ConfigurationValue> localDatabase =
      await GetConfigurationValuesFromDatabase();

    Console.WriteLine(
      $"Found {localDatabase.Count} entries in the database.");

    List<ConfigurationValueModel> items =
      GetIntersection(masterJson, localDatabase);

    items.AddRange(GetExcept(masterJson, localDatabase, MASTER));
    items.AddRange(GetExcept(localDatabase, masterJson, LOCAL));

    if (_reportDifferences)
    {
      ReportDifferences(items);
    }
    else
    {
      await UpdateItems(items);
    }
  }

  internal async Task SortMasterJsonFile()
  {
    List<ConfigurationValue> masterJson =
      await GetConfigurationValuesFromJson();

    masterJson.Sort();

    await SaveConfigurationValuesToJson(masterJson);
  }

  internal async Task Script()
  {
    List<ConfigurationValue> masterJson = await GetConfigurationValuesFromJson();

    List<string> lines = GenerateScriptLines(masterJson);

    File.WriteAllLines(Path.Combine(_scriptFilePath, _scriptFileName), lines);
  }

  private List<string> GenerateScriptLines(List<ConfigurationValue> masterJson)
  {
    const string commentTemplate = "-- {0}";
    const string declareTemplate = 
      "DECLARE @value{0} nvarchar(4000) = '{1}';EXEC usp_Upsert_ConfigurationValue '{2}', @value{3}";

    List<string> generatedScriptLines = new();
    int index = 1;
    foreach (ConfigurationValue item in masterJson)
    {
      generatedScriptLines.Add(string.Format(
        CultureInfo.InvariantCulture, 
        commentTemplate, 
        item.Id));

      generatedScriptLines.Add(string.Format(
        CultureInfo.InvariantCulture, 
        declareTemplate, 
        index, 
        item.Value, 
        item.Id,
        index));

      index++;
    }

    return generatedScriptLines;
  }

  private void ReportDifferences(List<ConfigurationValueModel> items)
  {
    StringBuilder sb = new();
    sb.AppendLine("Id,Comparison,Master Json File,Database");
    int[] counts = new int[4];

    foreach (ConfigurationValueModel model in items)
    {
      sb.Append($"\"{model.Id}\",\"");

      if (model.IsValueSame)
      {
        if (model.DoesIdEndWithKey)
        {
          sb.Append("Matching Key");
        }
        else
        {
          sb.Append("Match");
        }
        
        counts[0]++;
      }
      else if (model.ExistsInMaster && model.ExistsInDatabase)
      {
        if (model.DoesIdEndWithKey)
        {
          sb.Append("Mismatching Key");
        }
        else
        {
          sb.Append("Mismatch");
        }
        counts[1]++;
      }
      else if (model.ExistsInMaster && !model.ExistsInDatabase)
      {
        sb.Append("Missing in Database");
        counts[2]++;
      }
      else if (!model.ExistsInMaster && model.ExistsInDatabase)
      {
        sb.Append("Missing in Master");
        counts[3]++;
      }

      sb.Append($"\",\"{model.ReportMasterValue}\",");
      sb.AppendLine($"\"{model.ReportDatabaseValue}\"");
    }

    sb.AppendLine();
    sb.AppendLine($"There are {counts[0]} matches.");
    sb.AppendLine($"There are {counts[1]} mismatches.");
    sb.AppendLine($"There are {counts[2]} missing in the database.");
    sb.AppendLine($"There are {counts[3]} missing in the master file.");

    string filePath = Path.Combine(_masterFilePath, _reportFileName);
    File.WriteAllText(filePath, sb.ToString());

    Console.WriteLine($"Updated {filePath} with results.");
   }

    private async Task UpdateItems(List<ConfigurationValueModel> items)
  {
    foreach (ConfigurationValueModel model in items)
    {
      if (model.IsValueSame)
      {
        Console.WriteLine(
          VALUE_SAME_IN_BOTH_TEMPLATE,
          model.Id,
          model.MasterValue);
      }
      else if (model.ExistsInMaster && model.ExistsInDatabase)
      {
        Console.WriteLine(
          VALUE_NOT_SAME_IN_BOTH_TEMPLATE,
          model.Id,
          model.MasterValue,
          model.LocalValue);

        char key = ReadKey(new()
        {
          'M', 'm', 'L', 'l', 'I', 'i'
        });

        await ProcessKey(model, key);
      }
      else if (model.ExistsInMaster && !model.ExistsInDatabase)
      {
        Console.WriteLine(
          EXISTS_IN_MASTER_TEMPLATE,
          model.Id,
          model.MasterValue);

        char key = ReadKey(new()
        {
          'A', 'a', 'D', 'd', 'I', 'i'
        });

        await ProcessKey(
          model,
          key,
          key == 'A' || key == 'a' ? LOCAL : MASTER);
      }

      else if (!model.ExistsInMaster && model.ExistsInDatabase)
      {
        Console.WriteLine(
          EXISTS_IN_LOCAL_TEMPLATE,
          model.Id,
          model.LocalValue);

        char key = ReadKey(new()
        {
          'A', 'a', 'D', 'd', 'I', 'i'
        });

        await ProcessKey(
          model,
          key,
          key == 'A' || key == 'a' ? MASTER : LOCAL);
      }

      Console.WriteLine();
      Console.WriteLine();
    }
  }

  private async Task<bool> CreateOrIgnoreMasterJsonFileCreation()
  {
    Console.WriteLine(MASTER_JSON_FILE_NOT_EXISTS);

    char key = ReadKey(new()
      {
        'C', 'c', 'I', 'i'
      });

    if (key == 'C' || key == 'c')
    {
      List<ConfigurationValue> items =
      await GetConfigurationValuesFromDatabase();
      await SaveConfigurationValuesToJson(items);

      return true;
    }

    return false;
  }

  private async Task<List<ConfigurationValue>>
    GetConfigurationValuesFromJson()
  {
    string jsonData = await File.ReadAllTextAsync(MasterFilePath);

    List<ConfigurationValue> configurationValues = JsonSerializer
      .Deserialize<List<ConfigurationValue>>(jsonData) 
        ?? new List<ConfigurationValue>();

    return configurationValues;
  }

  private async Task<List<ConfigurationValue>>
    GetConfigurationValuesFromDatabase()
  {
    return await _context.ConfigurationValues
      .AsNoTracking()
      .ToListAsync();
  }

  private List<ConfigurationValueModel> GetIntersection(
    List<ConfigurationValue> master,
    List<ConfigurationValue> local)
  {
    return master.Join(
      local,
      m => m.Id,
      l => l.Id,
      (m, l) => new ConfigurationValueModel
      {
        Id = m.Id,
        IsValueSame = m.Value.Equals(l.Value),
        MasterValue = m.Value,
        LocalValue = l.Value,
        ExistsInMaster = true,
        ExistsInDatabase = true
      })
      .ToList();
  }

  private List<ConfigurationValueModel> GetExcept(
    List<ConfigurationValue> list1,
    List<ConfigurationValue> list2,
    string key)
  {
    return list1.AsQueryable()
      .Except(list2, new ConfigurationValueComparer())
      .Select(x => new ConfigurationValueModel
      {
        Id = x.Id,
        IsValueSame = false,
        LocalValue = key == MASTER ? null : x.Value,
        MasterValue = key == MASTER ? x.Value : null,
        ExistsInMaster = key == MASTER,
        ExistsInDatabase = key != MASTER
      })
      .ToList();
  }

  private char ReadKey(List<char> validList)
  {
    char key;
    while (true)
    {
      key = Console.ReadKey().KeyChar;

      if (validList.Contains(key))
      {
        break;
      }
    }
    return key;
  }

  private async Task ProcessKey(
    ConfigurationValueModel model,
    char key,
    string destination = "Unknown")
  {
    switch (key)
    {
      case 'M':
      case 'm':
        await UpdateMasterJson(model.Id, model.LocalValue);
        break;
      case 'L':
      case 'l':
        await SaveChangesToLocalDatabase(model.Id, model.MasterValue);
        break;
      case 'A':
      case 'a':
        if (destination == LOCAL)
        {
          await SaveChangesToLocalDatabase(
            model.Id,
            model.MasterValue,
            EntityState.Added);
        }
        else
        {
          await AddToMasterJson(model.Id, model.LocalValue);
        }
        break;
      case 'D':
      case 'd':
        if (destination == LOCAL)
        {
          await SaveChangesToLocalDatabase(
            model.Id,
            model.LocalValue,
            EntityState.Deleted);
        }
        else
        {
          await DeleteFromMasterJson(model.Id);
        }
        break;
      default:
        break;
    }
  }

  private async Task UpdateMasterJson(string id, string value)
  {
    List<ConfigurationValue> items = await GetConfigurationValuesFromJson();
    items.Single(x => x.Id == id).Value = value;
    await SaveConfigurationValuesToJson(items);
  }

  private async Task DeleteFromMasterJson(string id)
  {
    List<ConfigurationValue> items = await GetConfigurationValuesFromJson();
    items.Remove(items.Single(x => x.Id == id));
    await SaveConfigurationValuesToJson(items);
  }

  private async Task AddToMasterJson(string id, string value)
  {
    List<ConfigurationValue> items = await GetConfigurationValuesFromJson();
    items.Add(new ConfigurationValue { Id = id, Value = value });
    await SaveConfigurationValuesToJson(items);
  }

  private async Task SaveChangesToLocalDatabase(
    string id,
    string value,
    EntityState state = EntityState.Modified)
  {
    EntityEntry<ConfigurationValue> entry =
      _context.Entry(new ConfigurationValue { Id = id, Value = value });
    entry.State = state;

    await _context.SaveChangesAsync();
  }

  private async Task SaveConfigurationValuesToJson(
    List<ConfigurationValue> items)
  {
    items.Sort();

    string jsonData = JsonSerializer.Serialize(
      items, 
      options: new JsonSerializerOptions() { WriteIndented = true } );

    await File.WriteAllTextAsync(MasterFilePath, jsonData);
  }

  private class ConfigurationValueComparer 
    : IEqualityComparer<ConfigurationValue>
  {
    public bool Equals(ConfigurationValue x, ConfigurationValue y)
    {
      return x?.Id == y?.Id;
    }

    public int GetHashCode([DisallowNull] ConfigurationValue obj)
    {
      return obj.Id.GetHashCode();
    }
  }
}
