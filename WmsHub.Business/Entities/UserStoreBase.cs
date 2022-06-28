using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Entities
{
  public class UserStoreBase:BaseEntity
  {
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
  }
}