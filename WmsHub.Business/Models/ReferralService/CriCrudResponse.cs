using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ReferralService
{
  public class CriCrudResponse: ReferralClinicalInfo
  {
    public CriCrudResponse() { }

    public CriCrudResponse(ReferralClinicalInfo model)
    {
      Ubrn = model.Ubrn;
      ClinicalInfoPdfBase64 = model.ClinicalInfoPdfBase64;
      ClinicalInfoLastUpdated = model.ClinicalInfoLastUpdated;
    }
    public void SetStatus(StatusType status, string message)
    {
      ResponseStatus = status;
      Errors.Add(message);
    }

    public virtual StatusType ResponseStatus { get; set; }

    public virtual List<string> Errors { get; set; }
      = new List<string>();

    public virtual string GetErrorMessage()
    {
      string msg = string.Join(" ", Errors);
      return msg;
    }

  }
}
