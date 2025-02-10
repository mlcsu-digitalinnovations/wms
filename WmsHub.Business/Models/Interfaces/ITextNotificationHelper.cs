using Notify.Interfaces;

namespace WmsHub.Business.Models.Notify
{
    public interface ITextNotificationHelper
  {
    IAsyncNotificationClient TextClient { get; set; }
  }
}