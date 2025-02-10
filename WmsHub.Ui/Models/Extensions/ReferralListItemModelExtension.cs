using WmsHub.Business.Enums;
using WmsHub.Business.Migrations;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Ui.Models.Extensions;

public static class ReferralListItemModelExtension
{
  public static bool GetCanOverrideException(this ReferralListItemModel model)
  {
    if (model == null)
    {
      return false;
    }

    if (model.Status == ReferralStatus.Exception.ToString() 
      && model.StatusReason != WarningMessages.NO_ATTACHMENT)
    {
      return true;
    }

    return false;
  }
}
