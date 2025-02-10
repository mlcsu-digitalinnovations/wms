using System.Collections.Generic;
using WmsHub.Business.Models;
using System.Threading.Tasks;

namespace WmsHub.Business.Services
{
  public interface ICsvExportService
  {
    byte[] Export<TAttribute>(IEnumerable<Referral> referrals)
      where TAttribute : ExportAttribute;
  }
}
