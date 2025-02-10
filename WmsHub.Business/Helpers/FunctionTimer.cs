using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Helpers;

public class FunctionTimer: IDisposable
{
  private readonly ILogger _log;
  private readonly string _processName;
  private readonly Stopwatch _stopwatch;
  private int _time;

  /// <summary>
  /// The function of this class is to provide a timer method to provide an
  /// insight into where a process is taking longer than an expected amount.
  /// </summary>
  /// <param name="processName">The name or details of the process being
  /// investigated. <br />
  /// For example: UpdateReferralCancelledByEReferralAsync(ubrn)</param>
  /// <param name="logger">Serilog from calling method.</param>
  /// <param name="time">Expected maximum time in seconds. Default is 30 
  /// seconds with a 10 second minimum.</param>
  public FunctionTimer(string processName, ILogger logger, int time = 30)
  {
    if (string.IsNullOrWhiteSpace(processName))
    {
      throw new ArgumentNullException(nameof(processName));
    }

    if (time < 10)
    {
      time = 10;
    }
    _log = logger;
    _processName = processName;
    _stopwatch = Stopwatch.StartNew();
    _time = time;
  }

  public void Dispose()
  {
    _stopwatch.Stop();
    if (_stopwatch.Elapsed.TotalSeconds > _time)
    {
      _log.Warning("Process {processName} took {timer} seconds.", 
        _processName, 
        _stopwatch.Elapsed.TotalSeconds);
    }
  }
}
