using WmsHub.Business.Enums;

namespace WmsHub.Business.Helpers;
public static class GpDocumentProxyHelper
{
  public static string[] ProgrammeOutcomesRequiringMessage()
  {
    return
    [
      ProgrammeOutcome.RejectedBeforeProviderSelection.ToString(),
      ProgrammeOutcome.RejectedAfterProviderSelection.ToString()
    ];
  }
}
