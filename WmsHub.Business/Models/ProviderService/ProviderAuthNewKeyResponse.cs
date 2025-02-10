using Azure;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService;
public class ProviderAuthNewKeyResponse
{
  public List<string> Errors { get; set; } = new();
  public bool KeySentSuccessfully => MessageTypesSent.Any();
  public List<MessageType> MessageTypesSent { get; set; } = new();
  public Provider Provider { get; set; }

  public ProviderAuthNewKeyResponse()
  { }

  public ProviderAuthNewKeyResponse(Provider provider)
  {
    Provider = provider;
  }

  public string GetAllErrors() => string.Join(" ", Errors);
  public string GetAllMessageTypesSent() => string.Join(", ", MessageTypesSent);
}
