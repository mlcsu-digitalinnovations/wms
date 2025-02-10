using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService
{
  public class Enums
  {
    public enum Outcome : int
    {
      ACCEPT_REFER_BOOK_LATER,
      CANCEL_APPOINTMENT_ACTION_LATER,
      PROVIDER_CONVERTED_ADVICE_AND_GUIDANCE_ADMIN_TO_REFER,
      RETURN_TO_REFERRER_WITH_ADVICE
    }

    public enum ReferralAction : int
    {
      RECORD_REVIEW_OUTCOME
    }
  }
}
