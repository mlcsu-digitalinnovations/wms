using WmsHub.Common.Models;

namespace WmsHub.ErsMock.Api.Models.ReferralRequest;

public class ErsWorkListItemHelper : ERSWorkListItem
{
  public static ERSWorkListItem CreateErsWorkListItem(string Ubrn)
  {
    ERSWorkListItem ersWorkListItem = new()
    {
      Reference = $"UBRN{REFERENCE_SEPARATOR}{Ubrn}"
    };

    return ersWorkListItem;
  }
}
