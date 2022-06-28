using System;
using WmsHub.Business.Enums;
using Xunit;

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
      ReferralStatus exclude)
    {
      return ReferralStatusesTheoryData(new ReferralStatus[]
      {
        exclude
      });
    }

    public static TheoryData<ReferralStatus> ReferralStatusesTheoryData(
      ReferralStatus[] excludedStatuses)
    {
      return EnumTheoryData(excludedStatuses);
    }

    public static TheoryData<ReferralSource> ReferralSourceTheoryData(
      ReferralSource exclude)
    {
      return ReferralSourceTheoryData(new ReferralSource[]
      {
        exclude
      });
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
  }
}

