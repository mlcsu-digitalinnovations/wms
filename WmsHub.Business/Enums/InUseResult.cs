using System;

namespace WmsHub.Business.Enums
{
  [Flags]
  public enum InUseResult
  {
    Found = 1,
    NotFound = 2,
    ProviderSelected = 4,
    ProviderNotSelected = 8,
    Complete = 16,
    Cancelled = 32,
    IsGeneralReferral = 64,
    IsNotGeneralReferral = 128
  }
}