using System;

namespace WmsHub.Business.Entities
{
  public class ReferralCriBase: BaseEntity
  {
    public DateTimeOffset ClinicalInfoLastUpdated { get; set; }
    public Guid? UpdateOfCriId { get; set; }
  }
}