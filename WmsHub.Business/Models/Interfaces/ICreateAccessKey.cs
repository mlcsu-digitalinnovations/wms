using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.Interfaces;

public interface ICreateAccessKey
{
  AccessKeyType AccessKeyType { get; set; }
  int ExpireMinutes { get; set; }
  int MaxActiveAccessKeys { get; set; }
  string Email { get; set; }
}