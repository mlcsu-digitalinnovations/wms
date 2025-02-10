using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.ChatBotService;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Models.ProviderService;
using static WmsHub.Common.Helpers.Constants.MessageTemplateConstants;

namespace WmsHub.Tests.Helper;

public static class TestConfiguration
{

  public static IOptions<ArcusOptions> CreateArcusOptions()
  {
    ArcusOptions options = new()
    {
      // DO NOT STORE ApiKey HERE
      ApiKey = "",
      ContactFlowName = "NHS Weight Management Service",
      Endpoint = "https://teqdjm8o13.execute-api.eu-west-2" +
        ".amazonaws.com/dev/callList",
      IsNumberWhiteListEnabled = true,
      ReturnLimit = 600,
      NumberWhiteList = new List<string>()
        { "+447000000000", "+441000000000" }
    };

    return Options.Create(options);
  }

  public static GpDocumentProxyOptions CreateGpDocumentProxyOptions()
  {
    GpDocumentProxyOptions options = new()
    {
      AwaitingDischargeRejectionReasons = [
        "Failed to Retrieve File (7006)",
        "Failed to Retrieve File (7007)",
        "MESH acknowledgement not at correct HTTP status, see error ID"
      ],
      CompleteRejectionReasons = [
        "REJ05"
      ],
      GpdpCompleteRejectionReasons = [
        "GPDPREJ05"
      ],
      GpdpUnableToDischargeRejectionReasons = [
        "GPDPREJ03",
        "GPDPREJ04",
        "GPDPREJ06",
        "GPDPREJ07",
        "GPDPREJ08"
      ],
      GpdpTracePatientRejectionReasons =
      [
        "GPDPREJ01",
        "GPDPREJ02"
      ],
      Gp = new()
      {
        ProgrammeOutcomeCompleteTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeDidNotCommenceTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeDidNotCompleteTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeFailedToContactTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeInvalidContactDetailsTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeRejectedAfterProviderSelectionTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeRejectedBeforeProviderSelectionTemplateId = Guid.NewGuid()
      },
      Msk = new()
      {
        ProgrammeOutcomeCompleteTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeDidNotCommenceTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeDidNotCompleteTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeFailedToContactTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeInvalidContactDetailsTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeRejectedAfterProviderSelectionTemplateId = Guid.NewGuid(),
        ProgrammeOutcomeRejectedBeforeProviderSelectionTemplateId = Guid.NewGuid()
      },
      TracePatientRejectionReasons =
      [
        "REJ01",
        "REJ02"
      ],
      UnableToDischargeRejectionReasons = [
        "REJ03",
        "REJ04",
        "REJ05",
        "REJ06",
        "REJ07",
        "REJ08",
        "CONNECT3",
        "No Acknowledge received from MESH after 5 days",
        "No Acknowledge received from Emis",
        "Tif Conversion Failed. File size too big",
        "Organisation",
        "No live SystmOne unit for ODS code",
        "Recalled by Sender",
        "Not acknowledged by EMIS",
        "No acknowledgement received from Mesh after 5 days",
      ]
    };

    return options;
  }

  public static IOptions<ProviderOptions> CreateProviderOptions()
  {
    ProviderOptions options = new()
    {
      CompletionDays = 84,
      NumDaysPastCompletedDate = 10
    };

    return Options.Create(options);
  }

    public static IOptions<AuthOptions> CreateProviderAuthOptions()
    {
      AuthOptions options = new()
      {
        EmailReplyToId = Guid.NewGuid().ToString(),
        EmailTemplateId = Guid.NewGuid().ToString(),
        SmsTemplateId = Guid.NewGuid().ToString(),
        SmsSenderId = Guid.NewGuid().ToString(),
        SmsApiKey = "abcdefg123456",
        NotifyLink = "TestLink",
        TokenExpiry = 5
      };
      return Options.Create(options);
    }

  public static IOptions<TextOptions> CreateTextOptions()
  {
    TextOptions options = new()
    {
      IsNumberWhiteListEnabled = true,
      NotifyLink = "https://app-nhseiwmshubui-uks-pre-1.azurewebsites.net/" +
        "patient/welcome",
      GeneralReferralNotifyLink =
        "https://app-nhseiwmshubui-uks-pre-1.azurewebsites.net" +
        "/patient/welcome",
      NumberWhiteList = new List<string>()
        { "+447000000000", "+441000000000" },
      SearchPredicate = null,
      // DO NOT STORE SmsApiKey HERE
      SmsApiKey = Environment.GetEnvironmentVariable(
        "WmsHub.GovUkNotify.Api_TextSettings:SmsApiKey"),
      // DO NOT STORE SmsBearerToken HERE
      SmsBearerToken = "",
      // DO NOT STORE SmsSenderId HERE
      SmsSenderId = Environment.GetEnvironmentVariable(
        "WmsHub.GovUkNotify.Api_TextSettings:SmsSenderId"),     
      SmsTemplates = new List<SmsTemplate>
      {
        new(
          Guid.Parse("df97c912-3f98-4112-8d16-359ead518116"),
          TEMPLATE_DYNAMIC_SOURCE_REFERRAL_FIRST),
        new(
          Guid.Parse("d075e8dd-dcc0-4f6a-83c3-eb44e864e7eb"),
          TEMPLATE_DYNAMIC_SOURCE_REFERRAL_SECOND),
        new(
          Guid.Parse("a712df8f-a8a8-41e1-8130-06727b9f42b0"),
          TEMPLATE_DYNAMIC_SOURCE_REFERRAL_THIRD),
        new(
          Guid.Parse("e7414b59-0cc7-4e56-9826-12d2c12920fa"),
          TEMPLATE_FAILEDTOCONTACT_SMS),
        new(
          Guid.Parse("ff2e7db9-fbf6-48a2-aaac-98de74b04d4c"),
          TEMPLATE_GENERAL_FIRST),
        new(
          Guid.Parse("8647f993-6ab0-46b7-95c6-5f03e99433a9"),
          TEMPLATE_GENERAL_SECOND),
        new(
          Guid.Parse("85cef310-24d0-4ed0-98c6-4d4b2af7967e"),
          TEMPLATE_GP_FIRST),
        new(
          Guid.Parse("48035641-2daa-4ee2-9b65-eeacb4d03a2e"),
          TEMPLATE_GP_SECOND),
        new(
          Guid.Parse("713aea37-e836-470a-adc0-fae6bd0b82b1"),
          TEMPLATE_MSK_FIRST),
        new(
          Guid.Parse("837db29f-fedd-4299-806d-cbc3063b58a4"),
          TEMPLATE_MSK_SECOND),
        new(
          Guid.Parse("4a368f50-83ee-4b09-8f31-0fb17ffbd210"),
          TEMPLATE_NONGP_DECLINED),
        new(
          Guid.Parse("550ba077-4b57-45d0-afdc-52471c671313"),
          TEMPLATE_NONGP_REJECTED),
        new(
          Guid.Parse("ec5cd13c-ef71-49df-bce2-630a46076037"),
          TEMPLATE_NONGP_TERMINATED),
        new(
          Guid.Parse("261b3add-0d94-46fb-9830-b13468844bcb"),
          TEMPLATE_NUMBERNOTMONITORED),
        new(
          Guid.Parse("6f8f21ae-bedd-4add-aa7f-1bca409ed8d4"),
          TEMPLATE_PHARMACY_FIRST),
        new(
          Guid.Parse("c61ea496-691f-4020-b89a-8721880ec9fb"),
          TEMPLATE_PHARMACY_SECOND),
        new(
          Guid.Parse("42eb69bb-86e6-4a4c-b77b-32232142b9c3"),
          TEMPLATE_SELF_CANCELLEDDUPLICATE),
        new(
          Guid.Parse("eb827b19-5735-4df3-a3f8-488e7e17532e"),
          TEMPLATE_SELF_FIRST),
        new(
          Guid.Parse("1e98e856-67f8-47c1-ae55-b315afc46659"),
          TEMPLATE_SELF_SECOND)
      },
      TokenEnabled = false,
      // DO NOT STORE TokenPassword HERE
      TokenPassword = "",
      // DO NOT STORE TokenSecret HERE
      TokenSecret = ""
    };

    return Options.Create(options);
  }
}
