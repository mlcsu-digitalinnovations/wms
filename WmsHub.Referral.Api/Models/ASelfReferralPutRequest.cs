using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models
{
  public abstract class ASelfReferralPutRequest
  {
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid ProviderId { get; set; }
  }
}