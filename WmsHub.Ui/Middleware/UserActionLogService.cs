using System.Threading.Tasks;
using WmsHub.Business.Entities;

namespace WmsHub.Business.Services
{
  public class UserActionLogService : IUserActionLogService
  {
    private readonly DatabaseContext _context;
    public UserActionLogService(DatabaseContext context)
    {
      _context = context;
    }

    public async Task CreateAsync(IUserActionLog entity)
    {
      _context.Add(entity);
      await _context.SaveChangesAsync();
    }
  }
}