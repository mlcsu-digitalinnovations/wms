using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.BusinessIntelligence;
public class BiQuestionnaire
{
  public Guid Id { get; set; }
  public string Answers { get; set; }
  public bool ConsentToShare { get; set; }
  public string Ubrn { get; set; }
  public QuestionnaireType QuestionnaireType { get; set; }
}
