﻿namespace WmsHub.Provider.Api.Models;

public class RefreshTokenRequest
{
  public string GrantType { get; set; }
  public string RefreshToken { get; set; }
}
