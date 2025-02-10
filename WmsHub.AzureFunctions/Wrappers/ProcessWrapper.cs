using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions.Wrappers;

/// <summary>
/// Providers a wrapper for <see cref="Process"></see> to enable unit testing./>
/// </summary>
/// <param name="process">The <see cref="Process"></see> object to wrap.</param>
internal class ProcessWrapper(Process process) : IProcess
{
  private readonly Process _process = process ?? throw new ArgumentNullException(nameof(process));
  private bool _disposedValue;

  [ExcludeFromCodeCoverage(Justification = "Unable to unit test because Process cannot be mocked")]
  public StreamReader StandardOutput => _process.StandardOutput;

  [ExcludeFromCodeCoverage(Justification = "Unable to unit test because Process cannot be mocked")]
  public ProcessStartInfo StartInfo
  {
    get => _process.StartInfo;
    set => _process.StartInfo = value;
  }

  [ExcludeFromCodeCoverage(Justification = "Unable to unit test because Process cannot be mocked")]
  public bool Start() => _process.Start();

  [ExcludeFromCodeCoverage(Justification = "Unable to unit test because Process cannot be mocked")]
  public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);

  [ExcludeFromCodeCoverage(Justification = "Unable to unit test because Process cannot be mocked")]
  protected virtual void Dispose(bool disposing)
  {
    if (!_disposedValue)
    {
      if (disposing)
      {
        _process.Dispose();
      }

      _disposedValue = true;
    }
  }

  [ExcludeFromCodeCoverage(Justification = "Unable to unit test because Process cannot be mocked")]
  public void Dispose()
  {
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
