using System;

namespace WmsHub.Business.Models
{
  public interface IReprocessedReferral
  {
    string Ubrn { get; set; }

    /// <summary>
    /// Either New, RmcCall or Exception taken from the first audit log
    /// for that referral
    /// </summary>
    string InitialStatus { get; }

    /// <summary>
    /// The status reason taken from the first audit log for that referral
    /// </summary>
    string InitialStatusReason { get; set; }

    /// <summary>
    /// Yes if the referral has had an Exception status and subsequently a
    /// RejectedToEreferrals status followed by another New, RmcCall or
    /// Exception status, N/A if initial status is New, else No
    /// </summary>
    bool Reprocessed { get; }

    /// <summary>
    /// Yes if the referral has had an Exception status and subsequently 
    /// a New or RmcCall status, N/A if intial status is New, else No
    /// </summary>
    bool SuccessfullyReprocessed { get; }

    /// <summary>
    /// Yes if the referral has had a CancelledByEreferrals status and
    /// subsequently a New or RmcCall status, N/A if intial status
    /// is New, else No
    /// </summary>
    bool Uncancelled { get; }

    /// <summary>
    /// Yes if current referral status is CancelledByEreferrals else No
    /// </summary>
    bool CurrentlyCancelled { get; }

    /// <summary>
    /// The current status reason if current referral status is
    /// CancelledByEreferrals else blank
    /// </summary>
    string CurrentlyCancelledStatusReason { get; set; }

    DateTimeOffset? DateOfReferral { get; set; }
    string ReferringGpPracticeCode { get; set; }
  }
}