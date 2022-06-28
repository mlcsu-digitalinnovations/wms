using AspNetCore.Authentication.ApiKey;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using WmsHub.Business;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Middleware;

namespace WmsHub.TextMessage.Api
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
    //Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddApiVersioning(config =>
      {
        // Specify the default API Version as 1.0
        config.DefaultApiVersion = new ApiVersion(1, 0);
        // If the client hasn't specified the API version in the request,
        // use the default API version number 
        config.AssumeDefaultVersionWhenUnspecified = true;
        // Advertise the API versions supported for the particular endpoint
        config.ReportApiVersions = true;
        // DEFAULT Version reader is QueryStringApiVersionReader();  
        // clients request the specific version using the X-version header
        config.ApiVersionReader = new HeaderApiVersionReader("X-version");
      });

      services.AddAutoMapper(new[]
      {
        typeof(Startup),
        typeof(Business.Models.Profiles.TextMessageProfile)
      });

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
              context.Response.StatusCode = StatusCodes.Status403Forbidden;
              context.Response.ContentType = "application/json";
              await context.Response.WriteAsync(
                "{\"detail\":\"Api key was missing or invalid.\"}");
              context.Handled();
            }
          };
        });

      services.AddDbContext<DatabaseContext>(options =>
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

      string filePath =
        Path.Combine(AppContext.BaseDirectory, "WmsHub.TextMessage.Api.xml");
      bool isXmlDocAvailable = File.Exists(filePath);

      if (_env.IsDevelopment())
      {
        services.AddSwaggerGen(c =>
        {
          c.SwaggerDoc(
            "v1.0",
            new OpenApiInfo
            {
              Title = "WmsHub.TextMessage.Api",
              Version = "v1.0"
            });
          if (isXmlDocAvailable) c.IncludeXmlComments(filePath);

          OpenApiSecurityScheme jwtSecurityScheme = new OpenApiSecurityScheme
          {
            Scheme = "bearer",
            BearerFormat = "JWT",
            Name = "JWT Authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Description =
              "Put **_ONLY_** your JWT Bearer token on textbox below!",

            Reference = new OpenApiReference
            {
              Id = JwtBearerDefaults.AuthenticationScheme,
              Type = ReferenceType.SecurityScheme
            }
          };

          c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id,
            jwtSecurityScheme);

          c.AddSecurityRequirement(new OpenApiSecurityRequirement
          {
            {jwtSecurityScheme, Array.Empty<string>()}
          });

        });
      }

      services.AddOptions<TextOptions>()
        .Bind(Configuration.GetSection(TextOptions.SectionKey))
        .ValidateDataAnnotations();
      
      services.AddHsts(options =>
      {
        options.MaxAge = DateTime.Now.AddYears(2) - DateTime.Now;
      });
      
      services.AddScoped<ITextService, TextService>();
      services.AddScoped<ITextNotificationHelper, TextNotificationHelper>();
      services.AddScoped<Filters.ApiAccessFilterAsync>();
      services
        .AddScoped<IRequestResponseLogService, RequestResponseLogService>();
      services.AddHttpContextAccessor();
    }

    // This method gets called by the runtime. Use this method to configure 
    // the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
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
        app.UseSwaggerUI(
          c => c.SwaggerEndpoint("/swagger/v1.0/swagger.json",
            "WmsHub.TextMessage.Api v1.0"));
      }
      else
      {
        app.UseExceptionHandler("/error");
      }
      // The default HSTS value is 30 days. You may want to change this for
      // production scenarios, see https://aka.ms/aspnetcore-hsts.
      app.UseHsts();

      NotifyTokenHandler.Configure(
        app.ApplicationServices.GetService<IOptions<TextOptions>>(),
        Configuration.GetConnectionString("WmsHub"),
        app.ApplicationServices.GetService<IHttpContextAccessor>());

      AuthServiceHelper.Configure(Configuration.GetConnectionString("WmsHub"),
        app.ApplicationServices.GetService<IHttpContextAccessor>());

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseMiddleware<RequestResponseLogging>();

      app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
  }
}
