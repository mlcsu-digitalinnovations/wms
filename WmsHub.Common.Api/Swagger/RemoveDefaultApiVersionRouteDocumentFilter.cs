using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace WmsHub.Common.Api.Swagger
{
  public class RemoveDefaultApiVersionRouteDocumentFilter : IDocumentFilter
  {
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
      foreach (var apiDescription in context.ApiDescriptions)
      {
        //ApiParameterDescription versionParam = apiDescription
        //  .ParameterDescriptions
        //  .FirstOrDefault(p => p.Name == "version");
        //    //&& p.Source.Id.Equals(
        //    //  "Query", StringComparison.InvariantCultureIgnoreCase));



        if (!apiDescription.RelativePath.Contains(
          apiDescription.GroupName,
          StringComparison.InvariantCultureIgnoreCase))
        {
          var route = "/" + apiDescription.RelativePath.TrimEnd('/');
          swaggerDoc.Paths.Remove(route);
        }
      }
    }
  }
}
