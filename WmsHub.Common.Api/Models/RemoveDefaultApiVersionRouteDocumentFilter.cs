using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace WmsHub.Common.Api.Models
{
  public class RemoveDefaultApiVersionRouteDocumentFilter : IDocumentFilter
  {
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
      foreach (var apiDescription in context.ApiDescriptions)
      {
        ApiParameterDescription apiParameterDescription = apiDescription
          .ParameterDescriptions
          .Where(p => p.Name == "api-version")
          .FirstOrDefault(p => p.Source.Id
            .Equals("Query", StringComparison.InvariantCultureIgnoreCase));

        if (apiParameterDescription != null)
        {
          var route = "/" + apiDescription.RelativePath.TrimEnd('/');
          swaggerDoc.Paths.Remove(route);
        }
      }
    }
  }
}
