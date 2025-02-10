using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WmsHub.Business.Entities
{
  public class GeneralReferral : IGeneralReferral
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Guid ReferralId { get; set; }
  }
}