using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WmsHub.TextMessage.Api.Tests
{
  public class TextMessageFactory<TEntryPoint> : 
    WebApplicationFactory<TEntryPoint> where TEntryPoint : class
  {
    protected override IWebHostBuilder CreateWebHostBuilder()
    {
      return WebHost.CreateDefaultBuilder(null)
       .UseStartup<TEntryPoint>();
    }
  }
}
