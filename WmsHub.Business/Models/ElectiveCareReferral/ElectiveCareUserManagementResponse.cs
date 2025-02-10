using System.Collections.Generic;
using System.Linq;

namespace WmsHub.Business.Models.ElectiveCareReferral;
public class ElectiveCareUserManagementResponse
{
  public List<string> Errors { get; set; } = new();
  public bool IsValid => Errors == null || !Errors.Any();
  public int Processed { get; set; } = 0;
  public int UsersAdded { get; set; } = 0;
  public int UsersRemoved { get; set; } = 0;

  public void Add(string error)
  {
    Errors ??= new List<string>();
    Errors.Add(error);
  }
  public int ErrorCount => Errors.Count;
  public string ErrorMessages => string.Join(", ", Errors);
  
  
}
