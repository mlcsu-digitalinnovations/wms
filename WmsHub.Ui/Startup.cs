using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
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
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Services;
using WmsHub.Ui.Middleware;
using WmsHub.Ui.Models;

namespace WmsHub.Ui
{
  public class Startup
  {
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
        Configuration.GetSection("RmcUi"));

      // inject counter and rules stores
      services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
      services
        .AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
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
            },
            OnTokenValidated = async context =>
            {
              var c = context;
            }
          };
        });

      services.AddDistributedMemoryCache();

      services.AddHttpContextAccessor();

      services.AddAutoMapper(new[] {
        typeof(Startup),
        typeof(Models.Profiles.ReferralProfile),
        typeof(Models.Profiles.ReferralAuditProfile),
        typeof(Models.Profiles.PatientTriageProfile),
        typeof(Models.Profiles.UserStoreProfile)
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

      services.AddScoped<IProviderService, ProviderService>();
      services.AddScoped<IReferralService, ReferralService>();
      services.AddScoped<IEthnicityService, EthnicityService>();
      services.AddScoped<ICsvExportService, CsvExportService>();
      services.AddScoped<IPatientTriageService, PatientTriageService>();
      services.AddSingleton<IConfiguration>(Configuration);
      services.AddTransient<IClaimsTransformation, RmcUserClaimsTransformation>();
      services.AddScoped<IUserActionLogService, UserActionLogService>();

      // Additional requirement for AspNetCoreRateLimit
      // configuration (resolvers, counter key builders)
      services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
      services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

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
          Secure = CookieSecurePolicy.Always
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
        context.Response.Headers.Add("X-Xss-Protection", "1");

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

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
          name: "default",
          pattern: "{controller=Home}/{action=Index}/{id?}");
        endpoints.MapControllerRoute(
          name: "welcome",
          pattern: "{controller=ServiceUser}/{action=Welcome}/{textId}");
        endpoints.MapRazorPages();
      });
    }
  }
}
