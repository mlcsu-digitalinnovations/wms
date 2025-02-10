using System.Diagnostics;

namespace WmsHub.AzureFunctions.Services;
public interface IProcess : IDisposable
{
  StreamReader StandardOutput { get; }

  bool Start();

  ProcessStartInfo StartInfo { get; set; }

  bool WaitForExit(int milliseconds);
}
