using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.ChatBot.Api.Models
{

  public static class TestArcusSetupModel
  {
    private static readonly Guid TEST_REFERRAL_API_USER_ID =
      new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41");
    public static readonly Guid CHATBOT_API_USER_ID =
      new Guid("eafc7655-89b7-42a3-bdf7-c57c72cd1d41");

    private static readonly string[] UBRN_LIST = new string[]
    {
      "58E55433-AF74-43DD-BA50-FE9E6620CDF1",
      "F3C6F179-D0E3-44DD-8809-99B301FFDE8F",
      "50522DE4-206B-49EA-8070-00CCAF8185E6",
      "B82BC3A2-0E85-4FB5-91D3-32E8E7A10C38",
      "DE7A7BB7-36C9-49A4-A7DF-C3BBC901AC93",
      "C974C601-06E5-49A5-9DE6-3AD0ADDE4003",
      "CC9E65D1-635F-4940-8FA7-66FD7764A74A",
      "FF9B69FE-BE34-4336-865E-E27A6A9115E2",
      "DD6EC29D-18B8-47A1-8EF8-962912A1B6F1",
      "1D0EB493-252A-4BB4-B7C6-A8B8BE470112",
      "F37E9F47-11F3-4EF0-9ED3-33922D837388",
      "CA73913E-E446-4BE6-9B96-F8E1CD1B560B",
      "8C41DFBE-1B90-4579-8D1D-2B3ECFB9C230",
      "0E1E0AAC-E7CB-482F-8BB9-506F903A01BA",
      "973FDB7C-DF84-4817-BE72-8CF0358B2A91",
      "477F89DC-7867-4F29-84D7-6C95933E5BF1",
      "146A3484-2FF1-4B6B-9E5D-E174CB1B3A75",
      "481D2657-FD09-4970-9B70-13F2567244D6",
      "C68E6501-2CF3-4992-8C82-635CF13A7A61",
      "B9648E40-DE34-4695-9766-27540A2A4EE6"
    };

    private static readonly string[] STATUS_REASON = new string[]
    {
      "TEST_D5889E4F-334D-4712-841D-5483385F9CF0",
      "TEST_3821AD48-E420-492D-9F48-7A4B4A3402F4",
      "TEST_195BDFB0-89AA-454F-9D33-89383AF2B2A6",
      "TEST_8CB20D4F-2BF1-4E70-BA56-8A2115CD60B5",
      "TEST_38526A64-7936-49F0-BC0E-7B28E7F6AA5C",
      "TEST_5E730241-C55E-4C7B-BEBD-FE386C206D64",
      "TEST_A97C7671-CE71-4CCF-9766-0FA0F8B23284",
      "TEST_A4C31046-1FD7-43F1-9282-8BC249215157",
      "TEST_FFE72F81-285C-4B0C-BD24-F11035DFAFC8",
      "TEST_95DAB9EF-C9DD-458D-9935-49D794FF5834",
      "TEST_F2CFFE8A-215C-4D60-927D-06170428F40F",
      "TEST_6C7E75C2-F13B-4436-B92C-7AB30BED1D11",
      "TEST_C512CE5E-EA19-4312-8A19-4A804FC2C4AA",
      "TEST_00156598-A16B-4EAA-84B7-298A9389DBE4",
      "TEST_C1A05936-4435-4E1C-BFB7-E1CB769C5088",
      "TEST_FAF722AA-6725-4117-8A38-CB9A391D51B3",
      "TEST_1A29B392-C696-4357-A5E6-5CA7ADB47454",
      "TEST_EDC84E52-9FAC-4412-BB0D-3BF4DBD90ECB",
      "TEST_EF089A43-3F18-4C04-ADF5-740FD0C0B594",
      "TEST_5F1EBFF5-8F94-4AE1-8EFA-A3300CFDC477"
    };

    private static readonly string[] CALL_NUMBERS = new string[]
    {
      "+442080681039",
      "+442080681204",
      "+442080681230",
      "+442080681249" ,
      "+442080681330" ,
      "+442080681336" ,
      "+442080681348" ,
      "+442080681349" ,
      "+442080681352" ,
      "+442080681353" ,
      "+442080681360" ,
      "+442080681362"
    };

    [Required]
    [MinLength(1)]
    [MaxLength(20)]
    public static List<Business.Entities.Referral> Referrals
    {
      get
      {
        var referrals = new List<Business.Entities.Referral>();
        var count = 0;
        foreach (var number in CALL_NUMBERS)
        {
          count++;
          var nhsnumber = 1111111110 + count;
          var isMobile = number.StartsWith("07") || number.StartsWith("+447");
          referrals.Add( new Business.Entities.Referral()
          {
            Address1 = $"Address1_{count}",
            Address2 = $"Address2_{count}",
            CalculatedBmiAtRegistration = 30m,
            ConsentForFutureContactForEvaluation = true,
            DateOfBirth = DateTimeOffset.Now.AddYears(-40).AddMonths(count),
            DateOfReferral = DateTimeOffset.Now,
            Email = $"pagambar+notify{count}@gmail.com",
            Ethnicity = "White",
            FamilyName = $"FamilyName_{count}",
            GivenName = $"GivenName_{count}",
            HasDiabetesType1 = true,
            HasDiabetesType2 = false,
            HasHypertension = true,
            HasRegisteredSeriousMentalIllness = false,
            HeightCm = 150m,
            IsActive = true,
            IsVulnerable = false,
            Mobile = isMobile? number:"",
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = TEST_REFERRAL_API_USER_ID,
            NhsNumber = nhsnumber.ToString(),
            Postcode = $"TF{count}4NF",
            ReferringGpPracticeNumber = $"M1111{count}",
            Sex = "Male",
            Status = Business.Enums.ReferralStatus.ChatBotCall1.ToString(),
            StatusReason = STATUS_REASON[count-1],
            Telephone = isMobile ? "":number,
            TriagedCompletionLevel = null,
            TriagedWeightedLevel = null,
            Address3 = $"Address_{count}",
            Ubrn = UBRN_LIST[count-1],
            VulnerableDescription = "Not Vulnerable",
            WeightKg = 120m,
            Calls = new List<Business.Entities.Call>()
            {
              new Business.Entities.Call
              {
                IsActive = true,
                ModifiedAt = DateTime.UtcNow,
                ModifiedByUserId = CHATBOT_API_USER_ID,
                Number = number,
                Sent = DateTimeOffset.Now
              }
            }
          });
        }
        return referrals;
      }
    }
  }
}
