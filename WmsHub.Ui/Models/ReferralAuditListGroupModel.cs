using System;
using System.Collections.Generic;
using System.Linq;

namespace WmsHub.Ui.Models;

public class ReferralAuditListGroupModel
{
  public ReferralAuditListGroupModel(Guid id, string nhsNumber)
  {
    MasterId = id;
    NhsNumber = nhsNumber;
  }

  private bool HasPreviousHistory => PastItems.Any();
  public Guid MasterId { get; set; }
  public string NhsNumber { get; set; }
  public Dictionary<Guid, IEnumerable<ReferralAuditListItemModel>> PastItems 
  { get; set; }= new();
}