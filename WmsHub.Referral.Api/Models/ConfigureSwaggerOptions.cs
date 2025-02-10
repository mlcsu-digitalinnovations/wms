using Asp.Versioning.ApiExplorer;
using WmsHub.Common.Api.Models;

namespace WmsHub.Referral.Api.Models;

public class ConfigureSwaggerOptions : AConfigureSwaggerOptions
{
  public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : base(provider)
  {
  }

  protected override string ApiName => "Referral";
}
