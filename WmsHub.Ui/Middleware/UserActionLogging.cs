using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using System;
using System.IO;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Services;
using WmsHub.Common.Extensions;

namespace WmsHub.Ui
{
  public class UserActionLogging
  {
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;
    private readonly RequestDelegate _next;

    public UserActionLogging(RequestDelegate next)
    {
      _next = next;
      _memoryStreamManager = new RecyclableMemoryStreamManager();
    }

    public async Task Invoke(
      HttpContext httpContext,
      IUserActionLogService userActionLogService)
    {
      httpContext.Request.RouteValues
        .TryGetValue("action", out object action);
      httpContext.Request.RouteValues
        .TryGetValue("controller", out object controller);

      string controlName = controller == null
        ? "Unknown"
        : controller.ToString();

      // only interested in the RMC controller
      if (controlName
        .Equals("Rmc", StringComparison.InvariantCultureIgnoreCase))
      {
        string actionName = action == null
          ? "Unknown"
          : action.ToString();

        string ipAddress = AuthServiceHelper
          .GetHeaderValueAs<string>("X-Azure-SocketIP");
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
          ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        string bodyAsText = await GetBodyAsText(httpContext.Request);

        string request =
          $"{httpContext.Request.Scheme}://{httpContext.Request.Host}" +
          $"{httpContext.Request.Path}" +
          $"{httpContext.Request.QueryString}" +
          (string.IsNullOrWhiteSpace(bodyAsText)
            ? ""
            : $"|{bodyAsText}");

        DateTimeOffset requestAt = DateTimeOffset.Now;

        UserActionLog userActionLog = new()
        {
          Action = actionName,
          Controller = controlName,
          IpAddress = ipAddress,
          Method = httpContext?.Request?.Method,
          Request = request,
          RequestAt = requestAt,
          UserId = httpContext?.User?.GetUserId() ?? Guid.Empty
        };

        await userActionLogService.CreateAsync(userActionLog);        
      }

      await _next(httpContext);
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
          .ReadBlock(readChunk, 0, readChunkBufferLength);
        textWriter.Write(readChunk, 0, readChunkLength);
      } while (readChunkLength > 0);

      return textWriter.ToString();
    }
  }
}