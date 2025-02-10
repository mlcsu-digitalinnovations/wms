using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Entities.Interfaces
{
  public interface IApiKeyStore
  {
    string Key { get; set; }
    string KeyUser { get; set; }
    int Domain { get; set; }
    string Domains { get; set; }
    string Sid { get; set; }
    DateTimeOffset? Expires { get; set; }
    Guid Id { get; set; }
    bool IsActive { get; set; }
    DateTimeOffset ModifiedAt { get; set; }
    Guid ModifiedByUserId { get; set; }
  }
}
