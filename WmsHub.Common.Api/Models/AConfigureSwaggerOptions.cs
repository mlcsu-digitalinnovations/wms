using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WmsHub.Common.Api.Models
{
  public abstract class AConfigureSwaggerOptions
    : IConfigureOptions<SwaggerGenOptions>
  {
    protected readonly IApiVersionDescriptionProvider _provider;
    protected abstract string ApiName { get; }

    public AConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) =>
      _provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
      // add a swagger document for each discovered API version
      // note: you might choose to skip or document deprecated API
      // versions differently
      foreach (var description in _provider.ApiVersionDescriptions)
      {
        options.SwaggerDoc(description.GroupName,
          CreateInfoForApiVersion(description));
      }

      options.DocumentFilter<RemoveDefaultApiVersionRouteDocumentFilter>();
    }

    private OpenApiInfo CreateInfoForApiVersion(
      ApiVersionDescription description)
    {
      var info = new OpenApiInfo()
      {
        Title = $"DWMP {ApiName} API",
        Version = description.ApiVersion.ToString(),
      };

      if (description.IsDeprecated)
      {
        info.Description += " This API version has been deprecated.";
      }

      return info;
    }
  }
}
