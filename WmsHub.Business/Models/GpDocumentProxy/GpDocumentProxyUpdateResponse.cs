using System;
using System.Collections.Generic;
using System.Linq;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.GpDocumentProxy;

public class GpDocumentProxyUpdateResponse
{
  public int CountOfAwaitingDischarge => GetCountOfSpecificUpdates(
    d => d.Status == ReferralStatus.AwaitingDischarge.ToString());

  public int CountOfComplete => GetCountOfSpecificUpdates(
    d => d.Status == ReferralStatus.Complete.ToString());

  public int CountOfDischargeAwaitingTrace => GetCountOfSpecificUpdates(
    d => d.Status == ReferralStatus.DischargeAwaitingTrace.ToString());

  public int CountOfSentForDischarge => GetCountOfSpecificUpdates(
    d => d.Status == ReferralStatus.SentForDischarge.ToString());

  public int CountOfUpdated => GetCountOfSpecificUpdates(
    d => d.UpdateStatus == DocumentUpdateStatus.Updated.ToString());

  public int CountOfNotUpdated => GetCountOfSpecificUpdates(
    d => d.UpdateStatus == DocumentUpdateStatus.NotUpdated.ToString());

  public int CountOfError => GetCountOfSpecificUpdates(
    d => d.UpdateStatus == DocumentUpdateStatus.Error.ToString());

  public List<GpDocumentProxyUpdateResponseItem> Discharges { get; set; }
    = new();

  private int GetCountOfSpecificUpdates(
    Func<GpDocumentProxyUpdateResponseItem, bool> search)
  {
    if (Discharges == null)
    {
      return 0;
    }

    int count = Discharges
      .Where(search)
      .Count();

    return count;
  }
}