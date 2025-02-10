using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Ui.Middleware;
using WmsHub.Ui.Models;

namespace WmsHub.Ui;

public class Startup
{
  private const StringComparison IGNORE_CASE =
    StringComparison.InvariantCultureIgnoreCase;
  private const string COOKIES = ".AspNetCore.Cookies";
  private const string CORRELATION_COOKIE = ".AspNetCore.Correlation";
  private const string NONCE_COOKIE = ".AspNetCore.OpenIdConnect.Nonce";

  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;
  }

  public IConfiguration Configuration { get; }

  // This method gets called by the runtime. Use this method to add services
  // to the container.
  public void ConfigureServices(IServiceCollection services)
  {
    // *** AspNetCoreRateLimit --
    // https://github.com/stefanprodan/AspNetCoreRateLimit
    services.AddOptions();

    // needed to store rate limit counters and ip rules
    services.AddMemoryCache();

    //load general configuration from appsettings.json
    services.Configure<IpRateLimitOptions>(
        Configuration.GetSection("IpRateLimiting"));

    //load ip rules from appsettings.json
    services.Configure<IpRateLimitPolicies>(
        Configuration.GetSection("IpRateLimitPolicies"));

    services.Configure<WebUiSettings>(
      Configuration.GetSection(WebUiSettings.SectionKey));

    // inject counter and rules stores
    services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    services
      .AddSingleton<IRateLimitCounterStore, 
      MemoryCacheRateLimitCounterStore>();
    services
      .AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    // *** AspNetCoreRateLimit

    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
      .AddMicrosoftIdentityWebApp(options =>
      {

        options.ClientId = Configuration["AzureAd:ClientId"];
        options.TenantId = Configuration["AzureAd:TenantId"];
        options.Domain = Configuration["AzureAd:Domain"];
        options.Instance = Configuration["AzureAd:Instance"];
        options.CallbackPath = Configuration["AzureAd:CallbackPath"];

        options.Events = new OpenIdConnectEvents()
        {
          OnRedirectToIdentityProvider = context =>
          {
            context.ProtocolMessage.RedirectUri =
              $"{Configuration["AzureFrontDoor_Uri"]}{options.CallbackPath}";
            return Task.FromResult(0);
          }
        };
      });

    services.AddDistributedMemoryCache();

    services.AddHttpContextAccessor();

    services.AddAutoMapper(new[] {
      typeof(Startup),
      typeof(Models.Profiles.ProviderProfile),
      typeof(Models.Profiles.ReferralProfile),
      typeof(Models.Profiles.ReferralAuditProfile),
      typeof(Models.Profiles.PatientTriageProfile),
      typeof(Models.Profiles.UserStoreProfile),
      typeof(Models.Profiles.ReferralStatusReasonProfile)
    });

    services.AddDbContext<DatabaseContext>
    (options =>
    {
      options.UseSqlServer(Configuration.GetConnectionString("WmsHub"),
        opt =>
        {
          opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
          opt.EnableRetryOnFailure();
        });
#if DEBUG
      options.EnableSensitiveDataLogging();
#endif
      options.EnableDetailedErrors();
      options.ConfigureWarnings(w =>
        w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
    });

    services.AddHttpClient<INotificationService, NotificationService>()
      .AddPolicyHandler(HttpClientHelper.GetRetryPolicy())
      .AddPolicyHandler(HttpClientHelper.GetCircuitBreakerPolicy())
      .SetHandlerLifetime(TimeSpan.FromMinutes(5));

    services.AddOptions<NotificationOptions>()
      .Bind(Configuration.GetSection(NotificationOptions.SECTION_KEY))
      .ValidateDataAnnotations();

    services.AddScoped<ILinkIdService, LinkIdService>();
    services.AddScoped<IProviderService, ProviderService>();
    services.AddScoped<ITextNotificationHelper, TextNotificationHelper>();
    services.AddScoped<INotificationService, NotificationService>();
    services.AddScoped<IReferralService, ReferralService>();
    services.AddScoped<IEthnicityService, EthnicityService>();
    services.AddScoped<ICsvExportService, CsvExportService>();
    services.AddScoped<IPatientTriageService, PatientTriageService>();
    services.AddSingleton<IConfiguration>(Configuration);
    services.AddTransient<IClaimsTransformation, 
      RmcUserClaimsTransformation>();
    services.AddScoped<IUserActionLogService, UserActionLogService>();

    // Additional requirement for AspNetCoreRateLimit
    // configuration (resolvers, counter key builders)
    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    services.AddSingleton<IProcessingStrategy, 
      AsyncKeyLockProcessingStrategy>();

    services.AddSession(options =>
    {
      options.IdleTimeout = TimeSpan.FromSeconds(600);
      options.Cookie.HttpOnly = true;
      options.Cookie.Name = ".Wms.Session";
      options.Cookie.IsEssential = true;
    });

    services.AddDistributedSqlServerCache(options =>
    {
      options.ConnectionString = Configuration.GetConnectionString("WmsHub");
      options.SchemaName = "dbo";
      options.TableName = "ServiceUserUiSessionCache";
    });

    services.AddHealthChecks();
    services.AddControllersWithViews(options =>
    {
      var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
      options.Filters.Add(new AuthorizeFilter(policy));
    });
    
    services.AddHsts(options =>
    {
      options.MaxAge = DateTime.Now.AddYears(2) - DateTime.Now;
    });
    
    services.AddRazorPages(options =>
    {
      options.Conventions.AllowAnonymousToFolder("/ServiceUser");
    })
      .AddMicrosoftIdentityUI()
    .AddRazorRuntimeCompilation();

    services.AddAuthorization(options =>
    {
      options.AddPolicy(
        "RmcUiDomainUsers",
        policyBuilder => policyBuilder.RequireClaim(
          RmcUserClaimsTransformation.REQUIRED_CLAIM,
          RmcUserClaimsTransformation.DOMAIN_RMC_UI));
    });

    //stop auto compile of cshtml during dev
    //services.AddControllersWithViews().AddRazorRuntimeCompilation();
  }

  // This method gets called by the runtime. Use this method to configure
  // the HTTP request pipeline.
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    app.UseCookiePolicy(
      new CookiePolicyOptions
      {
        MinimumSameSitePolicy = SameSiteMode.Strict,
        Secure = CookieSecurePolicy.Always,
        HttpOnly = HttpOnlyPolicy.Always,
        OnAppendCookie = (AppendCookieContext options) => {
          if (options.CookieName.StartsWith(CORRELATION_COOKIE, IGNORE_CASE)
            || options.CookieName.StartsWith(NONCE_COOKIE, IGNORE_CASE))
          {
            options.CookieOptions.SameSite = SameSiteMode.None;
          }
          else if (options.CookieName.StartsWith(COOKIES, IGNORE_CASE))
          {
            options.CookieOptions.SameSite = SameSiteMode.Lax;
          }
        }
      });

    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }
    else
    {
      app.UseExceptionHandler("/Home/Error");
    }
    // The default HSTS value is 30 days. You may want to change this for
    // production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    // Additional requirement for AspNetCoreRateLimit
    app.UseMiddleware<ServiceUserRateLimit>();
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.Use(async (context, next) =>
    {
      await next();
      if (context.Response.StatusCode == 404)
      {
        context.Request.Path = "/";
        await next();
      }
    });

    AuthServiceHelper.Configure(Configuration.GetConnectionString("WmsHub"),
      app.ApplicationServices.GetService<IHttpContextAccessor>());

    app.UseRouting();

    app.UseSession();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseServiceUserInterceptor();
    app.UseMiddleware<UserActionLogging>();
    app.UseSecurityHeaders(
      Configuration,
      env.IsDevelopment(), 
      Configuration.GetSection("SignalR_Endpoint").Value);

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
      endpoints.MapControllerRoute(
        name: "welcome",
        pattern: "{controller=ServiceUser}/{action=Welcome}/{textId}");
      endpoints.MapRazorPages();
      endpoints.MapHealthChecks("/health");
    });
  }
}
