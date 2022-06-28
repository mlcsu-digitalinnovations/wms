using System.Collections.Generic;
using WmsHub.Common.Api.Models;

namespace WmsHub.ReferralsService.Models
{
  public class RegistrationList
  {
    public List<GetActiveUbrnResponse> Ubrns { get; set; } = new();

    /// <summary>
    /// Searches for a Ubrn with a matching ID 
    /// </summary>
    /// <param name="ubrn"></param>
    /// <returns>The matching record or NULL if it doesn't exist</returns>
    public virtual GetActiveUbrnResponse FindByUbrn(string ubrn)
    {
      GetActiveUbrnResponse result = Ubrns.Find(a => a.Ubrn == ubrn);
      return result;
    }
  }
}
