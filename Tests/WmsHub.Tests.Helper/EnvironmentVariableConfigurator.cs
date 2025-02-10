using System;
using System.Collections.Generic;
using WmsHub.Common.Helpers;

namespace WmsHub.Tests.Helper;
public static class EnvironmentVariableConfigurator
{
  public static class EnvironmentVariables
  {
    public const string ADMIN_APIKEY = "AdminApiKey";
    public const string ADMIN_APIKEYVALUE =
      "hWAFRf9qG1H1CKyHf8lQHatD3a6hnbyqJY1kx0zNInoODH4ed2";
    public const string ALWAYS_ENCRYPTED_CLIENTID =
      "WmsHub_AlwaysEncrypted:ClientId";
    public const string ALWAYS_ENCRYPTED_CLIENTSECRET =
      "WmsHub_AlwaysEncrypted:ClientSecret";
    public const string ALWAYS_ENCRYPTED_TENANTID =
      "WmsHub_AlwaysEncrypted:TenantId";
    public const string APIKEY = "F8A5874D-AD63-4741-A588-00B32CED1303";
    public const string AUDIENCE = "http://gov.uk";
    public const string BI_APIKEY = "ApiKey";
    public const string BI_APIKEYVALUE = "THIS_IS_A_TEST_KEY";
    public const string BI_PROVIDER_STATUSES = 
      "BusinessIntelligenceOptions:ProviderSubmissionEndedStatusesValue";
    public const string BI_PROVIDER_STATUSES_VALUE = 
      "ProviderDeclinedByServiceUser,ProviderTerminated,ProviderRejected";
    public const string BI_WHITELIST_ENABLED = 
      "BusinessIntelligenceOptions:IsTraceIpWhitelistEnabled";
    public const string ELECTIVECARE_APIKEY = "ElectiveCareApiKey";
    public const string ELECTIVECARE_APIKEYVALUE =
      "hOJsUsxZVtey1fOmLS3AEKjEeJqkayyxgmQYH1e0uBoqMgmBSd";
    public const string DOCPROXYAWAITINGDISCHARGEREJECTIONREASONS = 
      "GpDocumentProxyOptions:AwaitingDischargeRejectionReasons:0";
    public const string DOCPROXYCOMPLETEREJECTIONREASONS =
      "GpDocumentProxyOptions:CompleteRejectionReasons:0";
    public const string DOCPROXYDELAYENDPOINT =
      "GpDocumentProxyOptions:DelayEndpoint";
    public const string DOCPROXYDELAYENDPOINTVALUE = "/delay/";
    public const string DOCPROXYENPOINT = "GpDocumentProxyOptions:Endpoint";
    public const string DOCPROXYENDPOINTVALUE = "https://localhost:7072";
    public const string DOCPROXYGPCOMPLETE = 
      "GpDocumentProxyOptions:Gp:ProgrammeOutcomeCompleteTemplateId";
    public const string DOCPROXYGPDIDNOTCOMMENCE =
      "GpDocumentProxyOptions:Gp:ProgrammeOutcomeDidNotCommenceTemplateId";
    public const string DOCPROXYGPDIDNOTCOMPLETE =
      "GpDocumentProxyOptions:Gp:ProgrammeOutcomeDidNotCompleteTemplateId";
    public const string DOCPROXYGPFAILEDTOCONTACT =
      "GpDocumentProxyOptions:Gp:ProgrammeOutcomeFailedToContactTemplateId";
    public const string DOCPROXYGPINVALIDCONTACTDETAILS =
      "GpDocumentProxyOptions:Gp:ProgrammeOutcomeInvalidContactDetailsTemplateId";
    public const string DOCPROXYGPREJECTEDAFTERPROVIDERSELECTION =
      "GpDocumentProxyOptions:Gp:ProgrammeOutcomeRejectedAfterProviderSelectionTemplateId";
    public const string DOCPROXYGPREJECTEDBEFOREPROVIDERSELECTION =
      "GpDocumentProxyOptions:Gp:ProgrammeOutcomeRejectedBeforeProviderSelectionTemplateId";
    public const string DOCPROXYGPDPCOMPLETEREJECTIONREASONS =
      "GpDocumentProxyOptions:GpdpCompleteRejectionReasons:0";
    public const string DOCPROXYGPDPTRACEPATIENTREJECTIONREASONS =
      "GpDocumentProxyOptions:GpdpTracePatientRejectionReasons:0";
    public const string DOCPROXYGPDPUNABLETODISCHARGEREJECTIONREASONS =
      "GpDocumentProxyOptions:GpdpUnableToDischargeRejectionReasons:0";
    public const string DOCPROXYMSKCOMPLETE =
      "GpDocumentProxyOptions:Msk:ProgrammeOutcomeCompleteTemplateId";
    public const string DOCPROXYMSKDIDNOTCOMMENCE =
      "GpDocumentProxyOptions:Msk:ProgrammeOutcomeDidNotCommenceTemplateId";
    public const string DOCPROXYMSKDIDNOTCOMPLETE =
      "GpDocumentProxyOptions:Msk:ProgrammeOutcomeDidNotCompleteTemplateId";
    public const string DOCPROXYMSKFAILEDTOCONTACT =
      "GpDocumentProxyOptions:Msk:ProgrammeOutcomeFailedToContactTemplateId";
    public const string DOCPROXYMSKINVALIDCONTACTDETAILS =
      "GpDocumentProxyOptions:Msk:ProgrammeOutcomeInvalidContactDetailsTemplateId";
    public const string DOCPROXYMSKREJECTEDAFTERPROVIDERSELECTION =
      "GpDocumentProxyOptions:Msk:ProgrammeOutcomeRejectedAfterProviderSelectionTemplateId";
    public const string DOCPROXYMSKREJECTEDBEFOREPROVIDERSELECTION =
      "GpDocumentProxyOptions:Msk:ProgrammeOutcomeRejectedBeforeProviderSelectionTemplateId";
    public const string DOCPROXYPOSTDISCHARGESLIMIT = "GpDocumentProxyOptions:PostDischargesLimit";
    public const string DOCPROXYPOSTDISCHARGESLIMITVALUE = "0";
    public const string DOCPROXYPOSTENDPOINT = "GpDocumentProxyOptions:PostEndpoint";
    public const string DOCPROXYPOSTENDPOINTVALULE = "/discharge";
    public const string DOCPROXYRESOLVEENDPOINT = "GpDocumentProxyOptions:ResolveEndpoint";
    public const string DOCPROXYRESOLVEENDPOINTVALULE = "/resolve/";
    public const string DOCPROXYTOKEN = "GpDocumentProxyOptions:Token";
    public const string DOCPROXYTOKENVALUE = "ABC123";
    public const string DOCPROXYTRACEPATIENTREJECTIONREASONS =
      "GpDocumentProxyOptions:TracePatientRejectionReasons:0";
    public const string DOCPROXYUNABLETODISCHARGEPATIENTREJECTIONREASONS =
      "GpDocumentProxyOptions:UnableToDischargeRejectionReasons:0";
    public const string DOCPROXYUPDATEENDPOINT = "GpDocumentProxyOptions:UpdateEndpoint";
    public const string DOCPROXYUPDATEENDPOINTVALUE = "/update/";
    public const string GENERALREFERRAL_APIKEY = "GeneralReferralApiKey";
    public const string GENERALREFERRAL_APIKEYVALUE =
      "u4VjGGIQ6rnmKdf8ozMinqG2wOrEYBfQ4WfWe0y0vRJ2oC0BGw";
    public const string ISSUER = "http://mytestsite.com";
    public const string MSK_APIKEY = "MskApiKey";
    public const string MSK_APIKEYVALUE =
      "5ezlaUFEL6t!J4fr1x*nt#knG8*n1G!xN%dzVaT4pvb&!I6S#M";
    public const string MSKCLAIMTYPE = "lkjhdfaslkfjh@opiuwetr34234bnm9DFHJB";
    public const string MSK_EMAIL_WHITELIST =
      "MskReferralOptions:EmailDomainWhitelist:0";
    public const string MSK_HUB_TEST1 = "MskReferralOptions:MskHubs:TEST1";
    public const string MSK_HUB_TEST1_VALUE = "UNIT Test 1";
    public const string QUESTIONNAIRE_APIKEY = "QuestionnaireApiKey";
    public const string QUESTIONNAIRE_APIKEYVALUE =
      "FqXK0J*n1pX3$lL8XUQpCk!8ej$8iUxb86L7vgS6n&JfJ^5n8#";
    public const string REFERRAL_COUNTS_API_KEY = "ReferralCountsApiKey";
    public const string REFERRAL_COUNTS_API_KEY_VALUE = "ApiKeyValueTest";
    public const string RMC_API_KEY = "RmcApiKey";
    public const string RMC_API_KEY_VALUE = "aoiureqnhr6askjh76434561654847";
    public const string TOKEN_VALUE = "NOT SET";
    public const string TOKENPASSWORD = "NotSetToAnythingInTest01";
    public const string TOKENSECRET = "123456789abcdefghIjlABCDEFGHIJKLMN";
    public const string VALIDUSER = "38DD9C1A-823E-4871-A160-80D31F29F95D";
  }

  public static void ConfigureEnvironmentVariablesForAlwaysEncrypted()
  {
    Environment.SetEnvironmentVariable(
      Constants.SQLCONNSTR_WMSHUB, 
      Constants.UNIT_TESTING);

    Environment.SetEnvironmentVariable(
      "WmsHub_AlwaysEncrypted:ClientId", 
      Guid.NewGuid().ToString());
    Environment.SetEnvironmentVariable(
      "WmsHub_AlwaysEncrypted:ClientSecret", 
      "abcde~ef1234567Fake");
    Environment.SetEnvironmentVariable(
      "WmsHub_AlwaysEncrypted:TenantId", 
      Guid.NewGuid().ToString());
  }

  public static void ConfigureEnvironmentVariablesForBusinessIntelligence()
  {
    Environment.SetEnvironmentVariable(
       "WmsHub_BusinessIntelligence_Api_MinDaysBetweenTraces", "1");
    Environment.SetEnvironmentVariable(
      "WmsHub_BusinessIntelligence_Api_DaysBetweenTraces", "7");
    Environment.SetEnvironmentVariable(
      "WmsHub_BusinessIntelligence_Api_MaxDaysBetweenTraces", "30");
  }

  public static void ConfigureEnvironmentVariablesForDocumentProxy()
  {
    Environment.SetEnvironmentVariable(
      "WmsHub_Referral_Api_GpDocumentProxyOptions:Endpoint",
      EnvironmentVariables.DOCPROXYENDPOINTVALUE);
    Environment.SetEnvironmentVariable(
      "WmsHub_Referral_Api_GpDocumentProxyOptions:Token", 
      EnvironmentVariables.TOKEN_VALUE);
  }

  public static void ConfigureEnvironmentVariableForMsk()
  {
    Environment.SetEnvironmentVariable(
      "WmsHub_Referral_Api_MskReferralOptions:MskHubs:TEST1",
      "Unit Test Hub");
    Environment.SetEnvironmentVariable(
      "WmsHub_Referral_Api_MskReferralOptions:EmailDomainWhitelist:0",
      "nhs.net");
    Environment.SetEnvironmentVariable(
      "WmsHub_Referral_Api_MskApiKey",
      EnvironmentVariables.APIKEY);
    Environment.SetEnvironmentVariable(
      "WmsHub_Referral_Api_MskClaimType",
      Guid.NewGuid().ToString());
    Environment.SetEnvironmentVariable(
      EnvironmentVariables.MSK_APIKEY,
      EnvironmentVariables.MSK_APIKEYVALUE);
  }

  public static void ConfigureEnvironmentVariablesForProvider()
  {

  }

  public static void ConfigureEnvironmentVariablesForReferral()
  {

  }

  public static void ConfigureEnvironmentVariablesForTextMessaging()
  {
    Environment.SetEnvironmentVariable(
      "WmsHub_TextMessage_Api_ApiKey", 
      EnvironmentVariables.APIKEY);
    Environment.SetEnvironmentVariable(
      "WmsHub_TextMessage_Api_TextSettings:Audience",
      EnvironmentVariables.AUDIENCE);
    Environment.SetEnvironmentVariable(
      "WmsHub_TextMessage_Api_TextSettings:IsNumberWhiteListEnabled", 
      "true");
    Environment.SetEnvironmentVariable(
      "WmsHub_TextMessage_Api_TextSettings:Issuer",
      EnvironmentVariables.ISSUER);
    Environment.SetEnvironmentVariable(
      "WmsHub_TextMessage_Api_TextSettings:SmsApiKey", 
      $"wms__test_key-{Guid.NewGuid()}");
    Environment.SetEnvironmentVariable(
      "WmsHub_TextMessage_Api_TextSettings:SmsSenderId", 
      Guid.NewGuid().ToString());
    Environment.SetEnvironmentVariable(
      "WmsHub_TextMessage_Api_TextSettings:TokenEnabled", 
      "true");
    Environment.SetEnvironmentVariable(
      "WmsHub_TextMessage_Api_TextSettings:TokenPassword",
      EnvironmentVariables.TOKENPASSWORD);
    Environment.SetEnvironmentVariable(
      "WmsHub_TextMessage_Api_TextSettings:TokenSecret", 
      EnvironmentVariables.TOKENSECRET);
    Environment.SetEnvironmentVariable(
      "WmsHub_TextMessage_Api_TextSettings:ValidUsers:0", 
      EnvironmentVariables.VALIDUSER);
  }

  public static void ConfigureEnvironmentVariablesForWmsHubUI()
  {
    Environment.SetEnvironmentVariable(
      "WmsHub_Ui_AzureAd:CallbackPath", "" +
      "/signin-oidc");
    Environment.SetEnvironmentVariable(
      "WmsHub_Ui_AzureAd:ClientId",
      Guid.NewGuid().ToString());
    Environment.SetEnvironmentVariable(
      "WmsHub_Ui_AzureAd:Domain",
      "mlcsu.nhs.uk");
    Environment.SetEnvironmentVariable(
      "WmsHub_Ui_AzureAd:Instance",
      "https://login.microsoftonline.com");
    Environment.SetEnvironmentVariable(
      "WmsHub_Ui_AzureAd:TenantId",
      Guid.NewGuid().ToString());
    Environment.SetEnvironmentVariable(
      "WmsHub_Ui_AzureFrontDoor_Uri", "https://localhost:44392");
  }

  public static List<KeyValuePair<string, string>> FakeGpDocumentProxyOptions()
  {
    List<KeyValuePair<string, string>> options = new()
    {
      new(EnvironmentVariables.DOCPROXYAWAITINGDISCHARGEREJECTIONREASONS, string.Empty),
      new(EnvironmentVariables.DOCPROXYCOMPLETEREJECTIONREASONS, string.Empty),
      new(
        EnvironmentVariables.DOCPROXYDELAYENDPOINT,
        EnvironmentVariables.DOCPROXYDELAYENDPOINTVALUE),
      new(EnvironmentVariables.DOCPROXYENPOINT, EnvironmentVariables.DOCPROXYENDPOINTVALUE),
      new(EnvironmentVariables.DOCPROXYGPCOMPLETE, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYGPDIDNOTCOMMENCE, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYGPDIDNOTCOMPLETE, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYGPFAILEDTOCONTACT, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYGPINVALIDCONTACTDETAILS, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYGPREJECTEDAFTERPROVIDERSELECTION, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYGPREJECTEDBEFOREPROVIDERSELECTION, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYGPDPCOMPLETEREJECTIONREASONS, string.Empty),
      new(EnvironmentVariables.DOCPROXYGPDPTRACEPATIENTREJECTIONREASONS, string.Empty),
      new(EnvironmentVariables.DOCPROXYGPDPUNABLETODISCHARGEREJECTIONREASONS, string.Empty),
      new(EnvironmentVariables.DOCPROXYMSKCOMPLETE, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYMSKDIDNOTCOMMENCE, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYMSKDIDNOTCOMPLETE, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYMSKFAILEDTOCONTACT, Guid.NewGuid().ToString()),
      new(EnvironmentVariables.DOCPROXYMSKINVALIDCONTACTDETAILS, Guid.NewGuid().ToString()),
      new(
        EnvironmentVariables.DOCPROXYMSKREJECTEDAFTERPROVIDERSELECTION,
        Guid.NewGuid().ToString()),
      new(
        EnvironmentVariables.DOCPROXYMSKREJECTEDBEFOREPROVIDERSELECTION,
        Guid.NewGuid().ToString()),
      new(
        EnvironmentVariables.DOCPROXYPOSTDISCHARGESLIMIT,
        EnvironmentVariables.DOCPROXYPOSTDISCHARGESLIMITVALUE),
      new(
        EnvironmentVariables.DOCPROXYPOSTENDPOINT,
        EnvironmentVariables.DOCPROXYPOSTENDPOINTVALULE),
      new(
        EnvironmentVariables.DOCPROXYRESOLVEENDPOINT,
        EnvironmentVariables.DOCPROXYRESOLVEENDPOINTVALULE),
      new(EnvironmentVariables.DOCPROXYTOKEN, EnvironmentVariables.DOCPROXYTOKENVALUE),
      new(EnvironmentVariables.DOCPROXYTRACEPATIENTREJECTIONREASONS, string.Empty),
      new(EnvironmentVariables.DOCPROXYUNABLETODISCHARGEPATIENTREJECTIONREASONS, string.Empty),
      new(
        EnvironmentVariables.DOCPROXYUPDATEENDPOINT,
        EnvironmentVariables.DOCPROXYUPDATEENDPOINTVALUE)
    };

    return options;
  }

  public static void SetDefaults()
  {
    Environment.SetEnvironmentVariable(Constants.REFERRAL_IN_TEST, "true");
  }
}