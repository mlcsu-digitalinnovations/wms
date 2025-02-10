using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Common.Exceptions;

namespace WmsHub.Common.Models
{
  public class NumberWhiteListOptions : INumberWhiteListOptions
  {
    [Required]
    public bool? IsNumberWhiteListEnabled { get; set; }

    public List<string> NumberWhiteList { get; set; }

    public virtual void ValidateNumbersAgainstWhiteList(
      IEnumerable<string> numbersToValidate)
    {
      // Default IsNumberWhiteListEnabled to true if it is missing
      // fail on the side of caution.
      bool isEnabled = IsNumberWhiteListEnabled ?? true;

      if (isEnabled)
      {
        if (NumberWhiteList != null && NumberWhiteList.Any())
        {
          foreach (string number in numbersToValidate)
          {
            if (!NumberWhiteList.Contains(number))
            {
              throw new NumberWhiteListException("NumberWhiteList is " +
                "enabled and there attempts to call numbers that are not " +
                "in it.");
            }
          }
        }
        else
        {
          throw new NumberWhiteListException("NumberWhiteList is " +
            "enabled but there are no numbers in the list.");
        }
      }
      else if (NumberWhiteList != null && NumberWhiteList.Any())
      {
        throw new NumberWhiteListException(
          "NumberWhiteList is disabled but there are numbers in the list.");
      }
    }
  }
}

