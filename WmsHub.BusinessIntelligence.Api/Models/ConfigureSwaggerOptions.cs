using Microsoft.AspNetCore.Mvc.ApiExplorer;
using WmsHub.Common.Api.Models;

namespace WmsHub.BusinessIntelligence.Api.Models
{
  public class ConfigureSwaggerOptions : AConfigureSwaggerOptions
  {
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
      : base(provider)
    {
    }

    protected override string ApiName => "Business Intelligence";
  }
}
