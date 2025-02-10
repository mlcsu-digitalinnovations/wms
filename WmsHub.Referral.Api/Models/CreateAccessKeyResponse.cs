using System;

namespace WmsHub.Referral.Api.Models;

public class CreateAccessKeyResponse
{
  public DateTimeOffset Expires { get; set; }
  public string KeyCode { get; set; }
}