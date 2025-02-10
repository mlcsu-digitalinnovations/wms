using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.ReferralService
{
  public class GeneralReferralUpdate
    : AGeneralReferralCreate, IGeneralReferralUpdate
  {
    [Required]
    public Guid Id { get; set; }
  }
}