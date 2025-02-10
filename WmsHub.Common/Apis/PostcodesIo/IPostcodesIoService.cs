using System.Threading.Tasks;

namespace WmsHub.Common.Apis.Ods.PostcodesIo;

public interface IPostcodesIoService
{
  Task<string> GetLsoaAsync(string postcode);
  Task<bool> IsEnglishPostcodeAsync(string postcode);
  Task<bool> IsUkOutwardCodeAsync(string outwardCode);
  Task<bool> IsUkPostcodeAsync(string postcode);
}