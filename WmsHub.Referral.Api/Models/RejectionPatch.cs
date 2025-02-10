using System;

namespace WmsHub.Referral.Api.Models;

public class RejectionPatch
{
  public Guid Id { get; set; }
  public string Information { get; set; }
}