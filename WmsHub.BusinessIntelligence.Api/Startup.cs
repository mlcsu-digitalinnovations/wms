using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using WmsHub.Business;
using WmsHub.Business.Models.BusinessIntelligence;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.BusinessIntelligence.Api.AuthPolicies;
using WmsHub.BusinessIntelligence.Api.Models;
using WmsHub.Common.Api.Middleware;
using WmsHub.Common.Api.Models;

namespace WmsHub.BusinessIntelligence.Api
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

    // This method gets called by the runtime. Use this method to 
    // add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddAutoMapper(typeof(Startup), typeof(Business.Models.Profiles.CallProfile));
      services.AddControllers();

      ApiVersionOptions apiVersionOptions = new();
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

      services.AddHealthChecks();
      services.AddControllers();

      services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
        .AddApiKeyInHeaderOrQueryParams<ApiKeyProvider>(options =>
        {
          options.Realm = "Business Intelligence API";
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

      var filePath = Path.Combine(
        AppContext.BaseDirectory, "WmsHub.BusinessIntelligence.Api.xml");
      bool isXmlDocAvailable = File.Exists(filePath);

      if (_env.IsDevelopment())
      {

        services
          .AddTransient<IConfigureOptions<SwaggerGenOptions>,
            ConfigureSwaggerOptions>();

        services.AddSwaggerGen(c =>
        {
          c.EnableAnnotations();
          c.CustomSchemaIds((type) => type.FullName);
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
            c.IncludeXmlComments(filePath);
          }

          c.DescribeAllParametersInCamelCase();
          c.MapType<Guid>(() => new OpenApiSchema { Type = "Guid" });
        });
      }

      services.AddOptions<BusinessIntelligenceOptions>()
        .Bind(Configuration.GetSection(BusinessIntelligenceOptions.SectionKey))
        .ValidateDataAnnotations();
      services.AddHsts(options =>
      {
        options.MaxAge = DateTime.Now.AddYears(2) - DateTime.Now;
      });
      services.AddScoped<IBusinessIntelligenceService,
        BusinessIntelligenceService>();
      services
        .AddScoped<IRequestResponseLogService, RequestResponseLogService>();

      services.AddAuthorization(options =>
      {
        AuthorizationPolicies.AddPolicies(options, Configuration);
      });
    }

    // This method gets called by the runtime. Use this method to configure 
    // the HTTP request pipeline.
    public void Configure(
      IApplicationBuilder app,
      IWebHostEnvironment env,
      IApiVersionDescriptionProvider apiVersionDescriptionProvider)
    {
      if (env.IsDevelopment())
      {
        app.UseStaticFiles(new StaticFileOptions
        {
          FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "SwaggerStyles")),
          RequestPath = "/SwaggerStyles"
        });
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
          foreach (var description in apiVersionDescriptionProvider
            .ApiVersionDescriptions)
          {
            options.SwaggerEndpoint(
              $"/swagger/{description.GroupName}/swagger.json",
              description.GroupName.ToUpperInvariant());
          }

          options.InjectStylesheet("/SwaggerStyles/wms-style.css");
          options.DocExpansion(
            Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.Full);

        });
      }
      else
      {
        app.UseExceptionHandler("/error");
      }
      // The default HSTS value is 30 days. You may want to change this for
      // production scenarios, see https://aka.ms/aspnetcore-hsts.
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
