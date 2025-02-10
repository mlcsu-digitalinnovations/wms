using Microsoft.Extensions.Configuration;
using WmsHub.Business;

namespace WmsHub.Utilities.Seeds
{
  public class SeederBaseBase
  {
    public static IConfiguration Config { get; set; }
    public static DatabaseContext DatabaseContext { get; set; }
  }
}