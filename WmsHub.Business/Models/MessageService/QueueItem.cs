using System;
using System.Collections.Generic;

namespace WmsHub.Business.Models.MessageService;

public class QueueItem
{
  public Guid Id { get; set; }
  public string GivenName { get; set; }
  public string Link { get; set; }
  public string NhsNumber { get; set; }
  public string Ubrn { get;  set; }
  public string EmailAddress { get; set; }
  public string MobileNumber { get; set; }
  public string Status { get; set; }
  public string Source { get; set; }
  public string ReferringOrganisationEmail { get; set; }
  public string ReferringClinicianEmail { get; set; }
}

public class IdComparer : IEqualityComparer<QueueItem>
{
  public bool Equals(QueueItem x, QueueItem y)
  {
    return x.Id == y.Id;
  }

  public int GetHashCode(QueueItem obj)
  {
    return obj.Id.GetHashCode();
  }
}
