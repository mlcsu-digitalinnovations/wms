using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.AuthService;

namespace WmsHub.Business.Models.Authentication
{
  public class ApiKeyStoreResponse : BaseValidationResponse
  {
    public string Key { get; set; }
    public virtual string KeyUser { get; set; }
    public int Domain { get; set; }
    public string Domains { get; set; }
    public virtual string Sid { get; set; }
    public DateTimeOffset? Expires { get; set; }
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Guid ModifiedByUserId { get; set; }
    public virtual bool HasExpired =>
      Expires != null && !(Expires < DateTimeOffset.Now);
    public virtual Guid UserId
    {
      get
      {
        Guid.TryParse(Sid, out Guid userId);
        return userId;
      }
    }
    public virtual DomainAccess Access => (DomainAccess)Domain;
    public bool IsValidDomain { get; set; }
  }
}
