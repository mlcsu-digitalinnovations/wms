using System;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Business.Models.Interfaces;

public interface ICreateAccessKeyResponse : IResponseBase
{
  DateTimeOffset Expires { get; }
  string AccessKey { get; }
  string Email { get; }
}