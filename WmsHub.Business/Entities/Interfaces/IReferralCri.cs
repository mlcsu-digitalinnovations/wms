using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Entities.Interfaces
{
  public interface IReferralCri
  {
    DateTimeOffset ClinicalInfoLastUpdated { get; set; }
    Guid Id { get; set; }
    bool IsActive { get; set; }
    DateTimeOffset ModifiedAt { get; set; }
    Guid ModifiedByUserId { get; set; }
  }
}
