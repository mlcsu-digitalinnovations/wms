using WmsHub.Common.Exceptions;

namespace WmsHub.Referral.Api.Models;

public abstract class EmailDomainWhitelistBase 
{
  protected abstract string ReferralTypeName { get; }
  public virtual string[] EmailDomainWhitelist { get; set; }
  public virtual bool IsEmailDomainWhitelistEnabled { get; set; } = true;
  public virtual bool HasEmailDomainWhiteList
    => EmailDomainWhitelist?.Length > 0;
  /// <summary>
  /// Returns a boolean indicating whether the provided email's domain is
  /// in the domain whitelist or not.
  /// </summary>
  /// <param name="email">The email to check the domain of.</param>
  /// <returns>true if the email's domain is in the whitelist or the whitelist
  /// is disabled or false if the email's domain is not in the whitelist.
  /// </returns>
  /// <exception cref="ArgumentNullOrWhiteSpaceException"></exception>
  /// <exception cref="EmailWhiteListException"></exception>
  public virtual bool IsEmailDomainInWhitelist(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(email));
    }

    bool isInWhiteList = false;

    if (IsEmailDomainWhitelistEnabled)
    {
      if (HasEmailDomainWhiteList)
      {
        for (int i = 0; i < EmailDomainWhitelist.Length; i++)
        {
          string domain = EmailDomainWhitelist[i];
          if (domain != null && !string.IsNullOrWhiteSpace(domain))
          {
            if (email.EndsWith(domain))
            {
              isInWhiteList = true;
              break;
            }
          }
        }
      }
      else
      {
        string errorMsg =
          $"{ReferralTypeName} referral email domain whitelist " +
          "is enabled but empty.";
        throw new EmailWhiteListException(errorMsg);
      }
    }
    else
    {
      isInWhiteList = true;
    }

    return isInWhiteList;
  }
}
