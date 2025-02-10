using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Services;
public class MskOrganisationService
  : ServiceBase<Entities.MskOrganisation>, IMskOrganisationService
{
  public MskOrganisationService(DatabaseContext context) : base(context)
  { }

  /// <inheritdoc/>
  public virtual async Task<MskOrganisation> AddAsync(
    MskOrganisation organisation)
  {
    if (organisation == null)
    {
      throw new ArgumentNullException(nameof(organisation));
    }

    ValidateMskOrganisation(organisation);

    Entities.MskOrganisation entity = await _context
      .MskOrganisations
      .Where(o => o.OdsCode == organisation.OdsCode)
      .SingleOrDefaultAsync();

    if (entity != null && entity.IsActive)
    {
      throw new InvalidOperationException(
        $"An organisation with the ODS code {organisation.OdsCode} already " +
          "exists.");
    }

    if (entity == null)
    {
      entity = new();
      _context.MskOrganisations.Add(entity);
    }

    entity.IsActive = true;
    entity.OdsCode = organisation.OdsCode;
    entity.SendDischargeLetters = organisation.SendDischargeLetters;
    entity.SiteName = organisation.SiteName;

    UpdateModified(entity);
    await _context.SaveChangesAsync();

    return new MskOrganisation()
    {
      OdsCode = entity.OdsCode,
      SendDischargeLetters = entity.SendDischargeLetters,
      SiteName = entity.SiteName
    };
  }

  /// <inheritdoc/>
  public virtual async Task<string> DeleteAsync(string odsCode)
  {
    if (string.IsNullOrWhiteSpace(odsCode))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(odsCode));
    }

    Entities.MskOrganisation entity = await _context
      .MskOrganisations
      .Where(x => x.OdsCode == odsCode)
      .FirstOrDefaultAsync();

    if (entity == null || !entity.IsActive)
    {
      throw new MskOrganisationNotFoundException(
        $"An organisation with the ODS code {odsCode} does not exist.");
    }

    entity.IsActive = false;
    UpdateModified(entity);
    await _context.SaveChangesAsync();

    return $"Referral with ODS code {odsCode} deleted.";
  }

  /// <inheritdoc/>
  public virtual async Task<bool> ExistsAsync(string odsCode)
  {
    if (string.IsNullOrWhiteSpace(odsCode))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(odsCode));
    }

    return await _context
      .MskOrganisations
      .Where(o => o.IsActive)
      .Where(o => o.OdsCode == odsCode)
      .AnyAsync();
  }

  /// <inheritdoc/>
  public virtual async Task<IEnumerable<MskOrganisation>> GetAsync()
  {
    List<MskOrganisation> organisations = await _context
      .MskOrganisations
      .Where(o => o.IsActive)
      .OrderBy(o => o.OdsCode)
      .Select(o => new MskOrganisation()
      {
        OdsCode = o.OdsCode,
        SendDischargeLetters = o.SendDischargeLetters,
        SiteName = o.SiteName
      })
      .ToListAsync();

    return organisations;
  }

  /// <inheritdoc/>
  public virtual async Task<MskOrganisation> GetAsync(string odsCode)
  {
    return await _context
      .MskOrganisations
      .Where(o => o.IsActive)
      .Where(o => o.OdsCode == odsCode)
      .Select(o => new MskOrganisation()
      {
        OdsCode = o.OdsCode,
        SendDischargeLetters = o.SendDischargeLetters,
        SiteName = o.SiteName
      })
      .SingleOrDefaultAsync();
  }

  /// <inheritdoc/>
  public virtual async Task<MskOrganisation> UpdateAsync(
    MskOrganisation organisation)
  {
    if (organisation == null)
    {
      throw new ArgumentNullException(nameof(organisation));
    }

    ValidateMskOrganisation(organisation);

    Entities.MskOrganisation entity = await _context
      .MskOrganisations
      .Where(x => x.OdsCode == organisation.OdsCode)
      .FirstOrDefaultAsync();

    if (entity == null)
    {
      throw new MskOrganisationNotFoundException(
        $"An organisation with the ODS code {organisation.OdsCode} does not " +
          "exist.");
    }

    entity.IsActive = true;
    entity.OdsCode = organisation.OdsCode;
    entity.SendDischargeLetters = organisation.SendDischargeLetters;
    entity.SiteName = organisation.SiteName;

    UpdateModified(entity);
    await _context.SaveChangesAsync();

    return new MskOrganisation()
    {
      OdsCode = entity.OdsCode,
      SendDischargeLetters = entity.SendDischargeLetters,
      SiteName = entity.SiteName
    };
  }

  private static void ValidateMskOrganisation(MskOrganisation mskOrganisation)
  {
    ValidateModelResult result = Validators.ValidateModel(mskOrganisation);

    if (!result.IsValid)
    {
      throw new MskOrganisationValidationException(result.Results);
    }
  }
}
