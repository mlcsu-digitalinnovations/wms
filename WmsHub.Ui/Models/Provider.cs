using System;
using System.Collections.Generic;
using System.Linq;

namespace WmsHub.Ui.Models;

public class Provider
{
  public Guid Id { get; set; }
  public bool IsSelected { get; set; }
  public string Logo { get; set; }
  public string Name { get; set; }
  public List<ProviderDetail> Details { get; set; }
  public string Summary { get; set; }
  public string Summary2 { get; set; }
  public string Summary3 { get; set; }
  public string Website { get; set; }

  public enum BooleanDetailSections
  {
    AudioDescription,
    ChangeTextSize,
    MobileApp,
    OnScreenKeyboard,
    ScreenReader,
    Website,
  }

  public enum StringDetailSections
  {
    AccessLength,
    MainCoaching,
    OtherCoaching,
  }

  public bool GetDetailSectionValue(BooleanDetailSections detailSection)
  {
    string detailSectionValue = Details
      .FirstOrDefault(x => x.Section == detailSection.ToString())
      ?.Value
      ?? "false";

    return detailSectionValue.Equals("true", StringComparison.OrdinalIgnoreCase);
  }

  public string GetDetailSectionValue(StringDetailSections detailSection)
  {
    return Details.FirstOrDefault(x => x.Section == detailSection.ToString())?.Value ?? "Unknown";
  }
}

public class BusinessProvider : Business.Models.Provider
{
  public int TriageLevel { get; set; }
}