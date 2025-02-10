using System.Linq;
using WmsHub.Common.Exceptions;

namespace WmsHub.Business.Models
{
  public class PharmacyReferralOptions
  {
    public const string SectionKey = "PharmacyReferralOptions";
    public const string OptionsSectionKey =
      "PharmacyEmailWhitelist";

    public string[] Emails { get; set; }

    public bool HasEmails => Emails != null && Emails.Length > 0;

    public bool IsWhitelistEnabled { get; set; } = false;

    public bool IsEmailInWhitelist(string email, bool throwErrors = true)
    {
      bool isInWhiteList = false;

      if (IsWhitelistEnabled)
      {
        if (HasEmails)
        {
          isInWhiteList = Emails.Any(t => t == email);

          if (!isInWhiteList && throwErrors)
          {
            throw new EmailWhiteListException(
              $"Email {email} is not in the pharmacy whitelist.");
          }
        }
        else if (throwErrors)
        {
          throw new EmailWhiteListException(
            $"Pharmacy whitelist is enabled but empty.");
        }
      }
      else
      {
        isInWhiteList = true;
      }

      return isInWhiteList;
    }
  }
}
