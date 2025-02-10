using System;

namespace WmsHub.Business.Models
{
  public class Ethnicity : BaseModel, IEthnicity
  {
    public string DisplayName { get; set; }
    public string GroupName { get; set; }
    public string Census2001 { get; set; }
    public string NhsDataDictionary2001Code { get; set; }
    public string NhsDataDictionary2001Description { get; set; }
    public string TriageName { get; set; }
    public decimal? MinimumBmi { get; set; }
    public int GroupOrder { get; set; }
    public int DisplayOrder { get; set; }

    public bool IsMatch(string ethnicity)
    {
      bool isMatch;
      StringComparison ignoreCase = StringComparison.InvariantCultureIgnoreCase;

      if (string.IsNullOrWhiteSpace(ethnicity))
      {
        isMatch = false;
      }
      else if (Census2001?.Equals(ethnicity, ignoreCase) == true)
      {
        isMatch = true;
      }
      else if (DisplayName?.Equals(ethnicity, ignoreCase) == true)
      {
        isMatch = true;
      }
      else if (GroupName?.Equals(ethnicity, ignoreCase) == true)
      {
        isMatch = true;
      }
      else if (NhsDataDictionary2001Code
        ?.Equals(ethnicity, ignoreCase) == true)
      {
        isMatch = true;
      }
      else if (NhsDataDictionary2001Description
        ?.Equals(ethnicity, ignoreCase) == true)
      {
        isMatch = true;
      }
      else if (TriageName?.Equals(ethnicity, ignoreCase) == true)
      {
        isMatch = true;
      }
      else
      {
        isMatch = false;
      }

      return isMatch;
    }
  }
}