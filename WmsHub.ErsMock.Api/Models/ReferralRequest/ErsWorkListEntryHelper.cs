using WmsHub.Common.Models;

namespace WmsHub.ErsMock.Api.Models.ReferralRequest;

public class ErsWorkListEntryHelper : ErsWorkListEntry
{
  public static ErsWorkListEntry CreateErsWorkListEntry(
    string NhsNumber,
    string Ubrn)
  {

    ErsWorkListEntry ersWorkListEntry = new()
    {
      Extension = new[]
      {
        new ExtensionModel()
        {
          Extension = new()
          {
            new ExtensionModel.ExtensionSubModel()
            {
              ValueReference = new ()
              {
                Reference = $"NHS_NUMBER{REFERENCE_SEPARATOR}{NhsNumber}"
              },
              Url = EXTENSION_URL_PATIENT,
            }
          }
        },
        new ExtensionModel()
        {
          Extension = new()
          {
            new ExtensionModel.ExtensionSubModel()
            {
              ValueDateTime = DateTimeOffset.Now,
              Url = EXTENSION_CRI_UPDATED,
            }
          }
        }
      },
      Item = ErsWorkListItemHelper.CreateErsWorkListItem(Ubrn)
    };

    return ersWorkListEntry;
  }
}
