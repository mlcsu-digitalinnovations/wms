using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services.Interfaces;
public interface IMskOrganisationService : IServiceBase
{
  /// <summary>
  /// Adds a new valid MskOrganisation.
  /// </summary>
  /// <param name="organisation">The MskOrganisation to be added.</param>
  /// <returns>The created MskOrganisation.</returns>
  Task<MskOrganisation> AddAsync(MskOrganisation organisation);
  Task<string> DeleteAsync(string odsCode);
  /// <summary>
  /// Checks whether an active MskOrganisation with the provided OdsCode 
  /// exists.
  /// </summary>
  /// <param name="odsCode">The OdsCode to be checked.</param>
  /// <returns>A boolean value of whether an active MskOrganisation exists.
  /// </returns>
  Task<bool> ExistsAsync(string odsCode);
  /// <summary>
  /// Returns a list of all MskOrganisations.
  /// </summary>
  /// <returns>A list of all MskOrganisations.</returns>
  Task<IEnumerable<MskOrganisation>> GetAsync();
  /// <summary>
  /// Returns a single MskOrganisation matching the provided OdsCode, if it 
  /// exists.
  /// </summary>
  /// <param name="odsCode">The OdsCode of the MskOrganisation to be returned.
  /// </param>
  /// <returns>The MskOrganisation matching the provided OdsCode, or null if 
  /// no active matching MskOrganisation exists.</returns>
  Task<MskOrganisation> GetAsync(string odsCode);
  /// <summary>
  /// Updates an MskOrganisation to match the values of the provided
  /// MskOrganisation.
  /// </summary>
  /// <param name="organisation">An MskOrganisation object with property values
  ///  the stored MskOrganisation should be updated to.</param>
  /// <returns>The updated MskOrganisation.</returns>
  Task<MskOrganisation> UpdateAsync(MskOrganisation organisation);
}
