using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using WmsHub.Business;
using WmsHub.Business.Models.ChatBotService;
using WmsHub.Business.Services;
using WmsHub.ChatBot.Api.Clients;
using WmsHub.Common.Api.Middleware;
using WmsHub.Common.SignalR;

namespace WmsHub.ChatBot.Api
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
      services.AddCors();
      services.AddSignalR();
      services.AddAutoMapper(typeof(Startup),
        typeof(Business.Models.Profiles.CallProfile));

      services.AddApiVersioning(options =>
      {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
      });

      services.AddApiVersioning(options =>
      {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
      });

      services.AddControllers();

      services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
        .AddApiKeyInHeaderOrQueryParams<ApiKeyProvider>(options =>
        {
          options.Realm = "Chat Bot API";
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

      string filePath =
       Path.Combine(AppContext.BaseDirectory, "WmsHub.ChatBot.Api.xml");
      bool isXmlDocAvailable = File.Exists(filePath);

      if (_env.IsDevelopment())
      {
        services.AddSwaggerGen(c =>
        {
          c.SwaggerDoc(
            "v1.0",
            new OpenApiInfo {Title = "WmsHub.ChatBot.Api", Version = "v1.0"});

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

          if (isXmlDocAvailable) c.IncludeXmlComments(filePath);
        });
      }

      services.AddOptions<ArcusOptions>()
        .Bind(Configuration.GetSection(ArcusOptions.SectionKey))
        .ValidateDataAnnotations();
      services.AddHsts(options =>
      {
        options.MaxAge = DateTime.Now.AddYears(2) - DateTime.Now;
      });
      services.AddScoped<IChatBotService, ChatBotService>();
      services.AddScoped<IArcusClientHelper, ArcusClientHelper>();
      services
        .AddScoped<IRequestResponseLogService, RequestResponseLogService>();
    }

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
        app.UseSwaggerUI(c =>
        {
          c.SwaggerEndpoint(
            "/swagger/v1.0/swagger.json", "WmsHub.ChatBot.Api v1.0");
        });
      }
      else
      {
        app.UseExceptionHandler("/error");
      }
      // The default HSTS value is 30 days. You may want to change this for
      // production scenarios, see https://aka.ms/aspnetcore-hsts.
      app.UseHsts();

      string[] allowedOrigins =
        Configuration.GetSection("SignalR:AllowedOrigins").Get<string[]>();

      if (allowedOrigins != null)
      {
        app.UseCors(builder =>
        {
          builder.WithOrigins(allowedOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
        });
      }

      app.UseCors("CorsPolicy");

      app.UseHttpsRedirection();
      app.UseRouting();
      app.UseAuthentication();
      app.UseAuthorization();

      app.UseMiddleware<RequestResponseLogging>();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers().RequireAuthorization();
        endpoints.MapHub<SignalRHub>("/signalR");
      });
    }
  }
}
