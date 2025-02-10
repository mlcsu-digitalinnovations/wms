using System;

namespace WmsHub.Referral.Api.Models;

public class ValidateAccessKeyResponse
{
  public DateTimeOffset Expires { get; set; }
  public bool ValidCode { get; set; }
}