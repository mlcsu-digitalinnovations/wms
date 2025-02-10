using System;

namespace WmsHub.Business.Models.GpDocumentProxy;

public class GpDocumentProxyPostDischarge
{
  public DateTimeOffset DateOfBirth { get; set; }
  public DateTimeOffset? DateCompletedProgramme { get; set; }
  public DateTimeOffset DateOfReferral { get; set; }
  public string FamilyName { get; set; }
  public decimal? FirstRecordedWeight { get; set; }
  public string GivenName { get; set; }
  public decimal? LastRecordedWeight { get; set; }
  public DateTimeOffset? LastRecordedWeightDate { get; set; }
  public string Message { get; set; }
  public string NhsNumber { get; set; }
  public string ProviderName { get; set; }
  public Guid ReferralId { get; set; }
  public string ReferralSource { get; set; }
  public string ReferringOrganisationOdsCode { get; set; }
  public string Sex { get; set; }
  public Guid? TemplateId { get; set; }
}
