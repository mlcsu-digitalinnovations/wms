using Microsoft.EntityFrameworkCore;
using WmsHub.Business;
using WmsHub.ErsMock.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DatabaseContext> (options =>
  {
    options.UseSqlServer(builder.Configuration.GetConnectionString("WmsHub"),
      opt => opt.EnableRetryOnFailure());
  #if DEBUG
    options.EnableSensitiveDataLogging();
  #endif
    options.EnableDetailedErrors();
  });

builder.Services.AddOptions<ErsMockApiOptions>()
  .BindConfiguration(ErsMockApiOptions.ConfigSectionPath)
  .ValidateDataAnnotations()
  .ValidateOnStart();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
