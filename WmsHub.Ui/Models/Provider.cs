using System;

namespace WmsHub.Ui.Models
{
  public class Provider
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Summary { get; set; }
    public string Summary2 { get; set; }
    public string Summary3 { get; set; }
    public string Website { get; set; }
    public string Logo { get; set; }
    public bool IsSelected { get; set; }
  }

  public class BusinessProvider : Business.Models.Provider
  {
    public int TriageLevel { get; set; }
  }
}