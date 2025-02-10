using System.Text.Json.Serialization;

namespace WmsHub.Business.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuestionnaireType
{
  None,
  CompleteProT1,
  CompleteProT2and3,
  CompleteSelfT1,
  CompleteSelfT2and3,
  NotCompleteProT1and2and3,
  NotCompleteSelfT1and2and3
}
