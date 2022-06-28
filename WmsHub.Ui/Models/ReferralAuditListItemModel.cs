using System;

namespace WmsHub.Ui.Models
{
  public class ReferralAuditListItemModel : 
    IEquatable<ReferralAuditListItemModel>
  {
    private DateTimeOffset _modifiedAt;
    public DateTimeOffset ModifiedAt
    {
      get
      {
        DateTime azureServerTime = DateTime.UtcNow;

        TimeZoneInfo ukTimezone = TimeZoneInfo
          .FindSystemTimeZoneById("GMT Standard Time");

        if (ukTimezone.IsDaylightSavingTime(azureServerTime))
        {
          return _modifiedAt.UtcDateTime.AddHours(1);
        }

        return _modifiedAt;
      }

      set => _modifiedAt = value;
    }
    public int AuditId { get; set; }
    public string Ubrn { get; set; }
    public string NhsNumber { get; set; }
    public DateTimeOffset? DateOfReferral { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public bool WasDelayed => DateToDelayUntil != null;
    public DateTimeOffset? DateToDelayUntil { get; set; }
    public string DelayReason { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string Status { get; set; }
    public string StatusReason { get; set; }
    public bool? IsVulnerable { get; set; }
    public string VulnerableDescription { get; set; }
    public string Mobile { get; set; }
    public string Telephone { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string Postcode { get; set; }
    public string ReferringGpPracticeName { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }

    public bool Equals(ReferralAuditListItemModel other)
    {
      if (other is null)
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      //Check whether the products' properties are equal.
      return Ubrn.Equals(other.Ubrn) &&
        Status.Equals(other.Status) &&
        ModifiedAt.Equals(other.ModifiedAt);
    }

    public override int GetHashCode()
    {
      int hashUbrn = Ubrn == null ? 0 : Ubrn.GetHashCode();
      int hashStatus = Status == null ? 0 : Status.GetHashCode();
      int hashModifiedAt = ModifiedAt.ToString() == null
        ? 0
        : ModifiedAt.ToString().GetHashCode();

      return hashUbrn ^ hashStatus ^ hashModifiedAt;
    }
  }
}