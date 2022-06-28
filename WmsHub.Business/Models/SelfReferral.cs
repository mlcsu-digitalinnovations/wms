using System;
using WmsHub.Business.Entities;

namespace WmsHub.Business.Models
{
  public class SelfReferral : ISelfReferral
  {
    public int Id { get; set; }

    public Guid ReferralId { get; set; }

    public string Reference { get; set; }

    public string Ubrn => $"SR{Id:0000000000}";
  }
}
