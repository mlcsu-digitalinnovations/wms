using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WmsHub.Common.Attributes;

namespace WmsHub.Common.Helpers
{
  public static class Generators
  {
    const string UCASE_LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string NUMBERS = "0123456789";
    const string UCASE_LETTERS_AND_NUMBERS =
      "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static char GenerateCharacter(Random random, string text)
    {
      int index = random.Next(text.Length);
      return text[index];
    }

    public static string GenerateCreatedByUserId()
    {
      return Guid.NewGuid().ToString();
    }

    public static string GenerateAddress1(Random random)
    {
      return $"{random.Next(1, 300)} Test Street";
    }

    public static string GenerateEmail(Random random)
    {
      return $"mock{DateTime.Now.ToString("yyyyMMddHHmmssfffffff")}@mock.com";
    }

    public static Dictionary<string, string> GetMskHubs()
    {
      return new Dictionary<string, string>()
      {
        {"RY448","Hertfordshire Community Hospital Services"},
        {"R0A07","Wythenshawe Hospital"},
        {"NTPAD","St. Mary's Hospital"},
        {"RRE58","Sir Robert Peel Community Hospital"},
        {"NLX","Sirona Care & Health"},
        {"RVY38","Ormskirk & District General Hospital"},
        {"NR3","Nottingham Citycare Partnership"},
        {"RWK88","The Romford Road Centre"}
      };
    }

    public static string GenerateMskHubOdsCode(Random rnd)
    {
      Dictionary<string, string> mskHubs = GetMskHubs();
      return mskHubs.Keys.ToArray()[rnd.Next(0, mskHubs.Count)];
    }

    public static string GenerateNhsEmail(Random random)
    {
      return $"amelda.sample{DateTime.Now.ToString("yyyyMMddHHmmssfffffff")}" +
        "@nhs.net";
    }

    public static string GenerateEthnicity(Random random)
    {
      switch (random.Next(1, 6))
      {
        case 1:
          return "White";
        case 2:
          return "Asian";
        case 3:
          return "Black";
        case 4:
          return "Mixed";
        default:
          return "Other";
      }
    }

    public static DateTimeOffset? GeneratePastDate(Random rnd)
    {
      return DateTimeOffset.Now.AddDays(-rnd.Next(1, 101));
    }

    public static DateTimeOffset? GenerateDateOfBirth(Random rnd)
    {
      return DateTimeOffset.Now.AddYears(-rnd.Next(
        Constants.MIN_GP_REFERRAL_AGE,
        Constants.MAX_GP_REFERRAL_AGE));
    }

    public static decimal GenerateWeightKg(Random rnd)
    {
      return rnd.Next(Constants.MIN_WEIGHT_KG, Constants.MAX_WEIGHT_KG);
    }

    public static DateTimeOffset? GenerateDateOfBmiAtRegistration(Random rnd)
    {
      return DateTimeOffset.Now.AddDays(-rnd.Next(
        0,
        Constants.MAX_DAYS_BMI_DATE_AT_REGISTRATION));
    }

    public static decimal GenerateHeightCm(Random rnd)
    {
      return rnd.Next(Constants.MIN_HEIGHT_CM, Constants.MAX_HEIGHT_CM);
    }

    public static EthnicityGrouping GenerateEthnicityGrouping(Random random)
    {
      switch (random.Next(1, 19))
      {
        case 1:
          return new EthnicityGrouping(
            "Other", "Other ethnic group", "Any other ethnic group");
        case 2:
          return new EthnicityGrouping(
            "Asian", "Asian or Asian British", "Chinese");
        case 3:
          return new EthnicityGrouping(
            "Asian", "Asian or Asian British", "Indian");
        case 4:
          return new EthnicityGrouping(
            "Mixed",
            "Mixed or Multiple ethnic groups",
            "Any other Mixed or Multiple ethnic background");
        case 5:
          return new EthnicityGrouping(
            "Black", "Black, African, Caribbean or Black British", "African");
        case 6:
          return new EthnicityGrouping(
            "White", "White", "Gypsy or Irish Traveller");
        case 7:
          return new EthnicityGrouping(
            "Mixed",
            "Mixed or Multiple ethnic groups",
            "White and Black Caribbean");
        case 8:
          return new EthnicityGrouping("Other", "Other ethnic group", "Arab");
        case 9:
          return new EthnicityGrouping(
            "Black",
            "Black, African, Caribbean or Black British",
            "Any other Black, African or Caribbean background");
        case 10:
          return new EthnicityGrouping(
            "Asian", "Asian or Asian British", "Any other Asian background");
        case 11:
          return new EthnicityGrouping(
            "Black", "Black, African, Caribbean or Black British", "Caribbean");
        case 12:
          return new EthnicityGrouping(
            "Asian", "Asian or Asian British", "Bangladeshi");
        case 13:
          return new EthnicityGrouping(
            "White",
            "White",
            "English, Welsh, Scottish, Northern Irish or British");
        case 14:
          return new EthnicityGrouping(
            "Asian", "Asian or Asian British", "Pakistani");
        case 15:
          return new EthnicityGrouping(
            "Mixed", "Mixed or Multiple ethnic groups", "White and Asian");
        case 16:
          return new EthnicityGrouping(
            "Other",
            "I do not wish to Disclose my Ethnicity",
            "I do not wish to Disclose my Ethnicity");
        case 17:
          return new EthnicityGrouping("White", "White", "Irish");

        case 18:
          return new EthnicityGrouping(
            "White", "White", "Any other White background");
        default:
          return new EthnicityGrouping(
            "Mixed",
            "Mixed or Multiple ethnic groups",
            "White and Black African");
      }
    }

    public static decimal GenerateDocumentVersion(Random random)
    {
      return decimal.Parse(
        $"{GenerateCharacter(random, NUMBERS)}." +
        $"{GenerateCharacter(random, NUMBERS)}");
    }

    public static string GenerateGpPracticeNumber(Random random)
    {

      return $"M{GenerateCharacter(random, NUMBERS)}" +
        $"{GenerateCharacter(random, NUMBERS)}" +
        $"{GenerateCharacter(random, NUMBERS)}" +
        $"{GenerateCharacter(random, NUMBERS)}" +
        $"{GenerateCharacter(random, NUMBERS)}";
    }

    public static string GenerateIpAddress(Random random)
    {
      return $"{GenerateStringOfDigits(random, 3)}." +
        $"{GenerateStringOfDigits(random, 3)}." +
        $"{GenerateStringOfDigits(random, 3)}." +
        $"{GenerateStringOfDigits(random, 3)}";
    }

    public static string GenerateMobile(Random random)
    {
      return $"+447{GenerateStringOfDigits(random, 9)}";
    }


    public static string GenerateName(Random random, int len)
    {
      string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m",
        "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
      string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
      string Name = "";
      Name += consonants[random.Next(consonants.Length)].ToUpper();
      Name += vowels[random.Next(vowels.Length)];
      int b = 2;
      while (b < len)
      {
        Name += consonants[random.Next(consonants.Length)];
        b++;
        Name += vowels[random.Next(vowels.Length)];
        b++;
      }
      return Name;
    }

    public static string GenerateNhsNumber(
      Random random, string mustNotBeEqualTo = "")
    {

      IReadOnlyList<int> digits
        = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      List<int> selected = new List<int> { 9, 9, 9 };
      NhsNumberAttribute test = new NhsNumberAttribute();
      string nhsNumber = "";

      while (!test.IsValid(nhsNumber) || nhsNumber == mustNotBeEqualTo)
      {
        string number = "";
        for (int i = 1; i < 4; i++)
        {
          int digit = digits[random.Next(digits.Count)];
          number += digit.ToString();
          selected.Add(digit);
        }

        int total = selected.Sum();
        int remainder = total % 11;
        int checkDigit = 11 - remainder;

        nhsNumber = $"999{number}999{checkDigit}";
      }

      return nhsNumber;
    }

    public static string GenerateOdsCode(Random random)
    {
      string[] validFirstChars = new string[]
        { "A","B","C","D","E","F","G","H",
          "J","K","L","M","N",
          "P","Q","R","S","T","U","V","W",
          "Y","ALD","GUE","JER"};

      string odsCode =
        $"{validFirstChars[random.Next(validFirstChars.Length)]}12345"
        .Substring(0, 6);

      return odsCode;
    }

    public static string GeneratePharmacyOdsCode(Random random)
    {
      string[] validFirstChars = new string[]
      { "FA", "FB", "FC", "FD","FE", "FG","FH","FI","FJ","FK","FL","FM",
        "FN","FO","FP","FQ","FR","FS","FT","FU","FV","FW","FX","FY"};

      string odsCode =
        $"{validFirstChars[random.Next(validFirstChars.Length)]}" +
        $"{GenerateCharacter(random, UCASE_LETTERS_AND_NUMBERS)}" +
        $"{GenerateCharacter(random, UCASE_LETTERS_AND_NUMBERS)}" +
        $"{GenerateCharacter(random, UCASE_LETTERS_AND_NUMBERS)}";
      return odsCode;
    }

    public static string GeneratePostcode(Random random)
    {

      return $"{GenerateCharacter(random, UCASE_LETTERS)}" +
        $"{GenerateCharacter(random, UCASE_LETTERS)}" +
        $"{GenerateCharacter(random, NUMBERS)} " +
        $"{GenerateCharacter(random, NUMBERS)}" +
        $"{GenerateCharacter(random, UCASE_LETTERS)}" +
        $"{GenerateCharacter(random, UCASE_LETTERS)}";
    }

    public static string GenerateSex(Random random)
    {
      if (random.Next(1, 3) == 1)
        return "Female";
      else
        return "Male";
    }

    public static string GenerateStaffRole(Random random)
    {
      string[] roles =
      {
        "Administrative and clerical",
        "Allied Health Professional e.g. physiotherapist",
        "Ambulance staff",
        "Doctor",
        "Estates and porters",
        "Healthcare Assistant/Support worker",
        "Healthcare scientists",
        "Managerial",
        "Nursing and midwifery",
        "Other"
      };

      return roles[random.Next(roles.Length)];
    }
    public static string GenerateTelephone(Random random)
    {
      return $"+441{GenerateStringOfDigits(random, 9)}";
    }

    public static string GenerateStringOfDigits(Random random, int length)
    {
      string result = string.Empty;
      for (int i = 0; i < length; i++)
        result = string.Concat(result, random.Next(10).ToString());
      return result;
    }

    public static string GenerateUbrn(Random random)
    {
      return GenerateStringOfDigits(random, 12);
    }

    public static string GenerateSelfUbrn(Random random)
    {
      return $"SR{GenerateStringOfDigits(random, 10)}";
    }
    public static string GenerateUbrnGeneral(Random random)
    {
      return $"GR{GenerateStringOfDigits(random, 10)}";
    }

    public static string GenerateUrl(Random random)
    {
      return $"https://{GenerateName(random, 10)}." +
        $"{GenerateName(random, 10)}.com";
    }

    public static string GenerateApiKey(string source)
    {
      string keyPart =
        $"{Guid.NewGuid().ToString().Replace("_", "r2")}Zz1{source}";

      return GenerateHasValueFromString(keyPart);
    }

    public static string GenerateHasValueFromString(string source)
    {
      using (SHA256 sha256Hash = SHA256.Create())
      {
        string hash = GetHash(sha256Hash, source);


        if (VerifyHash(sha256Hash, source, hash))
          return hash;

        throw new ArgumentException("Hash values do not match");
      }
    }

    public static string GenerateKey(Random random)
    {

      IReadOnlyList<int> digits
        = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      string number = "";
      for (int i = 0; i < 8; i++)
      {
        int digit = digits[random.Next(digits.Count)];
        number += digit.ToString();
      }

      return number;
    }

    private static string GetHash(HashAlgorithm hashAlgorithm, string input)
    {
      byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
      StringBuilder sb = new StringBuilder();
      foreach (byte d in data)
      {
        sb.Append(d.ToString("x2"));
      }
      return sb.ToString();
    }

    // Verify a hash against a string.
    private static bool VerifyHash(HashAlgorithm hashAlgorithm,
      string input,
      string hash)
    {
      string hashOfInput = GetHash(hashAlgorithm, input);
      StringComparer comparer = StringComparer.OrdinalIgnoreCase;
      return comparer.Compare(hashOfInput, hash) == 0;
    }

    public static bool GenerateTrueFalse(Random random)
    {
      int i = random.Next(1, 99);
      if (i % 2 == 0)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public static bool? GenerateNullTrueFalse(Random random)
    {
      int i = random.Next(1, 99);
      if (i % 2 == 0 && i % 3 != 0)
      {
        return null;
      }
      else if (i % 3 == 0 && i % 2 != 0)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public static bool? GenerateNullFalse(Random random)
    {
      int i = random.Next(1, 99);
      if (i % 2 == 0)
      {
        return null;
      }
      else
      {
        return false;
      }
    }

    /// <summary>
    /// Generates a random code of 6 characters with upper case,
    /// lowercase, numbers and special characters
    /// </summary>
    /// <param name="random"></param>
    /// <returns>string</returns>
    public static string GenerateKeyCode(Random random)
    {
      return GenerateKeyCode(random, 6);
    }

    /// <summary>
    /// Generates a random code of 'n' characters with upper case,
    /// lowercase, numbers and special characters
    /// </summary>
    /// <param name="random"></param>
    /// <param name="size">int</param>
    /// <returns>string</returns>
    public static string GenerateKeyCode(Random random, int size)
    {
      return GenerateKeyCode(random, size, true, true);
    }

    /// <summary>
    /// Generates a random code of 'n' characters with upper case,
    /// lowercase, numbers with or without special characters
    /// </summary>
    /// <param name="random"></param>
    /// <param name="size">int</param>
    /// <param name="includeSpecialCharacters">bool</param>
    /// <param name="specialCharacterAtEnd">bool</param>
    /// <returns>string</returns>
    public static string GenerateKeyCode(Random random,
      int size,
      bool includeSpecialCharacters,
      bool specialCharacterAtEnd)
    {
      string _upperTest = "[A-Z]";
      string _lowerText = "[a-z]";
      string _numberTest = "[0-9]";
      string _specialTest = "[!#$%@<>?^~]";
      string code = "";
      int[] specials = new[] { 33, 35, 36, 37, 60, 62, 63, 64, 94, 126 };
      int minNumber = 48;
      int maxNumber = 57;
      int minLower = 97;
      int maxLower = 122;
      int minUpper = 65;
      int maxUpper = 90;

      int loopSize = size;
      if (includeSpecialCharacters && specialCharacterAtEnd)
        loopSize = size - 1;

      bool hasSpecial = false;

      while (code.Length < loopSize)
      {

        int i = random.Next(1000);

        MatchCollection numMatches = Regex.Matches(code, _numberTest);
        MatchCollection lowerMatches = Regex.Matches(code, _lowerText);
        MatchCollection upperMatches = Regex.Matches(code, _upperTest);
        MatchCollection specialMatches = Regex.Matches(code, _specialTest);

        int charCount = (int)Math.Ceiling((double)loopSize / 3);

        if (i % 3 == 0 && numMatches.Count < charCount)
        {
          int rnd = maxNumber - minNumber;
          code += (char)(minNumber + random.Next(rnd));
        }
        else if (i % 5 == 0 && lowerMatches.Count < charCount)
        {
          int rnd = maxLower - minLower;
          code += (char)(minLower + random.Next(rnd));

        }
        else if (i % 3 == 0 && i % 5 == 0 && upperMatches.Count < charCount)
        {
          int rnd = maxUpper - minUpper;
          code += (char)(minUpper + random.Next(rnd));
        }
        else if (
          includeSpecialCharacters && !hasSpecial && !specialCharacterAtEnd)
        {
          code += (char)specials[random.Next(specials.Length)];
          hasSpecial = true;
        }

      }

      if (includeSpecialCharacters && specialCharacterAtEnd)
      {
        code += (char)specials[random.Next(specials.Length)];
      }

      return code;
    }
  }
}
