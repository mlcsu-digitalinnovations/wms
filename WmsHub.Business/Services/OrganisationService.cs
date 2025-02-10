using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Common.Exceptions;

namespace WmsHub.Business.Services;

public class OrganisationService 
  : ServiceBase<Entities.Organisation>, IOrganisationService
{
  public OrganisationService(DatabaseContext context)
    : base(context)
  { }

  public virtual async Task<Organisation> AddAsync(Organisation organisation)
  {
    if (organisation is null)
    {
      throw new ArgumentNullException(nameof(organisation));
    }

    Entities.Organisation entity = await _context
      .Organisations
      .Where(x => x.OdsCode == organisation.OdsCode)
      .FirstOrDefaultAsync();

    if (entity != null && entity.IsActive)
    {
      throw new InvalidOperationException(
        $"An organisation with the ODS code {organisation.OdsCode} already exists.");
    }

    if (entity is null)
    {
      entity = new();
      _context.Organisations.Add(entity);
    }
    
    entity.IsActive = true;
    entity.OdsCode = organisation.OdsCode;
    entity.QuotaRemaining = organisation.QuotaRemaining;
    entity.QuotaTotal = organisation.QuotaTotal;

    UpdateModified(entity);

    await _context.SaveChangesAsync();

    return new Organisation()
    {
      OdsCode = entity.OdsCode,
      QuotaRemaining = entity.QuotaRemaining,
      QuotaTotal = entity.QuotaTotal,
    };
  }

  public virtual async Task DeleteAsync(string odsCode)
  {
    if (string.IsNullOrWhiteSpace(odsCode))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(odsCode));
    }

    Entities.Organisation entity = await _context
      .Organisations
      .Where(x => x.OdsCode == odsCode)
      .FirstOrDefaultAsync();

    if (entity == null || !entity.IsActive)
    {
      throw new InvalidOperationException(
        $"An organisation with the ODS code {odsCode} does not exist.");
    }

    entity.IsActive = false;

    UpdateModified(entity);

    await _context.SaveChangesAsync();
  }

  public virtual async Task<IEnumerable<Organisation>> GetAsync()
  {
    List<Organisation> organisations = await _context
      .Organisations
      .Where(x => x.IsActive)
      .OrderBy(x => x.OdsCode)
      .Select(x => new Organisation()
      {
        OdsCode = x.OdsCode,
        QuotaRemaining = x.QuotaRemaining,
        QuotaTotal = x.QuotaTotal
      })
      .ToListAsync();

    return organisations;
  }

  public virtual async Task ResetOrganisationQuotas()
  {
    List<Entities.Organisation> organisations = await _context
      .Organisations
      .Where(x => x.IsActive)
      .ToListAsync();

    foreach (Entities.Organisation organisation in organisations)
    {
      organisation.QuotaRemaining = organisation.QuotaTotal;
      UpdateModified(organisation);
    }

    await _context.SaveChangesAsync();
  }

  public virtual async Task<Organisation> UpdateAsync(Organisation organisation)
  {
    if (organisation is null)
    {
      throw new ArgumentNullException(nameof(organisation));
    }

    Entities.Organisation entity = await _context
      .Organisations
      .Where(x => x.OdsCode == organisation.OdsCode)
      .FirstOrDefaultAsync();

    if (entity == null)
    {
      throw new InvalidOperationException(
        $"An organisation with the ODS code {organisation.OdsCode} does not exist.");
    }

    entity.IsActive = true;
    entity.OdsCode = organisation.OdsCode;
    entity.QuotaRemaining = organisation.QuotaRemaining;
    entity.QuotaTotal = organisation.QuotaTotal;

    UpdateModified(entity);

    await _context.SaveChangesAsync();

    return new Organisation()
    {
      OdsCode = entity.OdsCode,
      QuotaRemaining = entity.QuotaRemaining,
      QuotaTotal = entity.QuotaTotal,
    };
  }
}
