using System;
using System.ComponentModel;

namespace WmsHub.Business.Enums
{
  [Flags]
  public enum ReferralSource
  {
    [Description("General Practice Referral")]
    GpReferral = 1,
    [Description("NHS Staff Self Referral")]
    SelfReferral = 2,
    [Description("Community Pharmacy Referral")]
    Pharmacy = 4,
    [Description("Public Self Referral")]
    GeneralReferral = 8,
    [Description("Community MSK Clinic Referral")]
    Msk = 16,
    [Description("Elective Care Referral")]
    ElectiveCare = 32
  }
}
