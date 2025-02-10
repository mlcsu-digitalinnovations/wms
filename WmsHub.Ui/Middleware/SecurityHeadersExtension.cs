using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WmsHub.Ui.Models;

namespace WmsHub.Ui.Middleware;

public static class SecurityHeadersExtension
{
  private const string CONST_CACHE_CONTROL = "Cache-Control";
  private const string CACHE_CONTROL_SETTING = "no-cache";
  private const string CONST_CONTENT_SECURITY_POLICY =
    "Content-Security-Policy";
  private const string CONST_X_XSS_PROTECTION =
    "X-XSS-Protection";

  public static IApplicationBuilder UseSecurityHeaders(
    this IApplicationBuilder app,
    IConfiguration configuration,
    bool isDevelopment,
    string signalRUrl)
  {
    IConfigurationSection settings = 
      configuration.GetSection(WebUiSettings.SectionKey);
    
    string connectSrc = 
      $"connect-src 'self' {signalRUrl.Replace("https", "wss")}";
    string childSrc = "child-src 'unsafe-inline'";
    string frameSrc = "frame-src 'unsafe-inline'";
    string scriptSrc = settings.GetValue<string>("SecurityHeaderScriptSrc");
    string styleSrc = settings.GetValue<string>("SecurityHeaderStyleSrc");

    if (isDevelopment)
    {
      string developmentConnectSrc = 
        settings.GetValue<string>("DevelopmentConnectSrc");
      connectSrc = $"{connectSrc} {developmentConnectSrc}";
    }

    app.Use(async (context, next) =>
    {
      if (!context.Response.Headers.ContainsKey(CONST_CONTENT_SECURITY_POLICY))
      {
        context.Response.Headers.Append(
          CONST_CONTENT_SECURITY_POLICY,
          $"{connectSrc}; {scriptSrc}; {styleSrc}; {childSrc}; {frameSrc};");
      }

      if (!context.Response.Headers.ContainsKey(CONST_X_XSS_PROTECTION))
      {
        context.Response.Headers.Append(
          CONST_X_XSS_PROTECTION,
          "1");
      }

      if (!context.Response.Headers.ContainsKey(CONST_CACHE_CONTROL))
      {
        context.Response.Headers.Append(
          CONST_CACHE_CONTROL,
          CACHE_CONTROL_SETTING);
      }

      await next();
    });

    return app;
  }
}
