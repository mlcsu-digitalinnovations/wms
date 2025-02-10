using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models.MSGraph;

namespace WmsHub.Business.Services.Interfaces;

public interface IMSGraphService : IServiceBase
{
  /// <summary>
  /// Uses the supplied spreadsheet to create new user if they 
  /// do not already exist.
  /// </summary>
  /// <param name="user">MsGraph.CreateUser</param>
  /// <returns>
  /// <list type="bullet">
  /// <item><description>ElectiveCareUser when valid.</description></item>
  /// <item><description>ArgumentNullException if user is null.
  /// </description></item>
  /// <item><description>Null if try->catch exception (logged as error)
  /// </description></item>
  /// </list>
  /// ElectiveCareUser
  /// </returns>
  Task<ElectiveCareUser> CreateElectiveCareUserAsync(
    string bearerToken,
    CreateUser user);
  /// <summary>
  /// Uses the supplied spreadsheet to delete a user if they exist.
  /// </summary>
  /// <param name="user">MsGraph.DeleteUser</param>
  /// <returns>Boolean</returns>
  Task<bool> DeleteUserByIdAsync(string bearerToken, DeleteUser user);

  /// <summary>
  /// Uses the Configuration Values, ClientId, TenantID and Secret to fetch 
  /// a valid Bearer token with a time limit of 3600 seconds.
  /// </summary>
  /// <returns>Access Token</returns>
  Task<string> GetBearerTokenAsync();
  /// <summary>
  /// Creates a filter that searches for a user by their Email Address.  This
  /// validates the user found by the issuer as they may have other 
  /// credentials.<br />
  /// The token expiry time is reduced by 60 seconds to ensure that a
  /// transaction isn't in progress when the token expires.
  /// </summary>
  /// <param name="email"></param>
  /// <param name="issuer"></param>
  /// <returns></returns>
  Task<List<FilteredUser>> GetUsersByEmailAsync(
    string bearerToken,
    string email,
    string issuer);
}
