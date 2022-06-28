using System;

namespace WmsHub.Business.Models
{
  public class MskReferral : IMskReferral
  {
    public int Id { get; set; }

    public Guid ReferralId { get; set; }

    public string Ubrn => $"MSK{Id:000000000}";
  }
}