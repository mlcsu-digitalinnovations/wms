using System;

namespace WmsHub.Common.Helpers;

public static class Constants
{
  public static class Actions
  {
    public const string CREATE = "Create";
    public const string CREATED = "Created";
    public const string DELETE = "Delete";
    public const string DELIVERED = "Delivered";
    public const string SENDING = "Sending";
  }

  public static class BusinessIntelligence
  {
    public const string AWAITINGTRIAGE = "Awaiting Triage";
    public static readonly string[] AWAITINGTRIAGESTATUSES = new string[]
    {
      "ChatBotCall1",
      "Exception",
      "New",
      "RmcCall",
      "RmcDelayed",
      "TextMessage1",
      "TextMessage2",
      "TextMessage3"
    };
    public const string INELIGIBLESTATUSREASON = "Language barrier";
    public const string NOTTRIAGED = "Not Triaged";
  }

  public static class CourseCompletion
  {
    public const string MINIMUMPOSSIBLESCORECOMPLETION =
      "MinimumPossibleScoreCompletion";

    public const string MAXIMUMPOSSIBLESCORECOMPLETION =
      "MaximumPossibleScoreCompletion";

    public const string MINIMUMPOSSIBLESCOREWEIGHT =
      "MinimumPossibleScoreWeight";

    public const string MAXIMUMPOSSIBLESCOREWEIGHT =
      "MaximumPossibleScoreWeight";

    public const string LOWCATEGORYLOWSCORECOMPLETION =
      "LowCategoryLowScoreCompletion";

    public const string MEDIUMCATEGORYLOWSCORECOMPLETION =
      "MediumCategoryLowScoreCompletion";

    public const string HIGHCATEGORYLOWSCORECOMPLETION =
      "HighCategoryLowScoreCompletion";

    public const string LOWCATEGORYHIGHSCORECOMPLETION =
      "LowCategoryHighScoreCompletion";

    public const string MEDIUMCATEGORYHIGHSCORECOMPLETION =
      "MediumCategoryHighScoreCompletion";

    public const string HIGHCATEGORYHIGHSCORECOMPLETION =
      "HighCategoryHighScoreCompletion";

    public const string LOWCATEGORYLOWSCOREWEIGHT =
      "LowCategoryLowScoreWeight";

    public const string MEDIUMCATEGORYLOWSCOREWEIGHT =
      "MediumCategoryLowScoreWeight";

    public const string HIGHCATEGORYLOWSCOREWEIGHT =
      "HighCategoryLowScoreWeight";

    public const string LOWCATEGORYHIGHSCOREWEIGHT =
      "LowCategoryHighScoreWeight";

    public const string MEDIUMCATEGORYHIGHSCOREWEIGHT =
      "MediumCategoryHighScoreWeight";

    public const string HIGHCATEGORYHIGHSCOREWEIGHT =
      "HighCategoryHighScoreWeight";

    public static string[] CourseCompletionList = new string[]
    {
      MINIMUMPOSSIBLESCORECOMPLETION,
      MAXIMUMPOSSIBLESCORECOMPLETION,
      MINIMUMPOSSIBLESCOREWEIGHT,
      MAXIMUMPOSSIBLESCOREWEIGHT,
      LOWCATEGORYLOWSCORECOMPLETION,
      MEDIUMCATEGORYLOWSCORECOMPLETION,
      HIGHCATEGORYLOWSCORECOMPLETION,
      LOWCATEGORYHIGHSCORECOMPLETION,
      MEDIUMCATEGORYHIGHSCORECOMPLETION,
      HIGHCATEGORYHIGHSCORECOMPLETION,
      LOWCATEGORYLOWSCOREWEIGHT,
      MEDIUMCATEGORYLOWSCOREWEIGHT,
      HIGHCATEGORYLOWSCOREWEIGHT,
      LOWCATEGORYHIGHSCOREWEIGHT,
      MEDIUMCATEGORYHIGHSCOREWEIGHT,
      HIGHCATEGORYHIGHSCOREWEIGHT
    };
  }
  public const int ACCESS_CODE_MAX_TRY_COUNT = 3;

  public static readonly string[] ALLOWED_SPREADSHEET_EXTENSIONS = 
    new[] { ".csv", ".xls", ".xlsx" };
  public const string CSV = ".csv";

  public const string SQLCONNSTR_WMSHUB = "SQLCONNSTR_WmsHub";
  public const string UNIT_TESTING = "UNIT_TESTING";
  public const string WMSHUB = "WmsHub";

  public const string REGEX_ALL_NON_ALPHA = @"^[^a-zA-Z]+$";

  public const string REGEX_PHONE_PLUS_NUMLENGTH = @"^\+[0-9]+$";
  public const string REGEX_NUMERIC_STRING = @"^[0-9]+$";

  public const string REGEX_MOBILE_PHONE_UK =
    @"^\+447(\d[0-9]{8})$";

  public const string REGEX_LANDLINE_PHONE_UK =
    @"^\+44([1-6,8,9])\d[0-9]{7,8}$";

  public const string REGEX_LANDLINE_HOME_UK =
    @"^\+44([1,2])\d[0-9]{7,8}$";

  public const string REGEX_FAMILYNAME = @"^[a-zA-Z\-’' ]+$";

  public const string REGEX_GIVENNAME = @"^[a-zA-Z\-’' ]+$";

  public const int MAX_SECONDS_API_REQUEST_AHEAD = 300;

  public const int HOURS_BEFORE_CHATBOTCALL1 = 48;
  public const int HOURS_BEFORE_NEXT_STAGE = 48;
  public const int HOURS_BEFORE_TEXTMESSAGE3 = 168;

  public const int LETTERSENT_GRACE_PERIOD = 14;

  public const string LINKIDCHARS = "23456789abcdefghijkmnpqrstuvwxyz";

  public const string MINIMUM_REQUEST_DATE = "2021-02-01";
  public const string MAXIMUM_REQUEST_DATE = "2121-02-01";
  public const string MINIMUM_DATE_OF_BIRTH = "1900-01-01";

  public const string UNKNOWN_GP_PRACTICE_NUMBER = "V81999";
  public const string UNKNOWN_GP_PRACTICE_NAME = "Unknown";
  public const string UNABLE_TO_TRACE_STATUS_REASON = 
    "Traced ODS code is unknown or same as previous value.";

  public static readonly string[] UNKNOWN_ODS_CODES = new[]
  {
    "V81997",
    "V81998",
    "V81999"
  };

  public static string DATE_OF_BIRTH_EXPIRY = "DoB Expiry";

  public const string REGEX_IPv4_ADDRESS =
    "((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.)" +
    "{3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";

  public const string REGEX_EMAIL_ADDRESS =
    @"^(?!.*\.\.)[^@\s]+(?<![\.\,])@(?!\.)[^@\s_]" + 
    @"+\.[^@\s\d_]+(?<!\-rka)+(?<!\-qka)+(?<!\.)$";

  public const string REGEX_GP_PRACTICE_NUMBER_ODS_CODE =
    @"^([A,B,C,D,E,F,G,H,J,K,L,M,N,P,Q,R,S,T,U,V,W,Y]\d{5}|(ALD|GUE|JER)\d{2})$";

  public const string TEXT_MESSAGE_SENT = "SENT";
  public const string TEXT_MESSAGE_FAILED = "FAILED";

  public const int MIN_GP_REFERRAL_AGE = 18;
  public const int MAX_GP_REFERRAL_AGE = 110;
  public const int MIN_SELF_REFERRAL_AGE = 18;
  public const int MAX_SELF_REFERRAL_AGE = 110;
  public const int MIN_PHARMACY_REFERRAL_AGE = 18;
  public const int MAX_PHARMACY_REFERRAL_AGE = 110;
  
  public const int MIN_HEIGHT_CM = 50;
  public const int MAX_HEIGHT_CM = 250;

  public const double MIN_HEIGHT_FEET = 1;
  public const double MAX_HEIGHT_FEET = 8;
  public const double MIN_HEIGHT_INCHES = 0;
  public const double MAX_HEIGHT_INCHES = 11.9999;

  public const int MIN_WEIGHT_KG = 35;
  public const int MAX_WEIGHT_KG = 500;

  public const double MIN_WEIGHT_STONES = 5;
  public const double MAX_WEIGHT_STONES = 78;
  public const double MIN_WEIGHT_POUNDS = 0;
  public const double MAX_WEIGHT_POUNDS = 13.9999;

  public const double MIN_BMI = 27.5d;
  public const double MAX_BMI = 90.0d;
  public const int MAX_EC_DAYS_ON_WAITING_LIST = 1095;
  public const int MAX_DAYS_BMI_DATE_AT_REGISTRATION = 730;

  public const int QUESTIONNAIRE_EXPIRY_DAYS = 28;

  public const StringSplitOptions SPLIT_TRIM_AND_REMOVE =
    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

  public static readonly string[] NHS_DOMAINS = new[]
    {"nhs.net", "nhs.uk"};

  public static string DO_NOT_CONTACT_EMAIL = "**DON'T CONTACT BY EMAIL**";
  public static string UNTRACEABLE_NHS_NUMBER = "UNTRACEABLE";

  public static string[] INVALID_EMAIL_DOMAINS = new[]
    {"@test.", "@foo.", "@bar.", "@blank.", "@self."};

  public static string[] INVALID_EMAIL_PREFIX = new[]
  {
    "nothing", "none", "n0n3", "qwerty", "self", "test"
  };

  public static string[] INVALID_EMAIL_TERMS = new[]
  {
    "invalid", "valid", "verified", "unverified", "confirmed", "unconfirmed",
    "home", "work", "other"
  };

  public static string OUTCOME_EXPIRED = "EXPIRED";
  public static string CALL_ATTEMPT_1 = "1";
  public static string CALL_ATTEMPT_2 = "2";

  public const string WMS_REFERRAL_ENV_ROUTE =
    "WmsHub_BusinessIntelligence_Api_";

  public const int MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION = 42;
  public const int MIN_DAYS_SINCE_DATESTARTEDPROGRAMME = 252;

  public const int MaxDaysAfterDateOfFirstContactToSendTextMessage3 = 35;
  public const int MAX_DAYS_UNTIL_FAILEDTOCONTACT = 42;

  public static char[] SPLITCHARS =>
    new char[] { ',', ';', '/', '\\', ' ' , '|' };

  public const string ErsOutcomeMessage =
    "The patient has been successfully referred to the NHS digital weight " +
    "management programme. This referral can now be removed from your " +
    "worklist using the Action: 'Cancel referral'. Please do not send it back into the service. " +
    "A discharge letter will be sent via Docman once the patient has completed the programme.";

  public static string SIGNINTYPE = "emailAddress";
  
  public const string XLS = ".xls";
  public const string XLSX = ".xlsx";

  public const string REFERRAL_IN_TEST = "WmsHub_Referral_ReferralsIsInTest";
  public const string POLICIES_IN_TEST = "WmsHub_Referral_PoliciesIsInTest";

  public const string ORGANISATION_NOT_SUPPORTED = 
    "Organisation not supported by Docman";

  public static class HttpMethod
  {
    public const string ACCESS_TOKEN = "access_token";
    public const string APPLICATIONURLENCODED = 
      "application/x-www-form-urlencoded";
    public const string BEARER = "Bearer";
    public const string CLIENT_ID = "client_id";
    public const string CLIENT_SECRET = "client_secret";
    public const string CONTENT_TYPE = "Content-Type";
    public const string EXPIRES_IN = "expires_in";
    public const string GET = "GET";
    public const string GRANT_TYPE = "grant_type";
    public const string POST = "POST";
    public const string SCOPE = "scope";
  }

  public static class WebUi
  {
    public const string RMC_CONTROLLER = "Rmc";
    public const string ENV_DEVELOPMENT = "Development";
    public const string ENV_STAGING = "Staging";
    public const string ENV_PRODUCTION = "Development";
    public const string RMC_AUTOMATED = "Automated System";
    public const string RMC_SERVICE_USER = "Service User";
    public const string RMC_UNKNOWN = "Unknown";
  }

  public static class NotificationPersonalisations
  {
    public const string CODE = "code";
    public const string GIVEN_NAME = "givenName";
    public const string LINK = "link";
    public const string NHS_NUMBER = "nhsNumber";
    public const string PROVIDER_COUNT = "providerCount";
    public const string PROVIDER_LIST = "providerList";
  }

  public static class MessageTemplateConstants
  {
    public const string ENDPOINT_SMS = "sms";
    public const string ENDPOINT_EMAIL = "email";
    public const string ORGANISATION_CODE = "organisationCode";
    public const string PASSWORD = "password";

    public const string TEMPLATE_DYNAMIC_SOURCE_REFERRAL_FIRST = "DynamicSourceReferralFirst";
    public const string TEMPLATE_DYNAMIC_SOURCE_REFERRAL_SECOND = "DynamicSourceReferralSecond";
    public const string TEMPLATE_DYNAMIC_SOURCE_REFERRAL_THIRD = "DynamicSourceReferralThird";

    public const string TEMPLATE_ELECTIVE_CARE_FIRST = "ElectiveCareFirst";
    public const string TEMPLATE_ELECTIVE_CARE_SECOMD = "ElectiveCareSecond";

    public const string TEMPLATE_FAILEDTOCONTACT_SMS = "FailedToContact";
    public const string TEMPLATE_FAILEDTOCONTACT_SERVICEUSER_EMAIL = "email";
    public const string TEMPLATE_FAILEDTOCONTACT_REFERRER_EMAIL = "email";

    public const string TEMPLATE_GENERAL_FIRST = "GeneralReferralFirst";
    public const string TEMPLATE_GENERAL_SECOND = "GeneralReferralSecond";

    public const string TEMPLATE_GP_FIRST = "GpReferralFirst";
    public const string TEMPLATE_GP_SECOND = "GpReferralSecond";

    public const string TEMPLATE_MSK_FIRST = "MskReferralFirst";
    public const string TEMPLATE_MSK_SECOND = "MskReferralSecond";

    public const string TEMPLATE_NONGP_DECLINED = "NonGpProviderDeclined";
    public const string TEMPLATE_NONGP_REJECTED = "NonGpProviderRejected";
    public const string TEMPLATE_NONGP_TERMINATED = "NonGpProviderTerminated";

    public const string TEMPLATE_NUMBERNOTMONITORED = "NumberNotMonitored";

    public const string TEMPLATE_PHARMACY_FIRST = "PharmacyReferralFirst";
    public const string TEMPLATE_PHARMACY_SECOND = "PharmacyReferralSecond";

    public const string TEMPLATE_PROVIDERS_BY_EMAIL = 
      "ProviderByEmailTemplateId";

    public const string TEMPLATE_SELF_CANCELLEDDUPLICATE =
      "StaffReferralCancelledDuplicate";
    public const string TEMPLATE_SELF_FIRST = "StaffReferralFirstMessage";
    public const string TEMPLATE_SELF_SECOND = "StaffReferralSecondMessage";
  }

  public static class NotificationProxyConstants
  {
    public const string PRODUCTION_KEY = "production-";
    public const string STAGING_KEY = "staging-";
    public const string DEVELOPMENT_KEY = "development-";
  }

  public static class MessageServiceConstants
  {
    public const string CONFIG_TEXT_TIME = "PrepareMessagesToSend";
    public const string KEY_EMAILS_QUEUED = "Emails Queued";
    public const string KEY_EXCEPTIONS = "Total number of exceptions";
    public const string KEY_EXCEPTIONS_MESSAGE = "Exceptions messages";
    public const string KEY_INFORMATION = "Information";
    public const string KEY_TEXT_QUEUED = "Text Messages Queued";
    public const string KEY_TOTAL_TO_SEND = "Total messages to send";
    public const string KEY_TOTAL_SENT = "Total messages sent";
    public const string KEY_VALIDATION_COUNT = "Validation Exception Count";
    public const string KEY_VALIDATION = "Validation";
  }

  public static class ProviderConstants
  {
    public const string ID_LIVA = "A7AFB83E-99C4-46A0-86B5-9CD688DD82CF";
    public const string ID_MORELIFE = "CE235712-395B-449D-9118-AF4736EE1844";
    public const string ID_OVIVA = "83D4F0C0-E010-4BEE-BE81-7E86AA9F48F6";
    public const string ID_SECONDNATURE = "05DA4135-9AD3-48B6-900C-FE31F4697835";
    public const string ID_SLIMMINGWORLD = "1BE04438-6D16-4924-BAA0-8F2A9DA415E6";
    public const string ID_XYLA = "2D11868B-6200-4C14-9F49-2A17D735A573";
  }

  public static class ReferralApiConstants
  {
    public const string INVALIDTERMINATIONREASON = "Termination reason is not valid.";
  }

  public static class WarningMessages
  {
    public const string INVALID_FILE_TYPE = "The eRS referral has an invalid" +
      " referral letter file type. Accepted types are .doc, .docx, .pdf and" +
      " .rtf; Non-Pdf files should be exportable as pdf.";
    public const string NO_ATTACHMENT =
      "The eRS referral does not have an attached referral letter.";

    public const string NHS_WORKLIST = "The NHS number in the eRS work list";
  }
}
