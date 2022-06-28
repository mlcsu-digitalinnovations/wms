using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Entities
{
  /// <summary>
  /// Store for all ApiKeys - The columns should be encrypted
  /// </summary>
  public class ApiKeyStoreBase : BaseEntity
  {
    public string Key { get; set; }
    public string KeyUser { get; set; }
    public int Domain { get; set; }
    public string Domains { get; set; }
    public string Sid { get; set; }
    public DateTimeOffset? Expires { get; set; }
  }
}
