using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Newtonsoft.Json;
using WmsHub.Business.Models;
using WmsHub.Business.Services;

namespace WmsHub.Common.Api.Middleware;

public class RequestResponseLogging
{
  private readonly RecyclableMemoryStreamManager _memoryStreamManager;
  private readonly RequestDelegate _next;

  public RequestResponseLogging(RequestDelegate next)
  {
    _next = next;
    _memoryStreamManager = new RecyclableMemoryStreamManager();
  }

  public async Task Invoke(
    HttpContext httpContext,
    IRequestResponseLogService requestResponseLogService)
  {
    httpContext.Request.RouteValues.TryGetValue("action", out object action);
    httpContext.Request.RouteValues
      .TryGetValue("controller", out object controller);

    if (controller == null && action == null)
    {
      await _next(httpContext);
    }
    else
    {
      string actionName = action == null ? "Unknown" : action.ToString();
      string controlName = controller == null 
        ? "Unknown" 
        : controller.ToString();

      // Ignore the Azure Front Door health probe
      if (actionName == "HealthProbe" && controlName == "HealthProbe")
      {
        await _next(httpContext);
      }
      else
      { 
        string bodyAsText = await GetBodyAsText(httpContext.Request);

        string queryString = httpContext.Request.QueryString.HasValue
          ? "?"
          : string.Empty;

        string request = $"{httpContext.Request.Method} " +
          $"{httpContext.Request.Scheme}://{httpContext.Request.Host}" + 
          $"{httpContext.Request.Path}" + 
          $"{queryString} {httpContext.Request.QueryString} {bodyAsText}";

        DateTimeOffset requestAt = DateTimeOffset.Now;

        try
        {
          await _next(httpContext);
        }
        catch (Exception ex)
        {
          if (ex is OptionsValidationException 
            || ex is ValidationException)
          {
            HttpResponse response = httpContext.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            string[] messageDetails = ex.Message.Split(';');

            List<ValidationError> errors = new ();
            for (int i = 0; i < messageDetails.Length; i++)
            {
              ValidationError error = new ();
              string[] innerDetails = messageDetails[i]
                .Replace("\r\n", "")
                .Replace("''","")
                .Replace("\"","")
                .Trim()
                .Split(":");

              error.Title = innerDetails[0];
              for (int j = 1;  j < innerDetails.Length; j++)
              {
                error.Errors.Add(innerDetails[j].Trim());
              }
              errors.Add(error);
            }

            string json = JsonConvert.SerializeObject(
              errors, 
              Formatting.Indented);
            await response.WriteAsJsonAsync(errors);
          }

          await _next(httpContext);
        }

        IRequestResponseLog requestResponseLog = new RequestResponseLog()
        {
          Action = actionName,
          Controller = controlName,
          Request = request,
          RequestAt = requestAt,
          Response = $"{httpContext.Response.StatusCode}",
          ResponseAt = DateTimeOffset.Now,
          UserId = null
        };

        if (Guid.TryParse(
          httpContext.User.FindFirst(ClaimTypes.Sid)?.Value, out Guid userId))
        {
          requestResponseLog.UserId = userId;
        }

        await requestResponseLogService.CreateAsync(requestResponseLog);
      }
    }
  }

  private async Task<string> GetBodyAsText(HttpRequest request)
  {
    request.EnableBuffering();
    await using MemoryStream requestStream = _memoryStreamManager.GetStream();
    await request.Body.CopyToAsync(requestStream);
    string bodyAsText = ReadStreamInChunks(requestStream);
    request.Body.Position = 0;
    return bodyAsText;
  }

  private static string ReadStreamInChunks(MemoryStream stream)
  {
    const int readChunkBufferLength = 4096;
    stream.Seek(0, SeekOrigin.Begin);
    using StringWriter textWriter = new StringWriter();
    using StreamReader reader = new StreamReader(stream);

    char[] readChunk = new char[readChunkBufferLength];
    int readChunkLength;
    do
    {
      readChunkLength = reader
        .ReadBlock(readChunk, 0,readChunkBufferLength);
      textWriter.Write(readChunk, 0, readChunkLength);
    } while (readChunkLength > 0);

    return textWriter.ToString();
  }
}