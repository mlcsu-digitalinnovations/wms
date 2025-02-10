using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.ElectiveCareReferral;

public class ElectiveCareUserData 
{
  public string Action { get; set; }
  [EmailAddress]
  public string EmailAddress { get; set; }
  public string GivenName { get; set; }
  public string ODSCode { get; set; }
  public int RowNumber { get; private set; }
  public string Surname { get; set; }
}
