using System;
using WmsHub.Business.Enums;

namespace WmsHub.Provider.Api.Models;

public class GetReferralStatusReasonsResponse
{
  public GetReferralStatusReasonsResponse(
    string description,
    ReferralStatusReasonGroup referralStatusReasonGroups,
    Guid id)
  {
    Description = description
      ?? throw new ArgumentNullException(nameof(description));
    Groups = (referralStatusReasonGroups
      & ReferralStatusReasonGroupConstants.ProviderStatuses).ToString();
    Id = id;
  }

  public string Description { get; }
  public string Groups { get; }
  public Guid Id { get; }
}
