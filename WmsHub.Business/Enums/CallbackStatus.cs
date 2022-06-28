using System.Runtime.Serialization;
using System.ComponentModel;

namespace WmsHub.Business.Enums
{
  public enum CallbackStatus
  {
    [Description("none")]
    None,
    [Description("delivered")]
    Delivered,
    [EnumMember(Value = "permanent-failure")]
    [Description("permanent-failure")]
    PermanentFailure,
    [EnumMember(Value = "temporary-failure")]
    [Description("temporary-failure")]
    TemporaryFailure,
    [EnumMember(Value = "technical-failure")]
    [Description("technical-failure")]
    TechnicalFailure
  }
}
