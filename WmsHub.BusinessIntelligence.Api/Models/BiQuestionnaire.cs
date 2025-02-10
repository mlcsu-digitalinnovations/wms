using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Text.Json.Serialization;
using WmsHub.Business.Enums;

namespace WmsHub.BusinessIntelligence.Api.Models;

[SwaggerSchema(Required = new[] { "BI Questionnaire Object." })]
public class BiQuestionnaire
{
  [SwaggerSchema("The questionnaire identifier.", ReadOnly = true)]
  public Guid Id { get; set; }
  [SwaggerSchema("The service user's answers.")]
  public string Answers { get; set; }
  [SwaggerSchema("The service user's consent to share their data.")]
  public bool ConsentToShare { get; set; }
  [SwaggerSchema("The service user's referral UBRN.", Format = "uuid")]
  public string Ubrn { get; set; }
  [SwaggerSchema("The type of questionnaire.")]
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public QuestionnaireType QuestionnaireType { get; set; }
}
