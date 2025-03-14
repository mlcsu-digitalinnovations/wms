using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using System.Net;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Services;
using System.Dynamic;
using WmsHub.Common.Api.Models;

namespace WmsHub.Common.Api.Controllers;

public class BaseController : ControllerBase
{
  const string HEADER_X_AZURE_SOCKETIP = "X-Azure-SocketIP";
  protected readonly IServiceBase _service;

  protected BaseController(IServiceBase service)
  {
    _service = service;
  }

  /// <summary>
  /// Produces 401
  /// </summary>
  /// <param name="response">object</param>
  /// <returns></returns>
  protected internal static IActionResult BaseBadRequestObjectResult(
    object response) => new BadRequestObjectResult(response);

  /// <summary>
  /// Produces 201
  /// </summary>
  /// <param name="response">object</param>
  /// <returns>IActionResult</returns>
  protected internal static IActionResult BaseCreatedAtRouteActionResult(
    object response, string route) =>
    new CreatedAtRouteResult("default", (id: 1, area: route), response);

  /// <summary>
  /// produces 404
  /// </summary>
  /// <returns>IActionResult</returns>
  protected internal static IActionResult BaseNotFoundActionResult() =>
    new NotFoundResult();

  protected internal static IActionResult BaseNotAuthorisedObjectResult(
    string errorMessage) => new UnauthorizedObjectResult(errorMessage);


  protected internal static IActionResult
    BasOkObjectResult(object response) => new OkObjectResult(response);

  protected internal virtual IActionResult BaseReturnResponse(
    StatusType status,
    object response,
    string errorMessage = "",
    string route = "")
  {

    return status switch
    {
      StatusType.NotAuthorised => BaseNotAuthorisedObjectResult(errorMessage),
      StatusType.CallIdDoesNotExist => BaseNotFoundActionResult(),
      StatusType.OutcomeIsUnknown or
      StatusType.TelephoneNumberMismatch or
      StatusType.ProviderUpdateFailed or
      StatusType.Invalid => BaseBadRequestObjectResult(
        Problem(errorMessage,
                statusCode: (int)HttpStatusCode.BadRequest)),
      StatusType.ProviderNotFound or
      StatusType.UnableToFindReferral => BaseBadRequestObjectResult(
        Problem(errorMessage,
                statusCode: (int)HttpStatusCode.NotFound)),
      StatusType.NoRowsUpdated => BaseBadRequestObjectResult(
        Problem(errorMessage,
                statusCode: (int)HttpStatusCode.InternalServerError)),
      StatusType.StatusIsUnknown => BaseBadRequestObjectResult(
        Problem(errorMessage,
                statusCode: (int)HttpStatusCode.NotModified)),
      StatusType.NoRowsReturned => BasOkObjectResult(response),
      StatusType.Created => BaseCreatedAtRouteActionResult(response, route),
      StatusType.Valid => BasOkObjectResult(response),
      _ => BaseNotFoundActionResult(),
    };
  }

  protected virtual void LogException(Exception ex)
  {
    LogException(ex, null);
  }

  protected virtual void LogException(Exception ex, object objToDestructure)
  {
    if (Log.IsEnabled(LogEventLevel.Debug) && objToDestructure != null)
      Log.Error(ex, "{Message} {@object}", ex.Message, objToDestructure);
    else
      Log.Error(ex, "{Message}", ex.Message);
  }

  protected virtual void LogInformation(string message)
  {
    Log.Information(message);
  }

  protected virtual void LogWarning(string message)
  {
    Log.Warning(message);
  }

  protected internal virtual IActionResult BaseBadRequestObjectResult(
   string message)
  {
    BadRequestObjectResult result = new BadRequestObjectResult(new
    {
      message = message,
      currentDate = DateTime.Now
    });
    return result;
  }

  protected static ValidationProblemDetails BaseProblemDetails(
    HttpStatusCode statusCode, string title, string[] errors)
  {
    var problemDetails = new ValidationProblemDetails
    {
      Status = (int)statusCode,
      Title = title
    };
    problemDetails.Errors.Add($"[0]", errors);

    return problemDetails;
  }

  protected virtual string GetAzureSocketIp()
  {
    StringValues values;
    string xAzureSocketIps = default;

    if (HttpContext?.Request?.Headers != null)
    {
      if (HttpContext.Request.Headers
        .TryGetValue(HEADER_X_AZURE_SOCKETIP, out values))
      {
        if (!string.IsNullOrEmpty(values.ToString()))
        {
          xAzureSocketIps = values.ToString();
        }
      }
    }

    if (xAzureSocketIps == default)
    {
      LogWarning($"{HEADER_X_AZURE_SOCKETIP}: Not present.");
    }
    else
    {
      Log.Debug($"{HEADER_X_AZURE_SOCKETIP}: {xAzureSocketIps}.");
    }

    return xAzureSocketIps;
  }

  protected void CheckAzureSocketIpAddressInWhitelist(
    string[] traceIpWhitelist)
  {
    if (traceIpWhitelist is null)
    {
      throw new ArgumentNullException(nameof(traceIpWhitelist));
    }

    string azureSocketIp = GetAzureSocketIp();

    if (azureSocketIp == null ||
      !traceIpWhitelist.Any(t => azureSocketIp.Contains(t)))
    {
      throw new UnauthorizedAccessException(
        $"{HEADER_X_AZURE_SOCKETIP}: '{azureSocketIp}' not in " +
        $"whitelist '{string.Join(", ", traceIpWhitelist)}'.");
    }
  }

  /// <summary>
  /// Returns a DateRange object. The offset is only used if 
  /// either or both of the fromDate and toDate parameters are null.
  /// </summary>
  /// <param name="fromDate">The from date for the range returned.</param>
  /// <param name="toDate">The to date for the range returned.</param>
  /// <param name="offset">The number of days after the fromDate the To 
  /// property will be if the toDate paramter is null, or the number of days
  /// before the toDate parameter the From property will be if the fromDate
  /// parameter is null.</param>
  /// <returns></returns>
  /// <exception cref="DateRangeNotValidException"></exception>
  protected static DateRange GetDateRange(
    DateTimeOffset? fromDate,
    DateTimeOffset? toDate,
    int offset = 31)
  {
    if (fromDate.HasValue && toDate.HasValue)
    {
      if (fromDate > toDate)
      {
        throw new DateRangeNotValidException(
         $"'from' date {fromDate} cannot be later than 'to' date {toDate}.");
      }
    }
    else if (fromDate.HasValue && !toDate.HasValue)
    {
      toDate = fromDate.Value.AddDays(offset);
    }
    else if (!fromDate.HasValue && toDate.HasValue)
    {
      fromDate = toDate.Value.AddDays(-offset);
    }
    else
    {
      toDate = DateTimeOffset.Now;
      fromDate = toDate.Value.AddDays(-offset);
    }

    DateRange dateRange = new(fromDate.Value, toDate.Value);
    return dateRange;
  }

  protected void ValidateDate(ref DateTimeOffset? fromDate, 
    ref DateTimeOffset? toDate)
  {
    if (fromDate.HasValue && toDate.HasValue)
    {
      if (fromDate > toDate)
      {
        throw new DateRangeNotValidException(
          $"'from' date {fromDate} cannot be later than 'to' date {toDate}.");
      }
    }
    else if (fromDate.HasValue && !toDate.HasValue)
    {
      toDate = fromDate.Value.AddDays(31);
    }
    else if (!fromDate.HasValue && toDate.HasValue)
    {
      fromDate = toDate.Value.AddDays(-31);
    }
    else
    {
      toDate = DateTimeOffset.Now;
      fromDate = toDate.Value.AddDays(-31);
    }
  }
}