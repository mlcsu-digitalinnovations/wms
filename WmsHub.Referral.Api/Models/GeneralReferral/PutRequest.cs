using System;

namespace WmsHub.Referral.Api.Models.GeneralReferral
{
  public class PutRequest : AReferralPostPutRequest
  {
    public Guid Id { get; set; }
  }
}