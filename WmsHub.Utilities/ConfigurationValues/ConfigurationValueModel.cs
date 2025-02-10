using System;

namespace WmsHub.Utilities.ConfigurationValues;

public class ConfigurationValueModel
{
  public bool ExistsInMaster { get; set; }
  public bool ExistsInDatabase { get; set; }
  public string Id { get; set; } = string.Empty;
  public bool DoesIdEndWithKey 
    => (Id ?? string.Empty).EndsWith("Key", StringComparison.OrdinalIgnoreCase);
  public bool IsValueSame { get; set; }
  public string LocalValue { get; set; } = string.Empty;
  public string MasterValue { get; set; } = string.Empty;
  public string ReportDatabaseValue
    => (LocalValue ?? string.Empty).Replace("\"", "\"\"");
  public string ReportMasterValue
    => (MasterValue ?? string.Empty).Replace("\"", "\"\"");
}
