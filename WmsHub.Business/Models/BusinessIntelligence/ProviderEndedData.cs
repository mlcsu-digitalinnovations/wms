using System;

namespace WmsHub.Business.Models;

public class ProviderEndedData
{
  public DateTimeOffset? FromDate { get; set; }
  public DateTimeOffset? ToDate { get; set; }
  /// <summary>
  /// ReferralStatus.ProviderDeclinedByServiceUser
  /// </summary>
  public int Declined { get; set; }
  /// <summary>
  /// ReferralStatus.ProviderRejected
  /// </summary>
  public int Rejected { get; set; }
  /// <summary>
  /// ReferralStatus.ProviderTerminated
  /// </summary>
  public int Terminated { get; set; }
}