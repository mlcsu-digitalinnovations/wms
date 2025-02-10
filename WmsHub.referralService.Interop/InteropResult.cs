namespace WmsHub.ReferralService.Interop;

public class InteropResult
{
  public byte[] Data { get; set; }
  public string ErrorText { get; set; } = "No Errors";
  public bool ExportError { get; set; } = false;
  public bool WordError { get; set; } = false;   
}