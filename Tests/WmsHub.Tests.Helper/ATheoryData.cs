using System;
using System.Runtime.Serialization;
using WmsHub.Business.Enums;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Tests.Helper
{
  public abstract class ATheoryData : ABaseTests
  {
    protected const string MAXLENGTH = "The field {0} must be a string or " +
      "array type with a maximum length of '{1}'.";
    protected const string MINLENGTH = "The field {0} must be a string or " +
      "array type with a minimum length of '{1}'.";
    protected const string RANGE = "The field {0} must be between " +
      "{1} and {2}.";
    protected const string REQUIRED = "The {0} field is required.";

    public static TheoryData<string, string>
      MobileInvalidTelephoneInvalidData()
    {
      return new()
      {
        { null, null },
        { "", "" },
        { null, "" },
        { "", null },
        { MOBILE_INVALID_LONG, null },
        { MOBILE_INVALID_SHORT, null },
        { MOBILE_INVALID_LONG, "" },
        { MOBILE_INVALID_SHORT, "" },
        { null, TELEPHONE_INVALID_LONG },
        { null, TELEPHONE_INVALID_SHORT },
        { "", TELEPHONE_INVALID_LONG },
        { "", TELEPHONE_INVALID_SHORT }
      };
    }

    public static TheoryData<string, string>
      MobileInvalidTelephoneIsMobileData()
    {
      return new()
      {
        { null, MOBILE_E164 },
        { "", MOBILE_E164 },
        { null, MOBILE },
        { "", MOBILE },
        { MOBILE_INVALID_LONG, MOBILE_E164 },
        { MOBILE_INVALID_SHORT, MOBILE_E164 },
        { MOBILE_INVALID_LONG, MOBILE },
        { MOBILE_INVALID_SHORT, MOBILE },
      };
    }

    public static TheoryData<string, string>
      MobileInvalidTelephoneIsTelephoneData()
    {
      return new()
      {
        { null, TELEPHONE_E164 },
        { "", TELEPHONE_E164 },
        { null, TELEPHONE },
        { "", TELEPHONE },
        { MOBILE_INVALID_LONG, TELEPHONE_E164 },
        { MOBILE_INVALID_SHORT, TELEPHONE_E164 },
        { MOBILE_INVALID_LONG, TELEPHONE },
        { MOBILE_INVALID_SHORT, TELEPHONE },
      };
    }

    public static TheoryData<string, string>
      MobileIsTelephoneTelephoneInvalidData()
    {
      return new()
      {
        { TELEPHONE_E164, null },
        { TELEPHONE_E164, "" },
        { TELEPHONE, null },
        { TELEPHONE, "" },
        { TELEPHONE_E164, TELEPHONE_INVALID_LONG },
        { TELEPHONE_E164, TELEPHONE_INVALID_SHORT },
        { TELEPHONE, TELEPHONE_INVALID_LONG },
        { TELEPHONE, TELEPHONE_INVALID_SHORT },
      };
    }

    public static TheoryData<string, string> 
      MobileIsMobileTelephoneInvalidData()
    {
      return new()
      {
        { MOBILE_E164, null },
        { MOBILE_E164, "" },
        { MOBILE, null },
        { MOBILE, "" },
        { MOBILE_E164, TELEPHONE_INVALID_LONG },
        { MOBILE_E164, TELEPHONE_INVALID_SHORT },
        { MOBILE, TELEPHONE_INVALID_LONG },
        { MOBILE, TELEPHONE_INVALID_SHORT },
      };
    }

    public static TheoryData<string, string>
      MobileIsMobileTelephoneIsMobile()
    {
      return new()
      {
        { MOBILE_E164, TELEPHONE_MOBILE_E164 },
        { MOBILE_E164, TELEPHONE_MOBILE },
        { MOBILE, TELEPHONE_MOBILE_E164 },
        { MOBILE, TELEPHONE_MOBILE },
      };
    }

    public static TheoryData<string, string>
      MobileIsTelephoneTelephoneIsTelephone()
    {
      return new()
      {
        { MOBILE_TELEPHONE_E164, TELEPHONE_E164 },
        { MOBILE_TELEPHONE, TELEPHONE_E164 },
        { MOBILE_TELEPHONE_E164, TELEPHONE },
        { MOBILE_TELEPHONE, TELEPHONE },
      };
    }

    public static TheoryData<string, string> MaxLength(
      int max,
      string fieldName)
    {
      TheoryData<string, string> data = new();
      if (max > 1)
      {
        data.Add(
          new('x', (max == int.MaxValue ? int.MaxValue : max + 1)),
          string.Format(MAXLENGTH, fieldName, max));
      }
      return data;
    }

    public static TheoryData<string> NullOrWhiteSpaceTheoryData()
    {
      return new()
      {
        { null },
        { "" },
        { " " }
      };
    }

    public static TheoryData<string, string> MinMaxLengthNullOrEmpty(
      int min,
      int max,
      string fieldName)
    {
      TheoryData<string, string> data = new()
      {
        { "", string.Format(REQUIRED, fieldName) },
        { null, string.Format(REQUIRED, fieldName) },
      };
      if (min > 1)
      {
        data.Add(
          new('x', min - 1),
          string.Format(MINLENGTH, fieldName, max));
      }
      if (max > 1)
      {
        data.Add(
          new('x', (max == int.MaxValue ? int.MaxValue : max + 1)),
          string.Format(MAXLENGTH, fieldName, max));
      }
      return data;
    }

    public static TheoryData<Ethnicity> EthnicityTheoryData()
    {
      return EnumTheoryData(Array.Empty<Ethnicity>());
    }

    public static TheoryData<string> InvalidEmailAddressTheoryData()
    {
      return new TheoryData<string>()
      {
        "",
        null,
        "plainaddress",
        "#@%^%#$@#$@#.com",
        "@example.com",
        "Joe Smith <email@example.com>",
        "email.example.com",
        "email@example@example.com",
        "email.@example.com",
        "email..email@example.com",
        "email@example.com (Joe Smith)",
        "email@example",
        "email@example..com",
        "Abc..123@example.com"
      };
    }

    public static TheoryData<string> InvalidGpPracticeOdsCodeTheoryData()
    {
      return new TheoryData<string>()
      {
        "",
        null,
        "M",
        "M1",
        "M12",
        "M123",
        "M1234",
        "M123456",
        "I12345",
        "O12345",
        "X12345",
        "Z12345",
      };
    }

    public static TheoryData<string> UnkownGpPracticeOdsCodeTheoryData()
    {
      return new TheoryData<string>()
      {
        "V81997",
        "V81998",
        "V81999"
      };
    }

    public static TheoryData<string> InvalidNhsNumberTheoryData()
    {
      return new TheoryData<string>()
      {
        "",
        null,
        "123456789",
        "12345678901",
        "1234567890"
      };
    }

    public static TheoryData<Sex> SexTheoryData()
    {
      return EnumTheoryData(Array.Empty<Sex>());
    }

    public static TheoryData<ReferralStatus> ReferralStatusesTheoryData()
    {
      return ReferralStatusesTheoryData(Array.Empty<ReferralStatus>());
    }

    public static TheoryData<ReferralStatus> ReferralStatusesTheoryData(
      ReferralStatus[] excludedStatuses)
    {
      return EnumTheoryData(excludedStatuses);
    }

    public static TheoryData<ProgrammeOutcome> ProgrammeOutcomesTheoryData()
    {
      return EnumTheoryData(Array.Empty<ProgrammeOutcome>());
    }

    public static TheoryData<ProgrammeOutcome> ProgrammeOutcomesTheoryData(
      ProgrammeOutcome[] excludedOutcomes)
    {
      return EnumTheoryData(excludedOutcomes);
    }

    public static TheoryData<ReferralSource> ReferralSourceTheoryData()
    {
      return ReferralSourceTheoryData(Array.Empty<ReferralSource>());
    }

    public static TheoryData<ReferralSource> ReferralSourceTheoryData(
      ReferralSource[] excludedSources)
    {
      return EnumTheoryData(excludedSources);
    }

    public static TheoryData<T> EnumTheoryData<T>(T[] excludedSources)
      where T : Enum
    {
      var theoryData = new TheoryData<T>();
      foreach (var referralSource in Enum.GetValues(typeof(T)))
      {
        bool addStatus = true;
        foreach (var excludedSource in excludedSources)
        {
          if (((T)referralSource).Equals(excludedSource))
          {
            addStatus = false;
            break;
          }
        }

        if (addStatus)
        {
          theoryData.Add((T)referralSource);
        }
      }

      return theoryData;
    }

    public static TheoryData<string, ReferralAuditData> AuditStatuses()
    {
      TheoryData<string, ReferralAuditData> data = new()
      {
        {
          "E5DC0466-E788-4EC9-ACE6-F264B9C23191",
          new ReferralAuditData
          {
            Modified = 11,
            Status = new[]
          {
            "New",
            "ChatBotCall1",
            "TextMessage2",
            "ChatBotCall1",
            "ChatBotCall1",
            "RmcDelayed",
            "RmcDelayed",
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "1FD2B133-6A15-4171-8B30-C10D4B172AAA",
          new ReferralAuditData
          {
            Modified = 9,
            Status = new[]
          {
            "New",
            "ChatBotCall1",
            "TextMessage2",
            "ChatBotCall1",
            "ChatBotCall2",
            "ChatBotCall2",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "76484364-B6EC-49D5-910D-D0FDDAC92D46",
          new ReferralAuditData
          {
            Modified = 7,
            Status = new[]
          {
            "New",
            "ChatBotCall1",
            "TextMessage2",
            "ChatBotCall1",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "42CCB653-3161-4360-A4B1-6C5E77C26A31",
          new ReferralAuditData
          {
            Modified = 7,
            Status = new[]
          {
            "New",
            "ChatBotCall1",
            "TextMessage2",
            "RejectedToEreferrals",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "659E1940-F8D6-4582-9672-2D66E688427D",
          new ReferralAuditData
          {
            Modified = 6,
            Status = new[]
          {
            "New",
            "New",
            "New",
            "New",
            "RejectedToEreferrals",
          },
            Expected = new ExpectedStatus()
          }
        },
        {
          "D0992CAC-9A1D-40C6-A0DD-248DB1EF799F",
          new ReferralAuditData
          {
            Modified = 8,
            Status = new[]
          {
            "New",
            "New",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "Exception",
            "RejectedToEreferrals",
          },
            Expected = new ExpectedStatus()
          }
        },
        {
          "BFEFF135-D186-4BDA-A18C-AFF9E52A81A6",
          new ReferralAuditData
          {
            Modified = 10,
            Status = new[]
          {
            "New",
            "New",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "RejectedToEreferrals",
          },
            Expected = new ExpectedStatus()
          }
        },
        {
          "03581F64-3379-4AC2-ACFB-C5D11EBE3507",
          new ReferralAuditData
          {
            Modified = 11,
            Status = new[]
          {
            "New",
            "New",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "RejectedToEreferrals",
            "RejectedToEreferrals",
          },
            Expected = new ExpectedStatus()
          }
        },
        {
          "03F0AD41-6AE1-4BBE-9B49-C0FB190AFEF1",
          new ReferralAuditData
          {
            Modified = 15,
            Status = new[]
          {
            "New",
            "New",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "ProviderAwaitingStart",
            "ProviderAccepted",
            "ProviderContactedServiceUser",
            "ProviderDeclinedByServiceUser",
            "RejectedToEreferrals",
          },
            Expected = new ExpectedStatus()
          }
        },
        {
          "6C8C6957-1B95-4C9C-9BAC-CCEDCCD4D88F",
          new ReferralAuditData
          {
            Modified = 14,
            Status = new[]
          {
            "New",
            "New",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "ProviderAwaitingStart",
            "ProviderAccepted",
            "ProviderDeclinedByServiceUser",
            "RejectedToEreferrals",
          },
            Expected = new ExpectedStatus()
          }
        },
        {
          "28EEF98E-B1DC-4FC9-B5E0-A47FA7A54AB6",
          new ReferralAuditData
          {
            Modified = 4,
            Status = new[]
          {
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "822905D8-51C0-4F59-BDB6-903EA6DC08CB",
          new ReferralAuditData
          {
            Modified = 11,
            Status = new[]
          {
            "RmcCall",
            "RmcCall",
            "RmcCall",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "27332BA6-D2E9-434E-AE18-50B455B5EB1E",
          new ReferralAuditData
          {
            Modified = 8,
            Status = new[]
          {
            "RmcCall",
            "RmcCall",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "6E29951D-C35D-40B9-BE14-ED3C8AD23D36",
          new ReferralAuditData
          {
            Modified = 10,
            Status = new[]
          {
            "RmcCall",
            "RmcCall",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "08586CB3-2795-4AAE-86E4-E847590C5EA5",
          new ReferralAuditData
          {
            Modified = 12,
            Status = new[]
          {
            "RmcCall",
            "RmcCall",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "487B56B9-22C1-49E3-B856-DCD2AC793FF5",
          new ReferralAuditData
          {
            Modified = 16,
            Status = new[]
          {
            "RmcCall",
            "RmcCall",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "9D3EC32B-FB07-4A96-9CF4-D3B52D34AE19",
          new ReferralAuditData
          {
            Modified = 11,
            Status = new[]
          {
            "RmcCall",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "ProviderAwaitingStart",
            "ProviderContactedServiceUser",
            "ProviderStarted",
            "ProviderTerminated",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "D1454B75-B5BC-4A13-8024-3E45D6E44161",
          new ReferralAuditData
          {
            Modified = 9,
            Status = new[]
          {
            "RmcCall",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "CCB54133-B459-4EFE-9FF5-1B8F1B300474",
          new ReferralAuditData
          {
            Modified = 11,
            Status = new[]
          {
            "RmcCall",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "Exception",
            "RejectedToEreferrals",
            "New",
          },
            Expected = new ExpectedStatus()
            { Reprocessed = true, SuccessfullyReprocessed = true }
          }
        },
        {
          "E411F4C2-B93F-493C-A59C-5BA87592DE1B",
          new ReferralAuditData
          {
            Modified = 13,
            Status = new[]
          {
            "RmcCall",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
          },
            Expected = new ExpectedStatus() { CurrentlyCancelled = true }
          }
        },
        {
          "4E4E47F0-3C62-4D22-ABA7-F19676E2A565",
          new ReferralAuditData
          {
            Modified = 16,
            Status = new[]
          {
            "Exception",
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RejectedToEreferrals",
            "New",
            "TextMessage2",
            "TextMessage2",
            "ChatBotCall2",
            "RmcCall",
            "RmcCall",
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "New",
          },
            Expected = new ExpectedStatus()
            {
              Reprocessed = true,
              SuccessfullyReprocessed = true,
              Uncancelled = true
            }
          }
        },
        {
          "C790CE1F-BE52-40AA-BF08-276A9038D199",
          new ReferralAuditData
          {
            Modified = 5,
            Status = new[]
          {
            "Exception", "Exception", "RejectedToEreferrals,RmcCall"
          },
            Expected = new ExpectedStatus()
            { Reprocessed = true, SuccessfullyReprocessed = true }
          }
        },
        {
          "CE4176DE-7337-457C-B514-F021594C5372",
          new ReferralAuditData
          {
            Modified = 27,
            Status = new[]
          {
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RejectedToEreferrals",
            "New",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage2",
            "TextMessage2",
            "TextMessage2",
            "TextMessage2",
            "TextMessage2",
            "TextMessage2",
            "TextMessage2",
            "ProviderAwaitingStart",
            "ProviderContactedServiceUser",
            "ProviderDeclinedByServiceUser",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "New"
          },
            Expected = new ExpectedStatus()
            {
              Reprocessed = true,
              SuccessfullyReprocessed = true,
              Uncancelled = true
            }
          }
        },
        {
          "1515A5D5-D8A5-4A64-AA39-7D6C6E9DD67D",
          new ReferralAuditData
          {
            Modified = 16,
            Status = new[]
          {
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RejectedToEreferrals",
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RmcCall"
          },
            Expected = new ExpectedStatus()
            {
              Reprocessed = true,
              SuccessfullyReprocessed = true,
              Uncancelled = true
            }
          }
        },
        {
          "96626ACF-242A-4563-9068-0DC8ACB336FD",
          new ReferralAuditData
          {
            Modified = 13,
            Status = new[]
          {
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RejectedToEreferrals",
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "Exception"
          },
            Expected = new ExpectedStatus()
            {
              Reprocessed = true
            }
          }
        },
        {
          "70C3C8A4-1F45-419D-858D-B3D25EF8CCE1",
          new ReferralAuditData
          {
            Modified = 28,
            Status = new[]
          {
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RejectedToEreferrals",
            "New",
            "New",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage2",
            "TextMessage2",
            "TextMessage2",
            "ChatBotCall1",
            "ChatBotCall1",
            "ChatBotCall2",
            "ChatBotCall2",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RmcCall"
          },
            Expected = new ExpectedStatus()
            {
              Reprocessed = true,
              SuccessfullyReprocessed = true,
              Uncancelled = true
            }
          }
        },
        {
          "DF5AA2F9-BF26-4B1E-9F47-ACC33157E350",
          new ReferralAuditData
          {
            Modified = 28,
            Status = new[]
          {
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RejectedToEreferrals",
            "New",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage1",
            "TextMessage2",
            "TextMessage2",
            "TextMessage2",
            "ChatBotCall1",
            "ChatBotCall1",
            "ChatBotCall2",
            "ChatBotCall2",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RmcDelayed",
            "RmcCall",
            "RejectedToEreferrals"
          },
            Expected = new ExpectedStatus()
            {
              Reprocessed = true,
              SuccessfullyReprocessed = true,
              Uncancelled = true
            }
          }
        },
        {
          "BBAD298C-5816-45D6-A0A8-D33851CB2A7A",
          new ReferralAuditData
          {
            Modified = 10,
            Status = new[]
          {
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RejectedToEreferrals",
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "Exception"
          },
            Expected = new ExpectedStatus()
            {
              Reprocessed = true
            }
          }
        },
        {
          "F077313C-02FD-4F35-BBF4-1188E271B171",
          new ReferralAuditData
          {
            Modified = 15,
            Status = new[]
          {
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RejectedToEreferrals",
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RejectedToEreferrals",
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals"
          },
            Expected = new ExpectedStatus()
            {
              Reprocessed = true,
              CurrentlyCancelled = true
            }
          }
        },
        {
          "1C490C94-EC19-4379-A6C9-90FFE8E427DC",
          new ReferralAuditData
          {
            Modified = 13,
            Status = new[]
          {
            "Exception",
            "Exception",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RejectedToEreferrals",
            "New",
            "New",
            "ChatBotCall1",
            "TextMessage2",
            "RejectedToEreferrals",
            "RejectedToEreferrals",
            "CancelledByEreferrals",
            "RmcCall"
          },
            Expected = new ExpectedStatus()
            {
              Reprocessed = true,
              SuccessfullyReprocessed = true,
              Uncancelled = true
            }
          }
        }
      };
      return data;
    }

    public static TheoryData<string> ReprocessedRoute()
    {
      var td = new TheoryData<string>();
      var s = AuditStatuses();
      foreach (var dv in s)
      {
        if (((ReferralAuditData)dv[1]).Expected.Reprocessed)
        {
          td.Add(string.Join("->", ((ReferralAuditData)dv[1]).Status));
        }
      }

      return td;
    }

    public static TheoryData<string> ReprocessedRouteFalse()
    {
      var td = new TheoryData<string>();
      var s = AuditStatuses();
      foreach (var dv in s)
      {
        if (!((ReferralAuditData)dv[1]).Expected.Reprocessed)
        {
          td.Add(string.Join("->", ((ReferralAuditData)dv[1]).Status));
        }
      }

      return td;
    }

    public static TheoryData<string> SuccessfullyReprocessedRoute()
    {
      var td = new TheoryData<string>();
      var s = AuditStatuses();
      foreach (var dv in s)
      {
        if (((ReferralAuditData)dv[1]).Expected.SuccessfullyReprocessed)
        {
          td.Add(string.Join("->", ((ReferralAuditData)dv[1]).Status));
        }
      }

      return td;
    }

    public static TheoryData<string> SuccessfullyReprocessedRouteFalse()
    {
      var td = new TheoryData<string>();
      var s = AuditStatuses();
      foreach (var dv in s)
      {
        if (!((ReferralAuditData)dv[1]).Expected.SuccessfullyReprocessed)
        {
          td.Add(string.Join("->", ((ReferralAuditData)dv[1]).Status));
        }
      }

      return td;
    }

    public static TheoryData<string> UncancelledRoute()
    {
      var td = new TheoryData<string>();
      var s = AuditStatuses();
      foreach (var dv in s)
      {
        if (((ReferralAuditData)dv[1]).Expected.Uncancelled)
        {
          td.Add(string.Join("->", ((ReferralAuditData)dv[1]).Status));
        }
      }

      return td;
    }

    public static TheoryData<string> UncancelledRouteFalse()
    {
      var td = new TheoryData<string>();
      var s = AuditStatuses();
      foreach (var dv in s)
      {
        if (!((ReferralAuditData)dv[1]).Expected.Uncancelled)
        {
          td.Add(string.Join("->", ((ReferralAuditData)dv[1]).Status));
        }
      }

      return td;
    }

    public static TheoryData<string> CurrentlyCancelledRoute()
    {
      var td = new TheoryData<string>();
      var s = AuditStatuses();
      foreach (var dv in s)
      {
        if (((ReferralAuditData)dv[1]).Expected.CurrentlyCancelled)
        {
          td.Add(string.Join("->", ((ReferralAuditData)dv[1]).Status));
        }
      }

      return td;
    }

    public static TheoryData<string> CurrentlyCancelledRouteFalse()
    {
      var td = new TheoryData<string>();
      var s = AuditStatuses();
      foreach (var dv in s)
      {
        if (!((ReferralAuditData)dv[1]).Expected.CurrentlyCancelled)
        {
          td.Add(string.Join("->", ((ReferralAuditData)dv[1]).Status));
        }
      }

      return td;
    }

    public static TheoryData<string,string> InitialStatusRoute()
    {
      var td = new TheoryData<string, string>();
      var s = AuditStatuses();
      foreach (var dv in s)
      {
        td.Add(((ReferralAuditData)dv[1]).Status[0],
          string.Join("->", ((ReferralAuditData)dv[1]).Status));
      }

      return td;
    }

    public static TheoryData<string, string> SqlInjectionData()
    {
      return new()
      {
        {"''", "''"},
        {"@'This Is A Test'", "'This Is A Test'"},
        {"@", ""},
        {"@@variable", "variable" },
        {"-1' UNION SELECT 1,2,3--+", "1' UNION SELECT 1,2,3--+" },
        {"xtype = 'U' and name > '.') --", "xtype = 'U' and name  '.') --" },
        {"-- or #", " or #" },
        {"%00", "00" },
        {"+", "" },
        {"%", "" },
        {"AND true", "AND true" },
        {"AND false", "AND false" },
        {"!\"$%^&*()<>", "!\"&()"},
        {"=HYPERLINK", "HYPERLINK"},
        {"==HYPERLINK", "HYPERLINK"},
        {"@=HYPERLINK", "HYPERLINK"},
        {"-=HYPERLINK", "HYPERLINK"},
        {"@HYPERLINK", "HYPERLINK"},
        {"+=HYPERLINK", "HYPERLINK"}
      };
    }

    public static TheoryData<string, int> DatesOfBirth()
    {
      TheoryData<string, int> data = new();

      for (var i = 1; i <= 12; i++)
      {
        for (var j = 25; j <= 50; j++)
        {
          int expectedAge = j;
          int currentMonth = DateTime.UtcNow.Month;
          int year = DateTime.UtcNow.AddYears(-j).Year;

          if (i > currentMonth)
          {
            expectedAge -= 1;
          }
          if (DateTime.UtcNow.Day > 27)
          {
            data.Add($"28/{i:##}/{year}", expectedAge);
          }
          else
          {
            data.Add($"{DateTime.UtcNow.Day:##}/{i:##}/{year}", expectedAge);
          }
        }
      }

      return data;
    }

    public static TheoryData<string, bool> DatesBool()
    {
      return new()
      {
        {"1996-11-01", false },
        {"1996-12-01", false },
        {"", true },
        {null, true }
      };
    }

    public static TheoryData<string, string> CanBeConvertedPhoneNumber()
    {
      return new()
      {
        {"0111111111", "+44111111111"},
        {"0211111111", "+44211111111"},
        {"0311111111", "+44311111111"},
        {"0411111111", "+44411111111"},
        {"0511111111", "+44511111111"},
        {"0611111111", "+44611111111"},
        {"0811111111", "+44811111111"},
        {"0911111111", "+44911111111"},
        {"+44111111111", "+44111111111"},
        {"+44211111111", "+44211111111"},
        {"+44311111111", "+44311111111"},
        {"+44411111111", "+44411111111"},
        {"+44511111111", "+44511111111"},
        {"+44611111111", "+44611111111"},
        {"+44811111111", "+44811111111"},
        {"+44911111111", "+44911111111"},
        {"01111111111", "+441111111111"},
        {"02111111111", "+442111111111"},
        {"03111111111", "+443111111111"},
        {"04111111111", "+444111111111"},
        {"05111111111", "+445111111111"},
        {"06111111111", "+446111111111"},
        {"08111111111", "+448111111111"},
        {"09111111111", "+449111111111"},
        {"+441111111111", "+441111111111"},
        {"+442111111111", "+442111111111"},
        {"+443111111111", "+443111111111"},
        {"+444111111111", "+444111111111"},
        {"+445111111111", "+445111111111"},
        {"+446111111111", "+446111111111"},
        {"+448111111111", "+448111111111"},
        {"+449111111111", "+449111111111"},
        {"09111 111111 ", "+449111111111"},
        {"(09111) 111111 ", "+449111111111"},
        {"09111-111111 ", "+449111111111"}
      };
    }

    public static TheoryData<string> CannotBeConvertedPhoneNumber()
    {
      return new()
      {
        {"0"},
        {"01"},
        {"011"},
        {"0111"},
        {"01111"},
        {"011111"},
        {"0111111"},
        {"01111111"},
        {"011111111"},
        {"0711111111"},
        {"07111111111"},
        {"00111111111"},
        {"011111111111"},
        {"0111111111111"},
        {"not a number"},
        {"+not a number"}
      };
    }

    public static TheoryData<string, bool> UkNumbers()
    {
      return new()
      {
        {"+4401752888999", false },
        {"01752888999", false },
        {"+441752888999", true }
      };
    }

    public static TheoryData<string> AwaitingDischargeRejectionReasons()
    {
      return new()
      {
        "Failed to Retrieve File (7006)",
        "Failed to Retrieve File (7007)",
        "MESH acknowledgement not at correct HTTP status, see error ID"
      };
    }

    public static TheoryData<ReferralSource, string> DischargeAwaitingTraceRejectionReasons()
    {
      return new()
      {
        {  ReferralSource.ElectiveCare, "REJ01" },
        {  ReferralSource.GeneralReferral, "REJ01" },
        {  ReferralSource.GpReferral, "REJ01" },
        {  ReferralSource.Msk, "REJ01" },
        {  ReferralSource.Pharmacy, "REJ01" },
        {  ReferralSource.SelfReferral, "REJ01" },
        {  ReferralSource.ElectiveCare, "REJ02" },
        {  ReferralSource.GeneralReferral, "REJ02" },
        {  ReferralSource.GpReferral, "REJ02" },
        {  ReferralSource.Msk, "REJ02" },
        {  ReferralSource.Pharmacy, "REJ02" },
        {  ReferralSource.SelfReferral, "REJ02" },
        {  ReferralSource.ElectiveCare, "GPDPREJ01" },
        {  ReferralSource.GeneralReferral, "GPDPREJ01" },
        {  ReferralSource.GpReferral, "GPDPREJ01" },
        {  ReferralSource.Msk, "GPDPREJ01" },
        {  ReferralSource.Pharmacy, "GPDPREJ01" },
        {  ReferralSource.SelfReferral, "GPDPREJ01" },
        {  ReferralSource.ElectiveCare, "GPDPREJ02" },
        {  ReferralSource.GeneralReferral, "GPDPREJ02" },
        {  ReferralSource.GpReferral, "GPDPREJ02" },
        {  ReferralSource.Msk, "GPDPREJ02" },
        {  ReferralSource.Pharmacy, "GPDPREJ02" },
        {  ReferralSource.SelfReferral, "GPDPREJ02" }
      };
    }

    public static TheoryData<string> CompleteRejectionReasons()
    {
      return new()
      {
        "REJ05",
        "GPDPREJ05"
      };
    }

    public static TheoryData<string> UnableToDischargeRejectionReasons()
    {
      return new()
      {
        "REJ03",
        "REJ04",
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
        "GPDPREJ03",
        "GPDPREJ04",
        "GPDPREJ06",
        "GPDPREJ07",
        "GPDPREJ08"
      };
    }

    public static TheoryData<ReferralStatus, ReferralSource>
      TerminateNotStartedProgrammeReferralsTheoryData()
    {
      return new()
      {
        { ReferralStatus.ProviderAccepted, ReferralSource.ElectiveCare },
        { ReferralStatus.ProviderAccepted, ReferralSource.GeneralReferral },
        { ReferralStatus.ProviderAccepted, ReferralSource.Msk },
        { ReferralStatus.ProviderAccepted, ReferralSource.Pharmacy },
        { ReferralStatus.ProviderAccepted, ReferralSource.SelfReferral },
        { ReferralStatus.ProviderContactedServiceUser, ReferralSource.ElectiveCare },
        { ReferralStatus.ProviderContactedServiceUser, ReferralSource.GeneralReferral },
        { ReferralStatus.ProviderContactedServiceUser, ReferralSource.Msk },
        { ReferralStatus.ProviderContactedServiceUser, ReferralSource.Pharmacy },
        { ReferralStatus.ProviderContactedServiceUser, ReferralSource.SelfReferral }
      };
    }
  }

  public class ReferralAuditData : IXunitSerializable
  {
    public ExpectedStatus Expected { get; set; }
    public int Modified { get; set; }
    public string[] Status { get; set; }    

    public void Deserialize(IXunitSerializationInfo info)
    {
      Expected = info.GetValue<ExpectedStatus>(nameof(Expected));
      Modified = info.GetValue<int>(nameof(Modified));
      Status = info.GetValue<string[]>(nameof(Status));
    }

    public void Serialize(IXunitSerializationInfo info)
    {
      info.AddValue(nameof(Expected), Expected);
      info.AddValue(nameof(Modified), Modified);
      info.AddValue(nameof(Status), Status);
    }
  }

  public class ExpectedStatus : IXunitSerializable
  {
    public bool CurrentlyCancelled { get; set; }
    public bool Reprocessed { get; set; }
    public bool SuccessfullyReprocessed { get; set; }
    public bool Uncancelled { get; set; }

    public void Deserialize(IXunitSerializationInfo info)
    {
      CurrentlyCancelled = info.GetValue<bool>(nameof(CurrentlyCancelled));
      Reprocessed = info.GetValue<bool>(nameof(Reprocessed));
      SuccessfullyReprocessed = info.GetValue<bool>(nameof(SuccessfullyReprocessed));
      Uncancelled = info.GetValue<bool>(nameof(Uncancelled));
    }

    public void Serialize(IXunitSerializationInfo info)
    {
      info.AddValue(nameof(CurrentlyCancelled), CurrentlyCancelled);
      info.AddValue(nameof(Reprocessed), Reprocessed);
      info.AddValue(nameof(SuccessfullyReprocessed), SuccessfullyReprocessed);
      info.AddValue(nameof(Uncancelled), Uncancelled);
    }
  }
}

