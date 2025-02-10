using System;
using WmsHub.Business.Extensions;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Enums;

[Flags]
public enum ReferralStatus : long
{
  Exception = 0x0,

  [ReferralStatusTrace]
  [ReferralStatusMessageType(MessageType.SMS)]
  New = 0x1,

  RejectedToEreferrals = 0x2,

  CancelledByEreferrals = 0x4,

  [ReferralStatusTrace]
  [ReferralStatusMessageType(MessageType.SMS)]
  TextMessage1 = 0x8,

  [ReferralStatusTrace]
  [ReferralStatusMessageType(MessageType.SMS)]
  TextMessage2 = 0x10,

  [ReferralStatusTrace]
  ChatBotCall1 = 0x20,

  [ReferralStatusTrace]
  ChatBotTransfer = 0x40,

  [ReferralStatusTrace]
  RmcCall = 0x100,

  [ReferralStatusTrace]
  RmcDelayed = 0x200,

  Letter = 0x400,

  FailedToContact = 0x800,

  [ReferralStatusMessageType(MessageType.SMS)]
  FailedToContactTextMessage = 0x1000,

  [ReferralStatusTrace]
  ProviderAwaitingTrace = 0x2000,

  [ReferralStatusTrace]
  ProviderAwaitingStart = 0x4000,

  [ReferralStatusTrace]
  ProviderAccepted = 0x8000,

  [ReferralStatusTrace]
  [RejectionList]
  ProviderDeclinedByServiceUser = 0x10000,

  [ReferralStatusTrace]
  [RejectionList]
  ProviderRejected = 0x20000,

  [ReferralStatusTrace]
  [ReferralStatusMessageType(MessageType.SMS)]
  ProviderRejectedTextMessage = 0x40000,

  [ReferralStatusTrace]
  ProviderContactedServiceUser = 0x80000,

  [ReferralStatusTrace]
  ProviderStarted = 0x100000,

  [ReferralStatusTrace]
  ProviderCompleted = 0x200000,

  [ReferralStatusTrace]
  [RejectionList]
  ProviderTerminated = 0x400000,

  [ReferralStatusTrace]
  [ReferralStatusMessageType(MessageType.SMS)]
  ProviderTerminatedTextMessage = 0x800000,

  Complete = 0x1000000,

  LetterSent = 0x2000000,
 
  [ReferralStatusMessageType(MessageType.SMS)]
  CancelledDuplicateTextMessage = 0x4000000,

  CancelledDuplicate = 0x8000000,

  [ReferralStatusTrace]
  AwaitingDischarge = 0x10000000,

  [ReferralStatusTrace]
  [ReferralStatusMessageType(MessageType.SMS)]
  ProviderDeclinedTextMessage = 0x20000000,

  DischargeOnHold = 0x40000000,

  CancelledDueToNonContact = 0x80000000,
  
  UnableToDischarge = 0x100000000,

  SentForDischarge = 0x200000000,

  [ReferralStatusTrace]
  DischargeAwaitingTrace = 0x400000000,

  [ReferralStatusMessageType(MessageType.Email)]
  FailedToContactEmailMessage = 0x800000000,

  Cancelled = 0x1000000000,

  [ReferralStatusTrace]
  [ReferralStatusMessageType(MessageType.SMS)]
  TextMessage3 = 0x2000000000
}