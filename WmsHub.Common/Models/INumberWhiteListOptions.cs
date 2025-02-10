using System.Collections.Generic;

namespace WmsHub.Common.Models
{
  public interface INumberWhiteListOptions
  {
    bool? IsNumberWhiteListEnabled { get; set; }
    List<string> NumberWhiteList { get; set; }
    void ValidateNumbersAgainstWhiteList(
      IEnumerable<string> numbersToValidate);
  }
}