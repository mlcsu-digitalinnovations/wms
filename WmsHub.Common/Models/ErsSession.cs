using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace WmsHub.Common.Models
{
  public class ErsSession
  {
    public string TypeInfo { get; set; }

    /// <summary>
    /// Session Id
    /// </summary>
    [Required]
    public string Id { get; set; }

    /// <summary>
    /// The token for the smart card
    /// </summary>
    /// <value></value>
    public string SmartCardToken { get; set; }

    /// <summary>
    /// If True, then the smart card is active and has been authenticated
    /// </summary>
    public bool SmartCardIsAuthenticated
    {
      get
      {
        return !(string.IsNullOrWhiteSpace(SmartCardToken));
      }
    }

    /// <summary>
    /// An active session has been started
    /// </summary>
    public bool IsAuthenticated
    {
      get
      {
        return !(string.IsNullOrWhiteSpace(Id));
      }
    }

    public UserModel User { get; set; }

    public PermissionModel Permission { get; set; }

    public class UserModel
    {
      public string Identifier { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string MiddleName { get; set; }

      public List<PermissionModel> Permissions { get; set; }      
    }

    public class PermissionModel
    {
      public string BusinessFunction { get; set; }
      public string OrgIdentifier { get; set; }
      public string OrgName { get; set; }
    }

  }
}