using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace WmsHub.Business.Models.AuthService
{
  public static class AuthServiceHelper
  {
    private static string _connectionString;
    private static IHttpContextAccessor _accessor;
    public static void Configure(string connectionString,
      IHttpContextAccessor accessor)
    {
      _connectionString = connectionString;
      _accessor = accessor;
    }


    public static T GetHeaderValueAs<T>(string headerName)
    {
      T returnValue = default;

      if (_accessor?.HttpContext?.Request?.Headers != null)
      {
        _accessor.HttpContext.Request.Headers
          .TryGetValue(headerName, out StringValues values);

        if (!StringValues.IsNullOrEmpty(values))
        {
          returnValue = (T)Convert.ChangeType(values.ToString(), typeof(T));
        }
      }
      return returnValue;
    }
  }
}
