using System;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Enums
{
  [Flags]
  public enum ReferralStatus : int
  {
    [ReferralStatusTrace(false)] Exception = 0x0,

    [ReferralStatusTrace(Constants.MIN_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MIN_DAYS_BETWEEN_TRACES)]
    New = 0x1,
    [ReferralStatusTrace(false)]
    RejectedToEreferrals = 0x2,
    [ReferralStatusTrace(false)]
    CancelledByEreferrals = 0x4,

    [ReferralStatusTrace(Constants.MIN_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MIN_DAYS_BETWEEN_TRACES)]
    TextMessage1 = 0x8,

    [ReferralStatusTrace(Constants.MIN_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MIN_DAYS_BETWEEN_TRACES)]
    TextMessage2 = 0x10,

    [ReferralStatusTrace(Constants.MIN_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MIN_DAYS_BETWEEN_TRACES)]
    ChatBotCall1 = 0x20,

    [ReferralStatusTrace(Constants.MIN_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MIN_DAYS_BETWEEN_TRACES)]
    ChatBotTransfer = 0x40,

    [ReferralStatusTrace(Constants.MIN_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MIN_DAYS_BETWEEN_TRACES)]
    ChatBotCall2 = 0x80,

    [ReferralStatusTrace(Constants.MIN_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MIN_DAYS_BETWEEN_TRACES)]
    RmcCall = 0x100,

    [ReferralStatusTrace(Constants.MIN_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MIN_DAYS_BETWEEN_TRACES)]
    RmcDelayed = 0x200,

    [ReferralStatusTrace(Constants.MIN_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MIN_DAYS_BETWEEN_TRACES)]
    Letter = 0x400,
    [ReferralStatusTrace(false)]
    FailedToContact = 0x800,
    [ReferralStatusTrace(false)]
    FailedToContactTextMessage = 0x1000,

    [ReferralStatusTrace(Constants.MIN_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MIN_DAYS_BETWEEN_TRACES)]
    ProviderAwaitingTrace = 0x2000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderAwaitingStart = 0x4000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderAccepted = 0x8000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderDeclinedByServiceUser = 0x10000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderRejected = 0x20000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderRejectedTextMessage = 0x40000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderContactedServiceUser = 0x80000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderStarted = 0x100000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderCompleted = 0x200000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderTerminated = 0x400000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderTerminatedTextMessage = 0x800000,

    [ReferralStatusTrace(Constants.MAX_DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_MAX_DAYS_BETWEEN_TRACES)]
    Complete = 0x1000000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    LetterSent = 0x2000000,

    [ReferralStatusTrace(false)]
    CancelledDuplicateTextMessage = 0x4000000,

    [ReferralStatusTrace(false)]
    CancelledDuplicate = 0x8000000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    AwaitingDischarge = 0x10000000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    ProviderDeclinedTextMessage = 0x20000000,

    [ReferralStatusTrace(Constants.DAYS_BETWEEN_TRACES,
      Constants.DEFAULT_DAYS_BETWEEN_TRACES)]
    DischargeOnHold = 0x40000000,
  }
}