using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace WmsHub.Tests.Helper
{
  public abstract class ABaseTests
  {
    protected static ITestOutputHelper _testOutput;

    protected static async Task LogProblemDetailsAsync(HttpContext context)
    {
      try
      {
        if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
          var problemDetails = await JsonSerializer
            .DeserializeAsync<ProblemDetails>(context.Response.Body);
          problemDetails.Extensions.ToList()
            .ForEach(e => _testOutput.WriteLine(e.ToString()));
        }
      }
      catch (Exception ex)
      {
        _testOutput.WriteLine(ex.ToString());
      }
    }
  }
}
