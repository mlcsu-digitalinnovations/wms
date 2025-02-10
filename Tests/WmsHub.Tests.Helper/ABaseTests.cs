using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace WmsHub.Tests.Helper
{
  public abstract class ABaseTests
  {
    protected const string MOBILE = "07111111111";
    protected const string MOBILE_E164 = "+447111111111";
    protected const string MOBILE_TELEPHONE = "02222222222";
    protected const string MOBILE_TELEPHONE_E164 = "+442222222222";
    protected const string MOBILE_INVALID_SHORT = "0712345678";
    protected const string MOBILE_INVALID_LONG = "071234567890";

    protected const string NHSNUMBER_VALID = "9999999999";
    
    protected const string REFERRINGGPPRACTICENUMBER_VALID = "M82040";
    protected const string REFERRINGGPPRACTICENUMBER_NOTAPPLICABLE = "V81998";
    protected const string REFERRINGGPPRACTICENUMBER_NOTKNOWN = "V81999";
    protected const string REFERRINGGPPRACTICENUMBER_NOTREGISTERED = "V81997";       

    protected const string TELEPHONE = "01111111111";
    protected const string TELEPHONE_E164 = "+441111111111";
    protected const string TELEPHONE_MOBILE_E164 = "+447222222222";
    protected const string TELEPHONE_MOBILE = "07222222222";    
    protected const string TELEPHONE_INVALID_SHORT = "012345678";
    protected const string TELEPHONE_INVALID_LONG = "012345678901";

    public const string TEST_USER_ID = "571342f1-c67d-49bf-a9c6-40a41e6dc702";

    protected static ITestOutputHelper _testOutput;

    protected static ClaimsPrincipal GetClaimsPrincipal()
    {
      return GetClaimsPrincipalWithId(TEST_USER_ID);
    }

    protected static ClaimsPrincipal GetInvalidClaimsPrincipal()
    {
      return GetClaimsPrincipalWithId(Guid.NewGuid().ToString());
    }

    protected static ClaimsPrincipal GetClaimsPrincipalWithId(string id)
    {
      List<Claim> claims = new List<Claim>()
        { 
        new Claim(ClaimTypes.Sid, id) ,
        new Claim(ClaimTypes.NameIdentifier, "Test") ,
      };

      ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);

      ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      return claimsPrincipal;
    }

    protected static async Task<ProblemDetails> LogProblemDetailsAsync(
      HttpContext context)
    {
      try
      {
        if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
          var problemDetails = await JsonSerializer
            .DeserializeAsync<ProblemDetails>(context.Response.Body);
          problemDetails.Extensions.ToList()
            .ForEach(e => _testOutput.WriteLine(e.ToString()));

          return problemDetails;
        }
      }
      catch (Exception ex)
      {
        _testOutput.WriteLine(ex.ToString());
      }
      return null;
    }

    /// <summary>
    /// Determines if the given property of a type has a expected attribute.
    /// </summary>
    /// <typeparam name="T">The type containing the property.</typeparam>
    /// <typeparam name="A">The type of the expected attribute.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="inherit">
    /// true to search this member's inheritance chain to find attributes.
    /// </param>
    /// <returns>true if the type's property has the expected attribute.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected static bool HasAttribute<T, A>(
      string propertyName,
      bool inherit = true)
    {
      PropertyInfo property = typeof(T).GetProperty(propertyName)
        ?? throw new InvalidOperationException(
          $"The type '{typeof(T).Name}' does not contain the property '{propertyName}'.");

      object[] attributes = property.GetCustomAttributes(typeof(A), inherit);
      
      return (attributes.Length == 1 && attributes.Single().GetType() == typeof(A));
    }
  }
}
