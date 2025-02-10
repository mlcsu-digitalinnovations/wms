namespace WmsHub.Business.Models;

public interface IPhoneBase
{
  string Mobile { get; set; }
  string Telephone { get; set; }
  bool? IsMobileValid { get; set; }
  bool? IsTelephoneValid { get; set; }
}