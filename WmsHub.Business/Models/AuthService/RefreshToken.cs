using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Models.AuthService
{
  public class RefreshToken: BaseModel, IRefreshToken
  {
    public string Token { get; set; }
    public DateTimeOffset Expires { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset? Revoked { get; set; }
    public string RevokedBy { get; set; }
    public string ReplacedByToken { get; set; }
    public bool IsExpired => DateTimeOffset.Now >= Expires;
    public bool IsActiveToken => Revoked == null && !IsExpired;
  }
}
