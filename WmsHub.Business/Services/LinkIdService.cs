using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Exceptions;

namespace WmsHub.Business.Services;
public class LinkIdService : ILinkIdService
{
  private const string BatchSizeToGenerateId = "BatchSizeToGenerate";
  private const string ConfigurationPrefix = "WmsHub_LinkIdService_";
  private const string IdLengthId = "IdLength";
  private const string IsRunningPrefix = "IsRunning:";

  private readonly DatabaseContext _context;
  private static readonly char[] s_idChars = Common.Helpers.Constants.LINKIDCHARS.ToCharArray();

  public LinkIdService()
  { }

  public LinkIdService(DatabaseContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Generates a single id using the same process as GenerateNewIds, with no check for uniqueness
  /// and no storage in the database.
  /// For use as a helper function for unit tests only.
  /// </summary>
  /// <param name="length">Length of the id to be generated (default = 12).</param>
  /// <returns>An id, as a string.</returns>
  public static string GenerateDummyId(int length = 12)
  {
    return GenerateId(length);
  }

  ///<inheritdoc/>
  public virtual async Task GenerateNewIdsAsync(int count)
  {
    int idLength = GetConfigurationValueAsInt(IdLengthId);
    List<LinkId> linkIds = new(count);

    for (int i = 0; i < count; i++)
    {
      string id;
      bool isConflict = false;

      do
      {
        id = GenerateId(idLength);

        isConflict = linkIds
          .Where(t => t.Id == id.ToString())
          .Any();
      }
      while (isConflict);
      linkIds.Add(new() { Id = id.ToString(), IsUsed = false });
    }

    List<LinkId> duplicateIdsInDb = await _context.LinkIds
      .Where(x => linkIds.Contains(x))
      .ToListAsync();

    _context.LinkIds.AddRange(linkIds.Except(duplicateIdsInDb));
    await _context.SaveChangesAsync();

    if (duplicateIdsInDb.Any())
    {
      await GenerateNewIdsAsync(duplicateIdsInDb.Count);
    }
  }

  ///<inheritdoc/>
  public virtual async Task<string> GetUnusedLinkIdAsync(int retries = 0)
  {
    IEnumerable<string> idBatch = await GetUnusedLinkIdBatchAsync(1, retries);
    return idBatch.Single();
  }

  ///<inheritdoc/>
  public virtual async Task<IEnumerable<string>> GetUnusedLinkIdBatchAsync(int count, int retries = 
    0)
  {
    bool isRunning;
    do
    {
      isRunning = GetIsRunning(nameof(GetUnusedLinkIdBatchAsync));
      if (!isRunning)
      {
        break;
      }
      else
      {
        Thread.Sleep(1000);
        retries--;
      }
    } while (retries >= 0);

    if (isRunning)
    {
      throw new ProcessAlreadyRunningException($"{nameof(GetUnusedLinkIdBatchAsync)} is already " +
        $"running.");
    }

    try
    {
      SetIsRunning(nameof(GetUnusedLinkIdBatchAsync), true);

      int availableIds = _context.LinkIds.Where(x => !x.IsUsed).Count();     

      if (availableIds < count)
      {
        int numberToGenerate = Math.Max(
          count-availableIds,
          GetConfigurationValueAsInt(BatchSizeToGenerateId));
        await GenerateNewIdsAsync(numberToGenerate);
      }

      List<LinkId> linkIds = await _context.LinkIds.Where(x => !x.IsUsed).Take(count).ToListAsync();
      foreach (LinkId linkId in linkIds)
      {
        linkId.IsUsed = true;
      }

      await _context.SaveChangesAsync();

      return linkIds.Select(x => x.Id);
    }
    finally
    {
      SetIsRunning(nameof(GetUnusedLinkIdBatchAsync), false);
    }
  }

  private static string GenerateId(int length)
  {
    if (length < 1 || length > 200)
    {
      throw new ArgumentOutOfRangeException(nameof(length));
    }

    StringBuilder id = new();
    Random random = new();
    while (id.Length < length)
    {
      id.Append(s_idChars[random.Next(s_idChars.Length)]);
    }

    return id.ToString();
  }

  private ConfigurationValue GetConfigurationValue(string id)
  {
    ArgumentNullException.ThrowIfNull(id);

    return _context.ConfigurationValues
      .Where(x => x.Id == $"{ConfigurationPrefix}{id}")
      .SingleOrDefault();
  }

  private int GetConfigurationValueAsInt(string id)
  {
    if (int.TryParse(GetConfigurationValueAsString(id), out int value))
    {
      return value;
    }
    else
    {
      throw new InvalidOptionsException($"Configuration value for {ConfigurationPrefix}{id} " +
        "could not be parsed to int.");
    }
  }

  private string GetConfigurationValueAsString(string id)
  {
    ConfigurationValue configurationValue = GetConfigurationValue(id);

    return configurationValue != null ? configurationValue.Value
      : throw new InvalidOptionsException($"No configuration value found for " +
      $"{ConfigurationPrefix}{id}");
  }

  private bool GetIsRunning(string method)
  {
    if (string.IsNullOrWhiteSpace(method))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(method));
    }

    ConfigurationValue isRunningConfigurationValue =
      GetConfigurationValue($"{IsRunningPrefix}{method}");

    if (isRunningConfigurationValue != null 
      && bool.TryParse(isRunningConfigurationValue.Value, out bool isRunning))
    {
      return isRunning;
    }
    else
    {
      SetIsRunning(method, false);
      return false;
    }
  }

  private bool SetIsRunning(string method, bool isRunning)
  {
    if (string.IsNullOrWhiteSpace(method))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(method));
    }

    ConfigurationValue existingConfigurationValue = 
      GetConfigurationValue($"{IsRunningPrefix}{method}");

    if (existingConfigurationValue == null)
    {
      _context.ConfigurationValues.Add(new()
      {
        Id = $"{ConfigurationPrefix}{IsRunningPrefix}{method}",
        Value = isRunning.ToString()
      });
    }
    else
    {
      existingConfigurationValue.Value = isRunning.ToString();
    }

    _context.SaveChanges();
    return isRunning;
  }
}
