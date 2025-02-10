using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models.GpDocumentProxy;

public class GpDocumentProxyReferralDischarge
{
  private string _referralSource;
  private string _referralSourceDescription;
  private string _sex;

  public DateTimeOffset DateOfBirth { get; set; }
  public DateTimeOffset? DateCompletedProgramme { get; set; }
  public DateTimeOffset DateOfReferral { get; set; }
  public string FamilyName { get; set; }
  public string GivenName { get; set; }
  public Guid Id { get; set; }
  public decimal? LastRecordedWeight { get; set; }
  public DateTimeOffset? LastRecordedWeightDate { get; set; }
  public string Message { get; set; }
  public string NhsNumber { get; set; }
  public string ProviderName { get; set; }
  public string ProgrammeOutcome { get; set; }
  public string ReferralSource 
  {
    get => _referralSource;
    set
    {
      _referralSource = value;

      if (!string.IsNullOrWhiteSpace(value) 
        && Enum.TryParse(value, out ReferralSource referralSourceEnum))
      {
        _referralSourceDescription = referralSourceEnum
          .GetDescriptionAttributeValue();
      }
    }
  }
  public string ReferralSourceDescription => _referralSourceDescription;
  public string ReferringOrganisationOdsCode { get; set; }
  public string Sex
  {
    get => _sex;
    set
    {
      if (!string.IsNullOrWhiteSpace(value) && value.TryParseSex(out Sex enumValue))
      {
        _sex = enumValue switch
        {
          Enums.Sex.NotKnown => "Unspecified",
          Enums.Sex.NotSpecified => "Unspecified",
          _ => value
        };
      }
    }
  }
  public Guid? TemplateId { get; set; }
  public string Ubrn { get; set; }
  public decimal? WeightOnReferral { get; set; }
}
