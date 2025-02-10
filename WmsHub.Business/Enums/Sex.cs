using System.ComponentModel;

namespace WmsHub.Business.Enums;

public enum Sex : int
{
  // NotKnown should not be changed from value 0 as Enum's default is used.
  [Description("Not Known")]
  NotKnown = 0,
  [Description("Male")]
  Male,
  [Description("Female")]
  Female,
  [Description("Not Specified")]
  NotSpecified
}
