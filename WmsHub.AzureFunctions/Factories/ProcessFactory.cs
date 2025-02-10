using System.Diagnostics;
using WmsHub.AzureFunctions.Services;
using WmsHub.AzureFunctions.Wrappers;

namespace WmsHub.AzureFunctions.Factories;

/// <summary>
/// Facilitates the mocking of <see cref="Process"/> for unit tests.
/// </summary>
public class ProcessFactory : IProcessFactory
{
  /// <summary>
  /// Creates a <see cref="ProcessWrapper"/> with a new <see cref="Process"/> instance.
  /// </summary>
  /// <returns>
  /// An <see cref="IProcess"/> interface to the instantiated <see cref="ProcessWrapper"/>.
  /// </returns>
  public IProcess Create() => new ProcessWrapper(new Process());
}
