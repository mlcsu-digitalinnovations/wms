using System;

namespace WmsHub.Business.Models
{
  public interface IMskReferral
  {
    int Id { get; set; }
    Guid ReferralId { get; set; }
    string Ubrn { get; }
  }
}