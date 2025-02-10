using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services;

public class EthnicityService
  : ServiceBase<Entities.Ethnicity>, IEthnicityService
{
  private readonly IMapper _mapper;

  public EthnicityService(DatabaseContext context, IMapper mapper)
    : base(context)
  {
    _mapper = mapper;
  }

  public async Task<IEnumerable<Ethnicity>> GetAsync()
  {
    List<Ethnicity> ethnicities = await _context
      .Ethnicities
      .Where(e => e.IsActive)
      .AsNoTracking()
      .OrderBy(g => g.GroupOrder)
      .OrderBy(d => d.DisplayOrder)
      .ProjectTo<Ethnicity>(_mapper.ConfigurationProvider)
      .ToListAsync();

    return ethnicities;
  }

  public async Task<Ethnicity> GetByMultiple(string ethnicity)
  {
    if (string.IsNullOrWhiteSpace(ethnicity))
    {
      return null;
    }
    else
    {
      IEnumerable<Ethnicity> ethnicities = await GetAsync();

      ethnicity = ethnicity.Trim();
      
      Ethnicity matchedEthnicity = ethnicities
        .OrderBy(x => x.DisplayOrder)
        .FirstOrDefault(x => x.IsMatch(ethnicity));

      return matchedEthnicity;
    }
  }

  public async Task<IEnumerable<Ethnicity>> GetEthnicityGroupMembersAsync(
    string groupName)
  {
    IEnumerable<Ethnicity> ethnicities = await _context
       .Ethnicities
       .Where(e => e.IsActive)
         .Where(e => e.GroupName == groupName)
       .AsNoTracking()
       .OrderBy(e => e.DisplayOrder)
       .ProjectTo<Ethnicity>(_mapper.ConfigurationProvider)
       .ToListAsync();

    return ethnicities;
  }

  public async Task<IEnumerable<string>> GetEthnicityGroupNamesAsync()
  {
    IEnumerable<string> ethnicityGroups = await _context
      .Ethnicities
      .Where(e => e.IsActive)
      .AsNoTracking()
        .OrderBy(g => g.GroupOrder)
        .Select(g => g.GroupName)
      .ToListAsync();

    return ethnicityGroups.Distinct().ToList();
  }

  public async Task<bool> IsBmiValidByTriageNameAsync(
    string triageName,
    decimal bmi)
  {
    if (triageName is null)
    {
      throw new ArgumentNullException(nameof(triageName));
    }

    Ethnicity ethnicity = (await GetAsync())
      .FirstOrDefault(x => x.TriageName == triageName);

    if (ethnicity == null)
    {
      throw new EthnicityNotFoundException(
        $"Ethnicity not found with a TriageName of '{triageName}'"); 
    }

    if (ethnicity.MinimumBmi > bmi)
    {
      return false;
    }

    return true;
  }
}