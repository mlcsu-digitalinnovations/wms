using System;
using WmsHub.Business.Entities;

namespace WmsHub.Business.Models
{
  public class GeneralReferral : IGeneralReferral
  {
    public int Id { get; set; }

    public Guid ReferralId { get; set; }

    public string Reference { get; set; }

    public string Ubrn => $"GR{Id:0000000000}";
  }
}