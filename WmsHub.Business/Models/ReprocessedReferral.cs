using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Models
{

  public class ReprocessedReferral : IReprocessedReferral
  {
    private const string EXCEPTION = "Exception";
    private const string NEW = "New";
    private const string RMCCALL = "RmcCall";
    private const string REJECTED = "RejectedToEreferrals";
    private const string CANCELED = "CancelledByEreferrals";
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
    public bool Reprocessed
    {
      get
      {
        if (StatusArray == null || StatusArray.Length < 2)
        {
          return false;
        }

        //If initial status is NOT NEW then return false
        if(InitialStatus != NEW)
        {
          return false;
        }
        
        //if there is no exception or the RejectedToErefferals was before any
        // exception then return false
        if((exceptionPosition == -1 || rejectedToReferralsPosition == -1) &&
          rejectedToReferralsPosition >= exceptionPosition)
        {
          return false;
        }

        //get the last event of New, RmcCall or Exception
        newRmcOrExceptionPosition = lastNew;
        if (lastException >  newRmcOrExceptionPosition)
        {
          newRmcOrExceptionPosition = lastException;
        }

        if (lastRmcCall > newRmcOrExceptionPosition)
        {
          newRmcOrExceptionPosition = lastRmcCall;
        }


        return newRmcOrExceptionPosition > rejectedToReferralsPosition;
      }
    }
    /// <summary>
    /// Yes if the referral has had an Exception status and subsequently 
    /// a New or RmcCall status, N/A if intial status is New, else No
    /// </summary>
    public bool SuccessfullyReprocessed
    {
      get
      {
        if (StatusArray == null || StatusArray.Length < 2)
        {
          return false;
        }

        //If initial status is NOT NEW then return false
        if (InitialStatus != NEW)
        {
          return false;
        }

        if(exceptionPosition == -1)
        {
          return false;
        }

        return !(exceptionPosition > -1 &&
          lastRmcCall <= exceptionPosition 
          && lastNew <= exceptionPosition);
        
      }
    }
    /// <summary>
    /// Yes if the referral has had a CancelledByEreferrals status and
    /// subsequently a New or RmcCall status, N/A if intial status
    /// is New, else No
    /// </summary>
    public bool Uncancelled
    {
      get
      {
        if (StatusArray == null || StatusArray.Length < 2)
        {
          return false;
        }

        //If initial status is NOT NEW then return false
        if (InitialStatus != NEW)
        {
          return false;
        }

        if(cancelledPosition == -1)
        {
          return false;
        }

        return cancelledPosition < lastNew || cancelledPosition < lastRmcCall;

      }
    }
    /// <summary>
    /// Yes if current referral status is CancelledByEreferrals else No
    /// </summary>
    public bool CurrentlyCancelled => StatusArray.Last() == CANCELED;
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
    private int exceptionPosition  => Array.IndexOf(StatusArray, EXCEPTION);
    private int rejectedToReferralsPosition => 
      Array.IndexOf(StatusArray, REJECTED);
    private int newRmcOrExceptionPosition;
    private int lastRmcCall => Array.LastIndexOf(StatusArray, RMCCALL);
    private int lastNew => Array.LastIndexOf(StatusArray, NEW);
    private int lastException => Array.LastIndexOf(StatusArray, EXCEPTION);
    private int cancelledPosition => Array.LastIndexOf(StatusArray, CANCELED);
  }
}
