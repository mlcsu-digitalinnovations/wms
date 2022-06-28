#nullable enable
using System;
using WmsHub.Common.Helpers;

namespace WmsHub.Common.Attributes
{
  public class ReferralStatusTraceAttribute:Attribute
  {
    private string? _numberDaysLookup;
    private int _numDays;

    public  string NumberDaysLookup
    {
      get => $"{Constants.WMS_REFERRAL_ENV_ROUTE}{_numberDaysLookup}";
      set => _numberDaysLookup = value??"";
    }
    public  bool CanTrace { get; set; }
    
    public int NumberOfDays
    {
      get
      {
        _numDays = -1;
        if (!string.IsNullOrWhiteSpace(_numberDaysLookup))
        {
          _numDays = 0;

          string? env = Environment.GetEnvironmentVariable(NumberDaysLookup);
          if (env != null && !string.IsNullOrWhiteSpace(env))
          {
            if (int.TryParse(env, out int i))
            {
              _numDays = i;
            }
          }
        }
        else if (string.IsNullOrWhiteSpace(_numberDaysLookup) && CanTrace)
        {
          _numDays = 1;
        }

        return _numDays;
      }
    }

    public ReferralStatusTraceAttribute(bool canTrace)
      : this(canTrace, null, 1)
    { }

    public ReferralStatusTraceAttribute(
      bool canTrace,
      string? numberLookup,
      int defaultDays)
    {
      _numDays = defaultDays;
      _numberDaysLookup = numberLookup;
      CanTrace = canTrace;
    }

    public ReferralStatusTraceAttribute( string numberLookup,
      int defaultDays):this(true, numberLookup, defaultDays)
    {
    }

  }
}
