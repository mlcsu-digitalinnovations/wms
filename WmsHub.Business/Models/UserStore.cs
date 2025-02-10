using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Models
{
  public class UserStore
  {
    public Guid Id { get; set; }

    /// <summary>
    /// Generated Api key
    /// </summary>
    [Required]
    public string ApiKey { get; set; }
    /// <summary>
    /// Owner Name is the name of the intended user
    /// </summary>
    [Required]
    public string OwnerName { get; set; }
    /// <summary>
    /// Domain which the ApiKey covers such as
    /// TextMessageApi, ProviderApi etc.
    /// would be as typeof(TexMessageApi
    /// i.e typeof(MyTestClass).Assembly.GetName().Name
    /// </summary>
    [Required]
    public string Domain { get; set; }
    public string Scope { get; set; }
    public DateTimeOffset? Expires { get; set; }
    public bool ForceExpiry { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Guid ModifiedByUserId { get; set; }
  }
}
