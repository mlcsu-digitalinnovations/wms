using System;

namespace WmsHub.Business.Entities;

public class ConfigurationValue : IComparable<ConfigurationValue>
{
  public string Id { get; set; } = string.Empty;
  public string Value { get; set; } = string.Empty;

  public int CompareTo(ConfigurationValue other) => other == null 
    ? 1 
    : string.CompareOrdinal(Id, other.Id);
}
