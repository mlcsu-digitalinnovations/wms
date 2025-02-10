using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.Interfaces;

public interface IValidateAccessKey
{
  AccessKeyType Type { get; set; }
  int MaxActiveAccessKeys { get; set; }
  string AccessKey { get; set; }
  string Email { get; set; }
}