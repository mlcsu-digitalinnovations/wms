using System;

namespace WmsHub.Business.Entities
{
  public class RefreshTokenBase:BaseEntity
  {
    public string Token { get; set; }
    public DateTimeOffset Expires { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset? Revoked { get; set; }
    public string RevokedBy { get; set; }
    public string ReplacedByToken { get; set; }

  }
}