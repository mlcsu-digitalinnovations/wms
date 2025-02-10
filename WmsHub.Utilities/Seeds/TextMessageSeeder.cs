using System;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;

namespace WmsHub.Utilities.Seeds
{
  public class TextMessageSeeder : SeederBase<TextMessage>
  {
    public static readonly Guid GOVUKNOTIFY_API_USER_ID =
       new Guid("7441532f-a823-4405-9d0f-55aaa2fbc14a");
    private const string TEST_DATA_KEY = "TEXTMESSAGE_TEST_DATA";
    private const string SMOKE_TEST_MOBILE = "+447700900000";

    internal static void DeleteTestData()
    {
      DatabaseContext.Referrals.RemoveRange(
        DatabaseContext.Referrals
          .Where(r => r.ProgrammeOutcome == TEST_DATA_KEY));
    }

    internal static async Task CreateTextMessages(int noOfRecords)
    {
      for (int i = 0; i < noOfRecords; i++)
      {
        var referral = RandomEntityCreator.CreateRandomReferral(
          mobile: SMOKE_TEST_MOBILE,
          programmeOutcome: TEST_DATA_KEY,
          status: ReferralStatus.New);

        DatabaseContext.Referrals.Add(referral);
      }

      await DatabaseContext.SaveChangesAsync();
    }
  }
}