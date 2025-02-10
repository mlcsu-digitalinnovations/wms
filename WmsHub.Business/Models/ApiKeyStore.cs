using System;
using WmsHub.Business.Entities.Interfaces;

namespace WmsHub.Business.Models
{
  public class ApiKeyStore : IApiKeyStore
  {
    public string Key { get; set; }
    public string KeyUser { get; set; }
    public int Domain { get; set; }
    public string Domains { get; set; }
    public string Sid { get; set; }
    public DateTimeOffset? Expires { get; set; }
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Guid ModifiedByUserId { get; set; }
  }
}
