using System.Threading.Tasks;
using WmsHub.ReferralService.Interop;

namespace WmsHub.ReferralsService.Interop
{
  //[ComVisible(true),
  // Guid("33ECB037-75BC-4A51-BC50-D2C53160A50F")]
  public interface IProcessor
  {
    Task<InteropResult> ConvertToPdfInteropAsync(
      byte[] byteArray, 
      string docType, 
      string saveLocation, 
      bool reformatDocument,
      string[] sectionHeadings);

    void Cleanup(string docPath, string pdfPath);
  }


}
