using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using WmsHub.Business.Models;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.ChatBotService;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;

namespace WmsHub.Tests.Helper
{
  public static class TestConfiguration
  {

    public static IOptions<ArcusOptions> CreateArcusOptions()
    {
      ArcusOptions options = new ArcusOptions
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

    public static IOptions<PostcodeOptions> CreatePostcodeOptions()
    {
      PostcodeOptions options = new PostcodeOptions
      {
        PostcodeIoUrl = "http://api.postcodes.io/postcodes/"
      };

      return Options.Create(options);
    }

    public static IOptions<ProviderOptions> CreateProviderOptions()
    {
      ProviderOptions options = new ProviderOptions
      {
        CompletionDays = 84,
        NumDaysPastCompletedDate = 10
      };

      return Options.Create(options);
    }

    public static IOptions<AuthOptions> CreateProviderAuthOptions()
    {
      AuthOptions options = new AuthOptions
      {
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
      TextOptions options = new TextOptions
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
        new SmsTemplate (
          Guid.Parse("e7414b59-0cc7-4e56-9826-12d2c12920fa"),
          TextOptions.TEMPLATE_FAILEDTOCONTACT),
        new SmsTemplate (
          Guid.Parse("ff2e7db9-fbf6-48a2-aaac-98de74b04d4c"),
          TextOptions.TEMPLATE_GENERAL_FIRST),
        new SmsTemplate (
          Guid.Parse("8647f993-6ab0-46b7-95c6-5f03e99433a9"),
          TextOptions.TEMPLATE_GENERAL_SECOND),
        new SmsTemplate (
          Guid.Parse("85cef310-24d0-4ed0-98c6-4d4b2af7967e"),
          TextOptions.TEMPLATE_GP_FIRST),
        new SmsTemplate (
          Guid.Parse("48035641-2daa-4ee2-9b65-eeacb4d03a2e"),
          TextOptions.TEMPLATE_GP_SECOND),
        new SmsTemplate (
          Guid.Parse("713aea37-e836-470a-adc0-fae6bd0b82b1"),
          TextOptions.TEMPLATE_MSK_FIRST),
        new SmsTemplate (
          Guid.Parse("837db29f-fedd-4299-806d-cbc3063b58a4"),
          TextOptions.TEMPLATE_MSK_SECOND),
        new SmsTemplate (
          Guid.Parse("4a368f50-83ee-4b09-8f31-0fb17ffbd210"),
          TextOptions.TEMPLATE_NONGP_DECLINED),
        new SmsTemplate (
          Guid.Parse("550ba077-4b57-45d0-afdc-52471c671313"),
          TextOptions.TEMPLATE_NONGP_REJECTED),
        new SmsTemplate (
          Guid.Parse("ec5cd13c-ef71-49df-bce2-630a46076037"),
          TextOptions.TEMPLATE_NONGP_TERMINATED),
        new SmsTemplate (
          Guid.Parse("261b3add-0d94-46fb-9830-b13468844bcb"),
          TextOptions.TEMPLATE_NUMBERNOTMONITORED),
        new SmsTemplate (
          Guid.Parse("6f8f21ae-bedd-4add-aa7f-1bca409ed8d4"),
          TextOptions.TEMPLATE_PHARMACY_FIRST),
        new SmsTemplate (
          Guid.Parse("c61ea496-691f-4020-b89a-8721880ec9fb"),
          TextOptions.TEMPLATE_PHARMACY_SECOND),
        new SmsTemplate (
          Guid.Parse("42eb69bb-86e6-4a4c-b77b-32232142b9c3"),
          TextOptions.TEMPLATE_SELF_CANCELLEDDUPLICATE),
        new SmsTemplate (
          Guid.Parse("eb827b19-5735-4df3-a3f8-488e7e17532e"),
          TextOptions.TEMPLATE_SELF_FIRST),
        new SmsTemplate (
          Guid.Parse("1e98e856-67f8-47c1-ae55-b315afc46659"),
          TextOptions.TEMPLATE_SELF_SECOND)
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
}
