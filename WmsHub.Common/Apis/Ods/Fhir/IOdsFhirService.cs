using System.Threading.Tasks;

namespace WmsHub.Common.Apis.Ods.Fhir;

public interface IOdsFhirService
{
  Task<bool> OrganisationCodeExistsAsync(string odsCode);
}
