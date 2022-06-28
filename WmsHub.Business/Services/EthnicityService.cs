using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace WmsHub.Business.Services
{
  public class EthnicityService
    : ServiceBase<Entities.Ethnicity>, IEthnicityService
  {
    private readonly IMapper _mapper;

    public EthnicityService(DatabaseContext context, IMapper mapper)
      : base(context)
    {
      _mapper = mapper;
    }

    public async Task<IEnumerable<Models.Ethnicity>> Get()
    {

      IEnumerable<Models.Ethnicity> ethnicities = await _context
         .Ethnicities
         .Where(e => e.IsActive)
         .AsNoTracking()
         .OrderBy(g => g.GroupOrder)
         .OrderBy(d => d.DisplayOrder)
         .ProjectTo<Models.Ethnicity>(_mapper.ConfigurationProvider)
         .ToListAsync();

      return ethnicities;
    }

    public async Task<IEnumerable<Models.Ethnicity>>
			GetEthnicityGroupMembersAsync(string groupName)
    {
      IEnumerable<Models.Ethnicity> ethnicities = await _context
         .Ethnicities
         .Where(e => e.IsActive)
				 .Where(e => e.GroupName == groupName)
         .AsNoTracking()
         .OrderBy(e => e.DisplayOrder)
         .ProjectTo<Models.Ethnicity>(_mapper.ConfigurationProvider)
         .ToListAsync();

      return ethnicities;
    }

    public async Task<IList<string>> GetEthnicityGroupNamesAsync()
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
  }
}