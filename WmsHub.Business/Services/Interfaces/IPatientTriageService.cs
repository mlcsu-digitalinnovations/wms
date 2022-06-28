using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Models.PatientTriage;

namespace WmsHub.Business.Services
{
  public interface IPatientTriageService: IServiceBase
  {
    CourseCompletionResult GetScores(
      CourseCompletionParameters courseCompletionParameters);
  }
}