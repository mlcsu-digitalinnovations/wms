using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Options;
using Notify.Models.Responses;
using WmsHub.Business;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using WmsHub.TextMessage.Api.Models.Notify;

namespace WmsHub.TextMessage.Api.Tests.TestSetup
{
  public static class TestGenerator
  {
    public static CallbackPostRequest CallbackPostRequestGenerator(
      string reference = null,
      string to = null,
      string status = null,
      DateTimeOffset created_at = default,
      DateTimeOffset? completed_at =default,
      DateTimeOffset? sent_at = default,
      string notification_type = null,
      string source_number = null,
      string destination_number = null,
      string message = null,
      DateTimeOffset date_receieved = default)
    {
      Random random = new Random();

      return new CallbackPostRequest
      {
        Id = Guid.NewGuid().ToString(),
        Reference = reference ?? Generators.GenerateUbrn(random),
        To = to ?? Generators.GenerateMobile(random),
        Status = status ?? CallbackStatus.None.ToString(),
        Notification_type =
          notification_type ?? CallbackNotification.Sms.ToString(),
        Source_number = source_number ?? Generators.GenerateMobile(random),
        Destination_number =
          destination_number ?? Generators.GenerateMobile(random),
        Message = message ?? "Test Message",
        Created_at = created_at == default
           ? DateTimeOffset.Now
           : created_at,
        Completed_at = completed_at == default
           ? DateTimeOffset.Now
           : completed_at,
        Sent_at = sent_at == default
          ? DateTimeOffset.Now
          : sent_at,
        Date_received = date_receieved == default
          ? DateTimeOffset.Now
          : date_receieved
      };
    }

    public static List<SmsMessage> GenerateSmsMessages()
    {
      Random random = new Random();
      return new List<SmsMessage>
      {
        new SmsMessage
        {
          MobileNumber = Generators.GenerateMobile(random),
          Personalisation = new Dictionary<string, dynamic>
          {
            {"givenName", "referral.GivenName"},
            {"familyName", "referral.FamilyName"},
          }
        }
      };
    }
  }

  public class TestSetup
  {
    public const string TEST_USER_ID = "571342f1-c67d-49bf-a9c6-40a41e6dc702";
    protected static ClaimsPrincipal GetClaimsPrincipal()
    {
      List<Claim> claims = new List<Claim>()
        { new Claim(ClaimTypes.Sid, TEST_USER_ID) };

      ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);

      ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      return claimsPrincipal;
    }
  }

}
