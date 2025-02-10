using System;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models
{
  public interface IRefreshToken: IBaseModel
  {
    string Token { get; set; }
    DateTimeOffset Expires { get; set; }
    DateTimeOffset Created { get; set; }
    string CreatedBy { get; set; }
    DateTimeOffset? Revoked { get; set; }
    string RevokedBy { get; set; }
    string ReplacedByToken { get; set; }
    bool IsExpired { get; }
    bool IsActiveToken { get; }
  }
}