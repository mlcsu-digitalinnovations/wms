using System.Collections.Generic;
using System.Threading.Tasks;

namespace WmsHub.Business.Services.Interfaces;
public interface ILinkIdService
{
  /// <summary>
  /// Generates a batch of new unique ids (n = count) and stores them in the LinkIds table.
  /// </summary>
  /// <param name="count">Number of ids to be generated.</param>
  public Task GenerateNewIdsAsync(int count);
  /// <summary>
  /// Gets a single LinkId from the LinkIds table with IsUsed = False, sets IsUsed to True and 
  /// returns the Id value.
  /// </summary>
  /// <param name="retries">
  /// Number of times to retry the process if the process is already running.
  /// </param>
  /// <returns>A single id, as a string.</returns>
  public Task<string> GetUnusedLinkIdAsync(int retries = 0);
  /// <summary>
  /// Gets a batch of LinkIds (n = count) from the LinkIds table with IsUsed = False, sets IsUsed to
  /// True and returns the batch of Id values.
  /// </summary>
  /// <param name="count">Number of ids to be returned.</param>
  /// <param name="retries">
  /// Number of times to retry the process if the process is already running.
  /// </param>
  /// <returns>A collection of ids, as strings,</returns>
  public Task<IEnumerable<string>> GetUnusedLinkIdBatchAsync(int count, int retries = 0);
}
