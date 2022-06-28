using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services
{
  public class RequestResponseLogService : IRequestResponseLogService
  {
    private readonly DatabaseContext _context;
    public RequestResponseLogService(DatabaseContext context)
    {
      _context = context;
    }

    public async Task CreateAsync(IRequestResponseLog model)
    {
      Entities.RequestResponseLog entity = new Entities.RequestResponseLog();
      model.MapToEntity(entity);
      _context.Add(entity);

      await _context.SaveChangesAsync();
    }
  }
}