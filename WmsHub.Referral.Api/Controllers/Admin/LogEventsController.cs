using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using System.Collections.Generic;
using WmsHub.Common.Extensions;

namespace WmsHub.Referral.Api.Controllers.Admin
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/admin/[controller]")]
  [Route("admin/[Controller]")]
  public class LogEventsController : ControllerBase
  {
    private readonly ILogger _logger;

    public LogEventsController(ILogger logger)
    {
      _logger = logger.ForContext<LogEventsController>();
    }

    [HttpPost]
    public IActionResult Post(
      [FromBody] List<Common.Models.LogEvent> logEvents)
    {
      const string NUM_EVENTS = "{numberOfEvents}";
      int numberOfEvents = logEvents?.Count ?? 0;
      string msg = $"Received batch of {NUM_EVENTS} log events.";

      _logger.Information(msg, numberOfEvents);

      if (logEvents == null)
      {
        _logger.Error("LogEvents.Events was null.");
      }
      else if (logEvents.Count == 0)
      {
        _logger.Error("LogEvents.Events contained no log events.");
      }
      else
      {
        foreach (var logEvent in logEvents)
        {
          string msgTemp = "OriginalTimestamp:{Timestamp};" +
            "OriginalLevel:{Level};" + logEvent.RenderedMessage;

          if (!logEvent.Level
            .TryParseToEnumName(out LogEventLevel logEventLvl))
          {
            logEventLvl = LogEventLevel.Error;
          }

          switch (logEventLvl)
          {
            case LogEventLevel.Error:
              _logger.Error(msgTemp, logEvent.Timestamp, logEvent.Level);
              break;
            case LogEventLevel.Fatal:
              _logger.Fatal(msgTemp, logEvent.Timestamp, logEvent.Level);
              break;
            default:
              _logger.Warning(msgTemp, logEvent.Timestamp, logEvent.Level);
              break;
          }
        }
      }

      return Ok(string.Format(msg.Replace(NUM_EVENTS, "{0}"), numberOfEvents));
    }
  }
}