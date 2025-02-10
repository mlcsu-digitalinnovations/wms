using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;

namespace WmsHub.Business.Models;
public class ReferralSourceInfo
{
  public string Name { get; set; }
  public int NoOfActiveReferrals { get; set; }
  public int NoOfCompleteReferrals { get; set; }
  public int NoOfCancelledReferrals { get; set; }

  public ReferralSourceInfo() { }
  public ReferralSourceInfo(string source,
                            int noOfActiveReferrals,
                            int noOfCompleteReferrals,
                            int noOfCancelledReferrals)
  {
    Name = source switch
    {
      nameof(ReferralSource.ElectiveCare) => "Elective Care",
      nameof(ReferralSource.GpReferral) => "GP",
      nameof(ReferralSource.GeneralReferral) => "Public",
      nameof(ReferralSource.Msk) => "MSK",
      nameof(ReferralSource.Pharmacy) => "Pharmacy",
      nameof(ReferralSource.SelfReferral) => "Staff",
      _ => throw new ReferralSourceNotFoundException(
        $"Label not defined for {source}")
    };

    NoOfActiveReferrals = noOfActiveReferrals;
    NoOfCompleteReferrals = noOfCompleteReferrals;
    NoOfCancelledReferrals = noOfCancelledReferrals;
  }
}
