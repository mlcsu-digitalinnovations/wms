using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Serialization;
using WmsHub.Business;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ElectiveCareReferral;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.MessageService;
using WmsHub.Business.Models.MSGraph;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Models.Profiles;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Middleware;
using WmsHub.Common.Api.Swagger;
using WmsHub.Common.Apis.Ods;
using WmsHub.Common.Apis.Ods.Fhir;
using WmsHub.Common.Apis.Ods.PostcodesIo;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.AuthPolicies;
using WmsHub.Referral.Api.Models;
using WmsHub.Referral.Api.Models.MskReferral;
using WmsHub.Referral.Api.Models.Profiles;

namespace WmsHub.Referral.Api;

public class Startup
{
  public Startup(IConfiguration configuration, IWebHostEnvironment env)
  {
    Configuration = configuration;
    _env = env;
  }

  private readonly IWebHostEnvironment _env;
  public IConfiguration Configuration { get; }

  public void ConfigureServices(IServiceCollection services)
  {
    Common.Api.Models.ApiVersionOptions apiVersionOptions =
      new();
    Configuration.Bind("ApiVersion", apiVersionOptions);

    services
      .AddApiVersioning(options =>
      {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(
          apiVersionOptions.DefaultMajor, apiVersionOptions.DefaultMinor);
        options.ReportApiVersions = true;
      })
      .AddApiExplorer(options =>
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
      typeof(PharmacyReferralProfile),
      typeof(MsGraphProfile)
    });

    services.AddHealthChecks();
    services.AddControllers()
      .AddJsonOptions(options => options.JsonSerializerOptions
          .Converters
          .Add(new JsonStringEnumConverter()));

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

        if (isXmlDocAvailable)
        {
          options.IncludeXmlComments(filePath);
        }
      });
    }

    services.AddHsts(options => 
      options.MaxAge = DateTime.Now.AddYears(2) - DateTime.Now);

    services.AddOptions<DeprivationOptions>()
      .Bind(Configuration.GetSection(DeprivationOptions.SectionKey))
      .ValidateDataAnnotations();

    services.AddOptions<ElectiveCareReferralOptions>()
      .Bind(Configuration.GetSection(ElectiveCareReferralOptions.SectionKey))
      .ValidateDataAnnotations();

    services.AddOptions<GpDocumentProxyOptions>()
      .Bind(Configuration.GetSection(GpDocumentProxyOptions.SectionKey))
      .ValidateDataAnnotations()
      .ValidateOnStart();

    services.AddOptions<MskReferralOptions>()
      .Bind(Configuration.GetSection(MskReferralOptions.SectionKey))
      .ValidateDataAnnotations();

    services.AddOptions<OdsOrganisationOptions>()
      .Bind(Configuration.GetSection(OdsOrganisationOptions.SectionKey))
      .ValidateDataAnnotations();

    services.AddOptions<PharmacyReferralOptions>()
      .Bind(Configuration.GetSection(
        PharmacyReferralOptions.OptionsSectionKey))
      .ValidateDataAnnotations();

    services.AddOptions<ProviderOptions>()
      .Bind(Configuration.GetSection(ProviderOptions.SectionKey))
      .ValidateDataAnnotations();

    services.AddOptions<QuestionnaireNotificationOptions>()
      .Bind(Configuration.GetSection(NotificationOptions.SECTION_KEY))
      .ValidateDataAnnotations();

    services.AddOptions<NotificationOptions>()
     .Bind(Configuration.GetSection(NotificationOptions.SECTION_KEY))
     .ValidateDataAnnotations();

      services.AddOptions<MessageOptions>()
        .Bind(Configuration.GetSection(MessageOptions.SECTIONKEY))
        .ValidateDataAnnotations();

    services.AddOptions<ReferralTimelineOptions>()
      .Bind(Configuration.GetSection(ReferralTimelineOptions.SectionKey))
      .ValidateDataAnnotations()
      .ValidateOnStart();

    services.AddOptions<StaffReferralOptions>()
      .Bind(Configuration.GetSection(StaffReferralOptions.SectionKey))
      .ValidateDataAnnotations();

    services.AddOptions<NotificationOptions>()
     .Bind(Configuration.GetSection(NotificationOptions.SECTION_KEY))
     .ValidateDataAnnotations();

    services.AddOptions<MsGraphOptions>()
     .Bind(Configuration.GetSection(MsGraphOptions.SECTION_KEY))
     .ValidateDataAnnotations();

    services.AddOptions<ProcessStatusOptions>()
     .Bind(Configuration.GetSection(ProcessStatusOptions.SectionKey))
     .ValidateDataAnnotations()
     .ValidateOnStart();

    services.AddProcessStatusService();

    services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
    services.AddScoped<IDeprivationService, DeprivationService>();
    services
      .AddScoped<
        IElectiveCareReferralService,
        ElectiveCareReferralService>();
    services.AddScoped<IEthnicityService, EthnicityService>();
    services.AddScoped<ILinkIdService, LinkIdService>();
    services.AddScoped<INotificationService, NotificationService>();

    services.AddOdsFhirService(Configuration);
    services.AddPostcodesIoService(Configuration);

      services.AddScoped<IMessageService, MessageService>();
      services.AddScoped<IMskOrganisationService, MskOrganisationService>();
      services.AddScoped<IOdsOrganisationService, OdsOrganisationService>();
      services.AddScoped<IOrganisationService, OrganisationService>();
      services.AddScoped<IPatientTriageService, PatientTriageService>();
      services.AddScoped<IPracticeService, PracticeService>();
      services.AddScoped<IPharmacyService, PharmacyService>();
      services.AddScoped<IProviderService, ProviderService>();
      services.AddScoped<IReferralAdminService, ReferralAdminService>();
      services.AddScoped<IReferralDischargeService,ReferralDischargeService>();
      services.AddScoped<
        IReferralQuestionnaireService, 
        ReferralQuestionnaireService>();
      services
        .AddScoped<IRequestResponseLogService, RequestResponseLogService>();
      services.AddScoped<IUsersStoreService, UsersStoreService>();
      services.AddScoped<IMessageService, MessageService>();
      services.AddScoped<IMSGraphService, MSGraphService>();

    services.AddAuthorization(options => AuthorizationPolicies.AddPolicies(
        options,
        Configuration,
        Environment.GetEnvironmentVariable(Constants.POLICIES_IN_TEST)));

    services.AddHttpClient<INotificationService, NotificationService>()
      .SetHandlerLifetime(TimeSpan.FromMinutes(5))
      .AddPolicyHandler(HttpClientHelper.GetRetryPolicy())
      .AddPolicyHandler(HttpClientHelper.GetCircuitBreakerPolicy());

    if (Environment.GetEnvironmentVariable(Constants.REFERRAL_IN_TEST)
      != null)
    {
      services.AddHttpClient<IReferralService, ReferralService>()
     .SetHandlerLifetime(TimeSpan.FromMinutes(5))
     .AddPolicyHandler(HttpClientHelper.GetRetryPolicyNoNotFound())
     .AddPolicyHandler(HttpClientHelper.GetCircuitBreakerPolicy());
    }
    else
    {
      services.AddHttpClient<IReferralService, ReferralService>(client =>
      {
        client.BaseAddress = new Uri(
          Configuration.GetSection(GpDocumentProxyOptions.SectionKey)
            .GetValue<string>(nameof(GpDocumentProxyOptions.Endpoint)));
        client.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue(
            $"Bearer",
            Configuration
              .GetSection(GpDocumentProxyOptions.SectionKey)
              .GetValue<string>(nameof(GpDocumentProxyOptions.Token)));
      });
    }

    services.AddHttpClient<IOdsOrganisationService, OdsOrganisationService>()
      .SetHandlerLifetime(TimeSpan.FromMinutes(5))
      .AddPolicyHandler(HttpClientHelper.GetRetryPolicyNoNotFound())
      .AddPolicyHandler(HttpClientHelper.GetCircuitBreakerPolicy());
  }

  public void Configure(
    IApplicationBuilder app,
    IWebHostEnvironment env,
    IApiVersionDescriptionProvider apiDescProvider)
  {
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger(options => 
          options.PreSerializeFilters.Add((swagger, httpReq) =>
            //Clear servers -element in swagger.json because it got the wrong 
            //port when hosted behind reverse proxy
            swagger.Servers.Clear()));
        app.UseSwaggerUI(options =>
        {
          foreach (ApiVersionDescription description in
            apiDescProvider.ApiVersionDescriptions)
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

      List<CultureInfo> cultures = new() { new CultureInfo("en-GB") };
      app.UseRequestLocalization(options =>
      {
        options.DefaultRequestCulture = new RequestCulture("en-GB");
        options.SupportedCultures = cultures;
        options.SupportedUICultures = cultures;
      });

      app.UseHsts();
      app.UseHttpsRedirection();
      app.UseRouting();
      app.UseAuthentication();
      app.UseAuthorization();

      app.UseMiddleware<RequestResponseLogging>();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers().RequireAuthorization();
        endpoints.MapHealthChecks("/health");
      });
    }
  }
}
