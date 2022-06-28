using System.Threading.Tasks;

namespace WmsHub.Business.Services
{
  public interface IPostcodeService
  {
    Task<string> GetLsoa(string postcode);
  }
}