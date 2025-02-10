using System;
using WmsHub.Business.Extensions;
using static WmsHub.Business.Enums.ReferralSource;
using static WmsHub.Business.Enums.ReferralStatus;
using static WmsHub.Business.Enums.MessageType;

namespace WmsHub.Business.Enums
{
  public enum MessageTemplates
  {
    [MessageTemplateLookup(Pharmacy | Msk, FailedToContactEmailMessage, Email)]
    FailedToContactReferrerEmail,
    [MessageTemplateLookup(SelfReferral, FailedToContactEmailMessage, Email)]
    FailedToContactServiceUserEmail,
    [MessageTemplateLookup(SelfReferral, FailedToContactTextMessage, SMS)]
    FailedToContactServiceUserSms,
    [MessageTemplateLookup(GeneralReferral, TextMessage1, SMS)]
    GeneralReferralFirst,
    [MessageTemplateLookup(GeneralReferral, TextMessage2, SMS)]
    GeneralReferralSecond,
    [MessageTemplateLookup(GpReferral, TextMessage1, SMS)]
    GpReferralFirst,
    [MessageTemplateLookup(GpReferral, TextMessage2, SMS)]
    GpReferralSecond,
    [MessageTemplateLookup(Msk, TextMessage1, SMS)]
    MskReferralFirst,
    [MessageTemplateLookup(Msk, TextMessage2, SMS)]
    MskReferralSecond,
    [MessageTemplateLookup(SelfReferral |   
      Pharmacy | 
      GeneralReferral | 
      Msk, 
      ProviderDeclinedTextMessage, SMS)]
    NonGpProviderDeclined,
    [MessageTemplateLookup(SelfReferral | 
      Pharmacy | 
      GeneralReferral | 
      Msk, 
      ProviderRejectedTextMessage, 
      SMS)]
    NonGpProviderRejected,
    [MessageTemplateLookup(SelfReferral | 
      Pharmacy | 
      GeneralReferral |
      Msk, 
      ProviderTerminatedTextMessage,
      SMS)]
    NonGpProviderTerminated,
    [MessageTemplateLookup(GpReferral | 
      SelfReferral | 
      Pharmacy | 
      GeneralReferral | 
      Msk, 
      ReferralStatus.Exception,SMS)]
    NumberNotMonitored,
    [MessageTemplateLookup(Pharmacy, TextMessage1, SMS)]
    PharmacyReferralFirst,
    [MessageTemplateLookup(Pharmacy, TextMessage2, SMS)]
    PharmacyReferralSecond,
    [MessageTemplateLookup(Msk, RmcCall, Email)]
    ProviderByEmailTemplateId,
    [MessageTemplateLookup(SelfReferral, CancelledDuplicateTextMessage, SMS)]
    StaffReferralCancelledDuplicate,
    [MessageTemplateLookup(SelfReferral, TextMessage1, SMS)]
    StaffReferralFirstMessage,
    [MessageTemplateLookup(SelfReferral, TextMessage2, SMS)]
    StaffReferralSecondMessage,
    [MessageTemplateLookup(ElectiveCare, TextMessage1, SMS)]
    ElectiveCareReferralFirst,
    [MessageTemplateLookup(ElectiveCare, TextMessage2, SMS)]
    ElectiveCareReferralSecond,
    [MessageTemplateLookup(ElectiveCare, New | RmcCall, Email)]
    ElectiveCareUserRegistrations,
    [MessageTemplateLookup(ElectiveCare, New | RmcCall | Cancelled, Email)]
    ElectiveCareUserDeletion,
  }

}