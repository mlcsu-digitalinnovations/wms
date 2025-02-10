//#nullable enable
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using WmsHub.Business;
//using WmsHub.Business.Entities;
//using WmsHub.Business.Enums;
//using WmsHub.Business.Helpers;
//using WmsHub.Provider.Api;
//using Xunit;

//namespace WmsHub.ProviderApi.Tests
//{

//  public class TestControllerUnitTests
//  {
//    private HttpClient _client = null!;
//    private const string _apiBase = "https://localhost:44316";
//    private const string DEV_ENV = "Development";
//    private const string NON_DEV_ENV = "Production";
//    public class DevelopmentEnvironmentTests : TestControllerUnitTests,
//      IClassFixture<CustomDevelopmentWebApplicationFactory<Startup>>
//    {
//      private readonly CustomDevelopmentWebApplicationFactory<Startup> _factory;

//      / <summary>
//      / Development Environment Tests
//      / </summary>
//      / <param name = "factory" ></ param >
//      public DevelopmentEnvironmentTests(
//        CustomDevelopmentWebApplicationFactory<Startup> factory)
//      {
//        if (_factory == null)
//        {
//          _factory = factory;
//          WebApplicationFactoryClientOptions clientOptions =
//            new WebApplicationFactoryClientOptions();
//          clientOptions.AllowAutoRedirect = true;
//          clientOptions.BaseAddress = new Uri("https://localhost:44316");
//          clientOptions.HandleCookies = true;
//          clientOptions.MaxAutomaticRedirections = 7;
//          _client = factory.CreateClient(clientOptions);
//          _client.DefaultRequestHeaders.Add(
//            "X-API-KEY", Guid.NewGuid().ToString());
//        }
//      }

//      [Fact]
//      public async Task Validate_Post_Endpoint_Exists()
//      {
//        Arrange
//        int expected = (int)HttpStatusCode.Forbidden;
//        Act
//        var response =
//          await _client.PostAsync($"{_apiBase}/Test?num=20", null!);
//        Assert
//        response.StatusCode.Should().Be(expected);
//      }

//      [Fact]
//      public async Task Validate_Delete_Endpoint_Exists()
//      {
//        Arrange
//        int expected = (int)HttpStatusCode.Forbidden;
//        Act
//        var response = await _client.DeleteAsync($"{_apiBase}/Test");
//        Assert
//        response.StatusCode.Should().Be(expected);
//      }

//    }

//    public class TestingEnvironmentTest : TestControllerUnitTests,
//      IClassFixture<CustomProductionWebApplicationFactory<Startup>>
//    {
//      private readonly CustomProductionWebApplicationFactory<Startup> _factory;

//      / <summary>
//      / Production Environment tests
//      / </summary>
//      / <param name = "factory" ></ param >
//      public TestingEnvironmentTest(
//        CustomProductionWebApplicationFactory<Startup> factory)
//      {
//        if (_factory == null)
//        {
//          _factory = factory;
//          WebApplicationFactoryClientOptions clientOptions = new();
//          clientOptions.AllowAutoRedirect = true;
//          clientOptions.BaseAddress = new Uri("https://localhost:44316");
//          clientOptions.HandleCookies = true;
//          clientOptions.MaxAutomaticRedirections = 7;
//          _client = factory.CreateClient(clientOptions);
//          var headerFound =
//            _client.DefaultRequestHeaders.Count(t => t.Key == "X-API-KEY");
//          if (headerFound == 0)
//          {
//            _client.DefaultRequestHeaders.Add("X-API-KEY",
//              Environment.GetEnvironmentVariable("WmsHub_Provider_Api_ApiKey"));
//          }
//        }
//      }

//      [Fact]
//      public async Task Validate_Post_Endpoint_Not_Exists()
//      {
//        Arrange
//        int expected = (int)HttpStatusCode.NotFound;
//        Act
//        var response =
//          await _client.PostAsync($"{_apiBase}/Test?num=20", null!);
//        Assert
//        response.StatusCode.Should().Be(expected);
//      }


//    }

//    public class CustomProductionWebApplicationFactory<TStartup>
//      : WebApplicationFactory<TStartup> where TStartup : class
//    {
//      protected override IWebHostBuilder CreateWebHostBuilder() =>
//        base.CreateWebHostBuilder().UseEnvironment(NON_DEV_ENV);

//      Add in memory dbcontext

//      protected override void ConfigureWebHost(IWebHostBuilder builder)
//      {
//        builder.ConfigureServices(services =>
//        {
//          ServiceDescriptor? descriptor = services.SingleOrDefault(
//            d => d.ServiceType ==
//                 typeof(DbContextOptions<DatabaseContext>));

//          services.Remove(descriptor!);

//          ServiceDescriptor? authDesciDescriptor = services.SingleOrDefault(
//            d => d.ServiceType == typeof(ApiKeyProvider));

//          try
//          {
//            services.AddDbContext<DatabaseContext>(options =>
//            {
//              options.UseInMemoryDatabase("wmsHub_ProviderApi");
//            });

//            ServiceProvider sp = services.BuildServiceProvider();

//            using (var scope = sp.CreateScope())
//            {
//              IServiceProvider scopedServices = scope.ServiceProvider;
//              DatabaseContext db =
//                scopedServices.GetRequiredService<DatabaseContext>();


//              db.Database.EnsureCreated();

//              try
//              {
//                DbGenerator.Initialise(db);
//              }
//              catch (Exception)
//              {

//              }
//            }
//          }
//          catch
//          {
//            Do nothing
//          }
//        });
//      }
//    }

//    public class CustomDevelopmentWebApplicationFactory<TStartup>
//      : WebApplicationFactory<TStartup> where TStartup : class
//    {
//      protected override IWebHostBuilder CreateWebHostBuilder() =>
//        base.CreateWebHostBuilder().UseEnvironment(DEV_ENV);

//      Add in memory dbcontext

//      protected override void ConfigureWebHost(IWebHostBuilder builder)
//      {
//        builder.ConfigureServices(services =>
//        {
//          ServiceDescriptor? descriptor = services.SingleOrDefault(
//            d => d.ServiceType ==
//                 typeof(DbContextOptions<DatabaseContext>));

//          try
//          {
//            services.Remove(descriptor!);

//            services.AddDbContext<DatabaseContext>(options =>
//            {
//              options.UseInMemoryDatabase("wmsHub_ProviderApi");
//            });
//          }
//          finally { }

//          ServiceProvider sp = services.BuildServiceProvider();

//          using (var scope = sp.CreateScope())
//          {
//            IServiceProvider scopedServices = scope.ServiceProvider;
//            DatabaseContext db =
//              scopedServices.GetRequiredService<DatabaseContext>();


//            db.Database.EnsureCreated();

//            try
//            {
//              DbGenerator.Initialise(db);
//            }
//            catch (Exception)
//            {

//            }
//          }
//        });
//      }
//    }
//    public class BaseCommon
//    {
//      public const string TEST_USER_ID = "571342f1-c67d-49bf-a9c6-40a41e6dc702";
//      public const string _ubrn = "120000000001";
//      public const string toNumber = "+447512751212";
//      public DatabaseContext _ctx = new DatabaseContext(
//        new DbContextOptionsBuilder<DatabaseContext>()
//          .UseInMemoryDatabase(databaseName: "wmsHub_CallbackApi")
//          .Options);
//    }
//    public class DbGenerator : BaseCommon
//    {
//      public static void Initialise(DatabaseContext ctx)
//      {
//        Guid providerId = Guid.Parse(TEST_USER_ID);
//        Business.Entities.Provider? provider = ctx.Providers
//          .SingleOrDefault(t => t.Id == providerId);
//        if (provider == null)
//        {
//          Add a provider
//          provider = new Business.Entities.Provider
//          {
//            Id = providerId,
//            IsActive = true,
//            Level1 = true,
//            Logo = "",
//            Name = "Test",
//            Summary = "Test",
//            Website = "Test"
//          };
//          ctx.Providers.Add(provider);

//          ctx.SaveChanges();
//        }

//        Add a service user
//        Referral? referral = ctx.Referrals
//          .Include(t => t.TextMessages)
//          .FirstOrDefault();
//        if (referral == null)
//        {
//          referral = RandomEntityCreator.CreateRandomReferral(
//            dateOfProviderSelection: DateTimeOffset.Now,
//            modifiedByUserId: provider.Id,
//            providerId: provider.Id,
//            referringGpPracticeName: provider.Id.ToString(),
//            status: ReferralStatus.TextMessage1,
//            triagedCompletionLevel: "1",
//            ubrn: _ubrn);

//          ctx.Referrals.Add(referral);

//          ctx.SaveChanges();
//        }

//        Business.Entities.TextMessage tm = new Business.Entities.TextMessage
//        {
//          IsActive = true,
//          ModifiedAt = DateTimeOffset.Now,
//          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
//          Number = toNumber
//        };

//        if (referral.TextMessages == null)
//          referral.TextMessages = new List<Business.Entities.TextMessage>();

//        referral.TextMessages.Add(tm);

//        ctx.SaveChanges();

//      }
//    }

//  }

//}
