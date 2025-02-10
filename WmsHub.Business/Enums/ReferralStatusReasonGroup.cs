using System;

namespace WmsHub.Business.Enums;

[Flags]
public enum ReferralStatusReasonGroup : int
{
  RmcRejected = 1,
  ProviderRejected = 2,
  ProviderDeclined = 4,
  ProviderTerminated = 8
}

public static class ReferralStatusReasonGroupConstants
{
  public readonly static ReferralStatusReasonGroup RmcStatuses =
    ReferralStatusReasonGroup.RmcRejected;

  public readonly static ReferralStatusReasonGroup ProviderStatuses =
    ReferralStatusReasonGroup.ProviderDeclined
    | ReferralStatusReasonGroup.ProviderRejected
    | ReferralStatusReasonGroup.ProviderTerminated;
}