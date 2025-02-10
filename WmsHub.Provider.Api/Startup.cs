using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using WmsHub.Business;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Middleware;
using WmsHub.Common.Api.Models;
using WmsHub.Provider.Api.Models;

namespace WmsHub.Provider.Api
{
  [ExcludeFromCodeCoverage]
  public class Startup
  {
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
      Configuration = configuration;
      _env = env;
    }

    private IWebHostEnvironment _env;

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. 
    // Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddAutoMapper(typeof(Startup),
        typeof(Business.Models.Profiles.ReferralProfile));

      services.AddAutoMapper(typeof(Startup),
        typeof(Business.Models.Profiles.RefreshTokenProfile));

      services.AddAutoMapper(typeof(Startup),
        typeof(Business.Models.Profiles.ServiceUserSubmissionRequestProfile));

      services.AddAutoMapper(typeof(Startup),
        typeof(Business.Models.Profiles.ReferralStatusReasonProfile));

      ApiVersionOptions apiVersionOptions = new();
      Configuration.Bind(ApiVersionOptions.SectionKey, apiVersionOptions);

      services
        .AddApiVersioning(options =>
        {
          options.AssumeDefaultVersionWhenUnspecified = true;
          options.DefaultApiVersion = new ApiVersion(
            apiVersionOptions.DefaultMajor, 
            apiVersionOptions.DefaultMinor);
          options.ReportApiVersions = true;
        })
        .AddApiExplorer(options => 
        {
          options.GroupNameFormat = "'v'VVV";
          options.SubstituteApiVersionInUrl = true;
        });

      services.AddHealthChecks();
      services.AddControllers();

      services.AddAuthentication(x =>
        {
          x.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;
          x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
       .AddJwtBearer(x =>
        {

          x.TokenValidationParameters =
            NotifyTokenHandler.GetTokenValidationParameters();
          x.Events = NotifyTokenHandler.GetEvents();
        })
       .AddApiKeyInHeaderOrQueryParams<ApiKeyProvider>("ApiKey", x =>
        {
          x.Realm = "Provider API";
          x.KeyName = "X-API-KEY";

          x.Events = new ApiKeyEvents
          {
            OnHandleChallenge = async (context) =>
            {
              // custom code to always return 403 for API Key failure
              string errorMessage = 
                "{\"detail\":\"Api key was missing or invalid.\"}";
              if (context.Response.StatusCode == StatusCodes.Status410Gone)
              {
                errorMessage = "{\"detail\":\"Api key has expired.\"}";
              }

              context.Response.StatusCode = StatusCodes.Status403Forbidden;
              context.Response.ContentType = "application/json";
              await context.Response.WriteAsync(errorMessage);
              context.Handled();
            }
          };
        });

      services.AddDbContext<DatabaseContext>
      (options =>
      {
        options.UseSqlServer(Configuration.GetConnectionString("WmsHub"),
          opt => opt.EnableRetryOnFailure());
#if DEBUG
        options.EnableSensitiveDataLogging();
#endif
        options.EnableDetailedErrors();
      });

      string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
      string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
      bool isXmlDocAvailable = File.Exists(xmlPath);

      if (_env.IsDevelopment())
      {
        services
          .AddTransient<IConfigureOptions<SwaggerGenOptions>,
            ConfigureSwaggerOptions>();

        services.AddSwaggerGen(c =>
        {
          c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
          {
            In = ParameterLocation.Header,
            Name = "X-API-KEY",
            Type = SecuritySchemeType.ApiKey
          });

          c.AddSecurityRequirement(new OpenApiSecurityRequirement
          {
            {
              new OpenApiSecurityScheme
              {
                Reference = new OpenApiReference
                {
                  Type = ReferenceType.SecurityScheme,
                  Id = "ApiKey"
                }
              },
              Array.Empty<string>()
            }
          });

          if (isXmlDocAvailable)
          {
            c.IncludeXmlComments(xmlPath);
          }
        });
      }

      services.AddHttpClient<INotificationService, NotificationService>()
        .AddPolicyHandler(HttpClientHelper.GetRetryPolicy())
        .AddPolicyHandler(HttpClientHelper.GetCircuitBreakerPolicy())
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

      services.AddOptions<ApiVersionOptions>()
       .Bind(Configuration.GetSection(ApiVersionOptions.SectionKey))
       .ValidateDataAnnotations();
      services.AddOptions<AuthOptions>()
       .Bind(Configuration.GetSection(AuthOptions.SectionKey))
       .ValidateDataAnnotations();
      services.AddOptions<NotificationOptions>()
        .Bind(Configuration.GetSection(NotificationOptions.SECTION_KEY))
        .ValidateDataAnnotations();
      services.AddOptions<ProviderOptions>()
        .Bind(Configuration.GetSection(ProviderOptions.SectionKey))
        .ValidateDataAnnotations();
      services.AddOptions<TextOptions>()
        .Bind(Configuration.GetSection(TextOptions.SectionKey))
        .ValidateDataAnnotations();

      services.AddHsts(options =>
      {
        options.MaxAge = DateTime.Now.AddYears(2) - DateTime.Now;
      });
      
      services.AddScoped<IWmsAuthService, WmsAuthService>();
      services.AddScoped<IProviderService, ProviderService>();
      services.AddScoped<INotificationService, NotificationService>();
      services.AddScoped<IRequestResponseLogService, RequestResponseLogService>();
      services.AddHttpContextAccessor();

      services.AddScoped<IApiKeyService, ApiKeyService>();

      if (_env.IsDevelopment())
      {
        services.AddFeatureManagement(
          Configuration.GetSection("DevFeatureManagement"));
      }
      else
      {
        services.AddFeatureManagement(
          Configuration.GetSection("FeatureManagement"));
      }
    }

    // This method gets called by the runtime. Use this method to configure 
    // the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, 
      IWebHostEnvironment env, 
      IApiVersionDescriptionProvider provider)
    {
      if (env.IsDevelopment())
      {
        app.UseStaticFiles();
        app.UseExceptionHandler("/error-development");
        app.UseSwagger(options =>
        {
          options.PreSerializeFilters.Add((swagger, httpReq) =>
          {
            //Clear servers -element in swagger.json because it got the wrong 
            //port when hosted behind reverse proxy
            swagger.Servers.Clear();
          });
        });
        app.UseSwaggerUI(options =>
        {
          foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
          {
            options.SwaggerEndpoint(
              $"/swagger/{description.GroupName}/swagger.json", 
              description.GroupName.ToUpperInvariant());
          }
        });
      }
      else
      {
        app.UseExceptionHandler("/error");
      }

      app.UseHsts();
      NotifyTokenHandler.Configure(
        app.ApplicationServices.GetService<IOptions<TextOptions>>(),
        Configuration.GetConnectionString("WmsHub"),
        app.ApplicationServices.GetService<IHttpContextAccessor>());

      AuthServiceHelper.Configure(Configuration.GetConnectionString("WmsHub"),
        app.ApplicationServices.GetService<IHttpContextAccessor>());

      app.UseHttpsRedirection();
      app.UseRouting();

      app.UseMiddleware<RequestResponseLogging>();

      app.UseAuthentication();
      app.UseAuthorization();      

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers().RequireAuthorization();
        endpoints.MapHealthChecks("/health");
      });
    }
  }
}
