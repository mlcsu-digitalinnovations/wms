using System.Net.Http;
using System.Threading.Tasks;
using WmsHub.Common.Apis.Ods.Models;

namespace WmsHub.Common.Apis.Ods
{
  public interface IOdsOrganisationService
  {
    HttpMessageHandler HttpMessageHandler { get; set; }

    Task<OdsOrganisation> GetOdsOrganisationAsync(string odsCode);
  }
}