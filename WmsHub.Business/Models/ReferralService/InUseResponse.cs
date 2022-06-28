using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;
using static WmsHub.Business.Enums.ReferralStatus;

namespace WmsHub.Business.Models.ReferralService
{
  public class InUseResponse
  {    
    private IReferral _referral;

    public virtual InUseResult InUseResult { get; private set; } = 
      InUseResult.NotFound;

    public bool WasFound => InUseResult.HasFlag(InUseResult.Found);
    public bool WasNotFound => InUseResult.HasFlag(InUseResult.NotFound);

    public bool IsCompleteAndProviderNotSelected => 
      InUseResult.HasFlag(InUseResult.Complete) 
      && InUseResult.HasFlag(InUseResult.ProviderNotSelected);

    public IReferral Referral
    {
      get => _referral;
      set
      {
        _referral = value;

        if (_referral == null)
        {
          InUseResult = InUseResult.NotFound;
        }
        else
        {
          InUseResult = InUseResult.Found;

          if (_referral.ProviderId == null)
          {
            InUseResult |= InUseResult.ProviderNotSelected;
          }
          else
          {
            InUseResult |= InUseResult.ProviderSelected;
          }

          if (_referral.ReferralSource.Is(ReferralSource.GeneralReferral))
          {
            InUseResult |= InUseResult.IsGeneralReferral;
          }
          else
          {
            InUseResult |= InUseResult.IsNotGeneralReferral;
          }

          if (_referral.Status.Is(CancelledByEreferrals))
          {
            InUseResult |= InUseResult.Cancelled;
          }
          else if (_referral.Status.Is(CancelledDuplicate))
          {
            InUseResult |= InUseResult.Cancelled;
          }
          else if (_referral.Status.Is(CancelledDuplicateTextMessage))
          {
            InUseResult |= InUseResult.Cancelled;
          }
          else if (_referral.Status.Is(Complete))
          {
            InUseResult |= InUseResult.Complete;
          }
        }
      }
    }
  }
}