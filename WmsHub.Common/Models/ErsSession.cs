using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Interface;

namespace WmsHub.Common.Models;

public class ErsSession : IErsSession
{
  /// <inheritdoc/>
  public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Id);
  /// <inheritdoc/>
  [Required]
  public string Id { get; set; }
  /// <inheritdoc/>
  public PermissionModel Permission { get; set; }
  public class PermissionModel
  {
    public string BusinessFunction { get; set; }
    public string OrgIdentifier { get; set; }
    public string OrgName { get; set; }
  }
  /// <inheritdoc/>
  public bool SmartCardIsAuthenticated => !string.IsNullOrWhiteSpace(SmartCardToken);
  /// <inheritdoc/>
  public string SmartCardToken { get; set; }
  public string TypeInfo { get; set; }
  public UserModel User { get; set; }
  public class UserModel
  {
    public string Identifier { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }

    public List<PermissionModel> Permissions { get; set; }
  }
}
