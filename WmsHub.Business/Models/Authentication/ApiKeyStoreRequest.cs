using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.Authentication
{
  public class ApiKeyStoreRequest
  {
    /// <summary>
    /// ApiKey
    /// </summary>
    /// <example>ZQGROhZQ0xhLDFIHGFdV6rqHkBYp2NR82fuKbqB9</example>
    public string Key { get; set; }
    /// <summary>
    /// The Claim Name i.e. provider, provider_admin
    /// </summary>
    /// <example>provider_admin</example>
    public string KeyUser { get; set; }
    /// <summary>
    /// Comma separated list of domains, or just single domain
    ///  ReferralApi,ProviderApi,TextMessageApi,ChatBotApi
    /// </summary>
    /// <example>ProviderApi</example>
    public string Domains { get; set; }
    /// <summary>
    /// THis is the GUID used by the system for Modified or Created By
    /// </summary>
    /// <example>C9AD5388-95D0-443F-992A-7D7B98FEF3CE</example>
    public string Sid { get; set; }
    /// <summary>
    /// If left null then the ApiKey does not expire
    /// </summary>
    public DateTimeOffset? Expires { get; set; }

    public int Domain
    {
      get
      {
        int result = 0;
        string[] domains = Domains.Split(',');
        foreach (var domain in domains)
        {
          if (Enum.TryParse(domain, out DomainAccess access))
          {
            result += (int) access;
          }
        }

        return result;
      }
    }

  }
}
