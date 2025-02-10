using WmsHub.Common.Models;

namespace WmsHub.Common.Interface;

public interface IErsSession
{
  /// <summary>
  /// An active session has been started
  /// </summary>
  bool IsAuthenticated { get; }
  /// <summary>
  /// Session Id
  /// </summary>
  string Id { get; set; }
  ErsSession.PermissionModel Permission { get; set; }
  /// <summary>
  /// If True, then the smart card is active and has been authenticated
  /// </summary>
  bool SmartCardIsAuthenticated { get; }
  /// <summary>
  /// The token for the smart card
  /// </summary>
  /// <value></value>
  string SmartCardToken { get; set; }
  string TypeInfo { get; set; }
  ErsSession.UserModel User { get; set; }
}
