using System;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Business.Models.Interfaces;

public interface IValidateAccessKeyResponse : IResponseBase
{
    DateTimeOffset Expires { get; }
    bool IsValidCode { get; }
}