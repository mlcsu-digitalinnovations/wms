#nullable enable
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WmsHub.Business;

namespace WmsHub.TextMessage.Api.Tests
{
  public class CustomWebApplicationFactory<TStartup>
     : WebApplicationFactory<TStartup> where TStartup : class
  {
    public DbContextOptions<DatabaseContext> Options { get; private set; } =
      null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.ConfigureServices(services =>
      {
        ServiceDescriptor? descriptor = services.SingleOrDefault(
          d => d.ServiceType == 
            typeof(DbContextOptions<DatabaseContext>));

        if (descriptor != null)
        {

          services.Remove(descriptor!);

          services.AddDbContext<DatabaseContext>(options =>
          {
            options.UseInMemoryDatabase("wmsHub_CallbackApi");
          });

        }

        ServiceProvider sp = services.BuildServiceProvider();

        using (var scope = sp.CreateScope())
        {
          IServiceProvider scopedServices = scope.ServiceProvider;
          DatabaseContext db = 
            scopedServices.GetRequiredService<DatabaseContext>();


          db.Database.EnsureCreated();

          try
          {
            DbGenerator.Initialise(db);
          }
          catch (Exception)
          {

          }
        }
      });
    }
  }

}