using System.Collections.Generic;

namespace WmsHub.Common.Api.Models;

public class ApiVersionOptions
{
  private int _defaultMajor = 1;
  private int _defaultMinor;  

  public int DefaultMajor
  {
    get => _defaultMajor;
    set
    {
      if (value <= 0)
      {
        value = 1;
      }

      _defaultMajor = value;
    }
  }

  public int DefaultMinor
  {
    get => _defaultMinor;
    set
    {
      if (value < 0)
      {
        value = 0;
      }

      _defaultMinor = value;
    }
  }

  public List<string> DeprecatedVersions { get; set; }

  public bool HideDeprecated { get; set; }

  public static string SectionKey => "ApiVersion";

  public bool IsVersionDeprecated(string path)
  {
    foreach (string deprecatedVersion in DeprecatedVersions ?? new List<string>())
    { 
      if (path.Contains(deprecatedVersion))
      {
        return true;
      }
    }

    return false;
  }
}
