using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models
{
  public class PostcodeOptions
  {
    public const string SectionKey = "Postcode";
    private string _postcodeIoUrl = "http://api.postcodes.io/postcodes/";

    [Required]
    public string PostcodeIoUrl
    {
      get => _postcodeIoUrl;
      set => _postcodeIoUrl = $"{(value.EndsWith("/") ? value : value + "/")}";
    }
    public DomainAccess Access => DomainAccess.PostcodeApi;
  }
}
