using System;
using System.Diagnostics.CodeAnalysis;
using WmsHub.ReferralsService.Models.BaseClasses;

namespace WmsHub.ReferralsService.Models.Results
{
  [ExcludeFromCodeCoverage]
  public class GetCriResult : ReferralsResult
  {
    public byte[] CriDocument { get; set; }
    public bool NoCriDocumentFound { get; set; }
  }
}
