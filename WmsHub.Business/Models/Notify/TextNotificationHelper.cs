using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Interfaces;

namespace WmsHub.Business.Models.Notify
{
    public class TextNotificationHelper : ITextNotificationHelper
  {
    private IAsyncNotificationClient _textClient;
    private readonly ITextOptions _options;
    public TextNotificationHelper(IOptions<TextOptions> options)
    {
      _options = options.Value;
    }

    public virtual IAsyncNotificationClient TextClient
    {
      get => _textClient
        ?? (_textClient = new NotificationClient(_options.SmsApiKey));
      set => _textClient = value;
    }
  }
}
