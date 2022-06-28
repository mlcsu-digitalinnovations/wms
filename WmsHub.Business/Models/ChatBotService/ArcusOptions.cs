using System;
using System.Diagnostics.CodeAnalysis;
using WmsHub.Business.Enums;
using WmsHub.Common.Models;

namespace WmsHub.Business.Models.ChatBotService
{
  [ExcludeFromCodeCoverage]
  public class ArcusOptions : NumberWhiteListOptions, INumberWhiteListOptions
  {
    public const string SectionKey = "ArcusSettings";

    public string ApiKey { get; set; }
    public string Endpoint { get; set; }
    public string ContactFlowName { get; set; }
      = "NHS Weight Management Service";
    public int ReturnLimit { get; set; } = 600;
    public DomainAccess Access => DomainAccess.ChatBotApi;
  }
}
