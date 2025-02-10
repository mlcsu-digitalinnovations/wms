using System;
using System.Text.RegularExpressions;

namespace WmsHub.Business.Models
{

  public class ReprocessedReferral : IReprocessedReferral
  {
    private const string ReprocessedPattern =
      "^(Exception|RmcCall).*(Exception).*(RejectedToEreferrals)" +
      ".*(New|RmcCall|Exception)";
    private const string SuccessfulPattern =
      @"^(Exception|RmcCall).*(Exception).*(New|RmcCall)";
    private const string UncancelledPattern =
      "^((Exception|RmcCall).*(CancelledByEreferrals).*(New|RmcCall)).*";
    private const string CancelledPattern =
      @".*(CancelledByEreferrals)$.*";
    public string Ubrn { get; set; }
    /// <summary>
    /// Either New, RmcCall or Exception taken from the first audit log
    /// for that referral
    /// </summary>
    public string InitialStatus 
      => StatusArray != null && StatusArray.Length > 0 
        ? StatusArray[0] 
        : "Not Set";
    /// <summary>
    /// The status reason taken from the first audit log for that referral
    /// </summary>
    public string InitialStatusReason { get;  set; }

    /// <summary>
    /// Yes if the referral has had an Exception status and subsequently a
    /// RejectedToEreferrals status followed by another New, RmcCall or
    /// Exception status, N/A if initial status is New, else No
    /// </summary>
    public bool Reprocessed => 
      new Regex(ReprocessedPattern).Match(StatusCsv).Success;

    /// <summary>
    /// Yes if the referral has had an Exception status and subsequently 
    /// a New or RmcCall status, N/A if intial status is New, else No
    /// </summary>
    public bool SuccessfullyReprocessed =>
      new Regex(SuccessfulPattern).Match(StatusCsv).Success;
    /// <summary>
    /// Yes if the referral has had a CancelledByEreferrals status and
    /// subsequently a New or RmcCall status, N/A if intial status
    /// is New, else No
    /// </summary>
    public bool Uncancelled =>
      new Regex(UncancelledPattern).Match(StatusCsv).Success;
    /// <summary>
    /// Yes if current referral status is CancelledByEreferrals else No
    /// </summary>
    public bool CurrentlyCancelled =>
      new Regex(CancelledPattern).Match(StatusCsv).Success;
    /// <summary>
    /// The current status reason if current referral status is
    /// CancelledByEreferrals else blank
    /// </summary>
    public string CurrentlyCancelledStatusReason 
    { 
      get { return CurrentlyCancelled ? _currentlyCancelledStatusReason : ""; }
      set { _currentlyCancelledStatusReason = value; }
    }
    private string _currentlyCancelledStatusReason;
    public DateTimeOffset? DateOfReferral { get;  set; }
    public string ReferringGpPracticeCode { get; set; }
    public string[] StatusArray { get; set; }
    public string StatusCsv => string.Join(',', StatusArray);
  }
}
