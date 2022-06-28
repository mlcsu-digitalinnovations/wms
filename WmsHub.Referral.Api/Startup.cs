using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;
using WmsHub.Business;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Profiles;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Middleware;
using WmsHub.Common.Api.Swagger;
using WmsHub.Common.Apis.Ods;
using WmsHub.Referral.Api.AuthPolicies;
using WmsHub.Referral.Api.Models;
using WmsHub.Referral.Api.Models.MskReferral;
using WmsHub.Referral.Api.Models.Profiles;

namespace WmsHub.Referral.Api
{
  public class Startup
  {
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
      Configuration = configuration;
      _env = env;
    }

    private IWebHostEnvironment _env;
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
      var apiVersionOptions = new Common.Api.Models.ApiVersionOptions();
      Configuration.Bind("ApiVersion", apiVersionOptions);

      services.AddApiVersioning(options =>
      {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(
          apiVersionOptions.DefaultMajor, apiVersionOptions.DefaultMinor);
        options.ReportApiVersions = true;
      });

      services.AddVersionedApiExplorer(options =>
      {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
      });

      services.AddAutoMapper(new[] {
        typeof(Startup),
        typeof(ReferralProfile),
        typeof(ReferralAuditProfile),
        typeof(PatientTriageProfile),
        typeof(PatientTriageRequestProfile),
        typeof(CourseCompletionResponseProfile),
        typeof(CRiUpdateRequestProfile),
        typeof(PharmacyReferralProfile)
      });
      services.AddControllers();

      services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
        .AddApiKeyInHeaderOrQueryParams<ApiKeyProvider>(options =>
        {
          options.Realm = "Referral API";
          options.KeyName = "X-API-KEY";
        });

      services.AddDbContext<DatabaseContext>
      (options =>
      {
        // This is accessible by convention from the environmental variable
        // SQLCONNSTR_WmsHub
        options.UseSqlServer(Configuration.GetConnectionString("WmsHub"),
          opt => opt.EnableRetryOnFailure());
#if DEBUG
        options.EnableSensitiveDataLogging();
#endif
        options.EnableDetailedErrors();
      });

      string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
      string filePath = Path.Combine(AppContext.BaseDirectory, xmlFile);
      bool isXmlDocAvailable = File.Exists(filePath);

      if (_env.IsDevelopment())
      {
        services.AddTransient<
          IConfigureOptions<SwaggerGenOptions>, 
          ConfigureSwaggerOptions>();

        services.AddSwaggerGen(options =>
        {
          // required because there are two PostRequest class in different
          // namespaces
          // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#customize-schema-ids
          options.CustomSchemaIds((type) => type.FullName);
          options.DocumentFilter<RemoveDefaultApiVersionRouteDocumentFilter>();

          options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
          {
            In = ParameterLocation.Header,
            Name = "X-API-KEY",
            Type = SecuritySchemeType.ApiKey
          });

          options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

          if (isXmlDocAvailable) options.IncludeXmlComments(filePath);
        });
      }

      services.AddHsts(options =>
      {
        options.MaxAge = DateTime.Now.AddYears(2) - DateTime.Now;
      });

      services.AddOptions<DeprivationOptions>()
        .Bind(Configuration.GetSection(DeprivationOptions.SectionKey))
        .ValidateDataAnnotations();

      services.AddOptions<PostcodeOptions>()
        .Bind(Configuration.GetSection(PostcodeOptions.SectionKey))
        .ValidateDataAnnotations();

      services.AddOptions<ProviderOptions>()
        .Bind(Configuration.GetSection(ProviderOptions.SectionKey))
        .ValidateDataAnnotations();

      services.AddOptions<PharmacyReferralOptions>()
        .Bind(Configuration.GetSection(
          PharmacyReferralOptions.OptionsSectionKey))
        .ValidateDataAnnotations();

      services.AddOptions<MskReferralOptions>()
        .Bind(Configuration.GetSection(MskReferralOptions.SectionKey))
        .ValidateDataAnnotations();

      services.AddOptions<OdsOrganisationOptions>()
        .Bind(Configuration.GetSection(OdsOrganisationOptions.SectionKey))
        .ValidateDataAnnotations();

      services.AddScoped<IDeprivationService, DeprivationService>();
      services.AddScoped<IPostcodeService, PostcodeService>();
      services.AddScoped<IPracticeService, PracticeService>();
      services.AddScoped<IPharmacyService, PharmacyService>();
      services.AddScoped<IProviderService, ProviderService>();
      services.AddScoped<IReferralService, ReferralService>();
      services.AddScoped<IReferralAdminService, ReferralAdminService>();
      services.AddScoped<IReferralDischargeService, ReferralDischargeService>();
      services
        .AddScoped<IRequestResponseLogService, RequestResponseLogService>();
      services.AddScoped<IPatientTriageService, PatientTriageService>();
      services.AddScoped<IUsersStoreService, UsersStoreService>();
      services.AddScoped<IOdsOrganisationService, OdsOrganisationService>();

      services.AddAuthorization(options =>
      {
        AuthorizationPolicies.AddPolicies(options, Configuration);
      });
    }

    public void Configure(
      IApplicationBuilder app, 
      IWebHostEnvironment env,
      IApiVersionDescriptionProvider apiDescProvider)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
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
          foreach (var description in apiDescProvider.ApiVersionDescriptions)
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
      app.UseHttpsRedirection();
      app.UseRouting();
      app.UseAuthentication();
      app.UseAuthorization();

      app.UseMiddleware<RequestResponseLogging>();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers().RequireAuthorization();
      });
    }
  }
}
