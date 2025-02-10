using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WmsHub.TextMessage.Api.Models
{
  public class GetTokenResponse
  {
    public string Token { get; set; }
    public DateTimeOffset Expiry { get; set; }
  }
}
