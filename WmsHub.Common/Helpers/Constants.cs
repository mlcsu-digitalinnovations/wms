namespace WmsHub.Common.Helpers
{
  public static class Constants
  {
    public const string REGEX_PHONE_PLUS_NUMLENGTH = @"^\+[0-9]+$";
    public const string REGEX_NUMERIC_STRING = @"^[0-9]+$";

    public const string REGEX_MOBILE_PHONE_UK =
      @"^\+447(\d[0-9]{8})$";

    public const string REGEX_LANDLINE_PHONE_UK =
      @"^\+44([1-6,8,9])\d[0-9]{7,8}$";

    public const int MAX_SECONDS_API_REQUEST_AHEAD = 300;

    public const int HOURS_BEFORE_NEXT_STAGE = 48;

    public const int LETTERSENT_GRACE_PERIOD = 14;

    public const string MINIMUM_REQUEST_DATE = "2021-02-01";
    public const string MAXIMUM_REQUEST_DATE = "2121-02-01";
    public const string MINIMUM_DATE_OF_BIRTH = "1900-01-01";

    public const string UNKNOWN_GP_PRACTICE_NUMBER = "V81999";
    public const string UNKNOWN_GP_PRACTICE_NAME = "Unknown";

    public static string DATE_OF_BIRTH_EXPIRY = "DoB Expiry";

    public const string REGEX_IPv4_ADDRESS =
      "((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.)" +
      "{3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";

    public const string REGEX_EMAIL_ADDRESS = 
      @"^(?!.*\.\.)[^@\s]+(?<!\.)@(?!\.)[^@\s]+\.[^@\s]+$";

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
    public const int MIN_WEIGHT_KG = 35;
    public const int MAX_WEIGHT_KG = 500;

    public const int MAX_DAYS_BMI_DATE_AT_REGISTRATION = 730;

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

    public const string MIN_DAYS_BETWEEN_TRACES = "MinDaysBetweenTraces";
    public const int DEFAULT_MIN_DAYS_BETWEEN_TRACES = 1;

    public const string MAX_DAYS_BETWEEN_TRACES = "MaxDaysBetweenTraces";
    public const int DEFAULT_MAX_DAYS_BETWEEN_TRACES = 30;

    public const string DAYS_BETWEEN_TRACES = "DaysBetweenTraces";
    public const int DEFAULT_DAYS_BETWEEN_TRACES = 7;


    public const string WMS_REFERRAL_ENV_ROUTE =
      "WmsHub_BusinessIntelligence_Api_";
  }
}
