using System;
using System.Threading.Tasks;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Models;
using WmsHub.Referral.Api.Models;
using WmsHub.ReferralsService.Models;
using WmsHub.ReferralsService.Models.Results;
using static WmsHub.ReferralsService.Enums;

namespace WmsHub.ReferralsService.Interfaces
{
  public interface IReferralsDataProvider
  {
    Task<CreateCriRecordResult> CreateCriRecord(CriCreateRequest criRecord);
    Task<CreateReferralResult> CreateReferral(ReferralPost referral);
    Task<ReviewCommentResult> RecordOutcome(ErsSession session, 
      string ubrn, string nhsNumber, Outcome outcome, string comment);
    Task<ReviewCommentResult> RecordOutcome(ErsSession session,
      ErsReferral ersReferral, string nhsNumber, Outcome outcome, 
      string comment);
    Task<AvailableActionResult> GetAvailableActions(ErsSession session,
      ErsReferral ersReferral, string nhsNumber);
    Task<GetCriResult> GetCriDocument(string ubrn, string nhsNumber, 
      ErsSession activeSession);
    Task<UpdateCriRecordResult> UpdateCriRecord(CriUpdateRequest criRecord,
      string ubrn);
    Task<RegistrationListResult> GetReferralList(bool useServiceId);
    Task<ErsReferralResult> GetRegistration(
      ErsWorkListEntry ersWorkListEntry,
      long? attachmentId,
      long? overrideAttachmentId,
      ErsSession activeSession,
      bool showDiagnostics);
    Task<WorkListResult> GetWorkListFromErs(ErsSession session);
    Task<UpdateReferralResult> UpdateReferral
      (ReferralPut referral, string ubrn);
    Task<UpdateReferralResult> UpdateReferralCancelledByEReferral(string ubrn);

    Task<ErsReferral> GetErsReferralByUbrn(ErsSession session,
        string ubrn, string nhsNumber = "Unknown");
    Task<GetDischargeListResult> GetDischarges();

    Task NewNhsNumberMismatch(ReferralNhsNumberMismatchPost request);
    Task NewMissingAttachment(ReferralMissingAttachmentPost request);

    Task UpdateNhsNumberMismatch(ReferralNhsNumberMismatchPost request);
    Task UpdateMissingAttachment(ReferralMissingAttachmentPost request);
    Task NewInvalidAttachment(
      ReferralInvalidAttachmentPost invalidAttachmentPost);

    Task UpdateInvalidAttachment(
      ReferralInvalidAttachmentPost invalidAttachmentPost);
    Task<bool> CompleteDischarge(Guid id);
  }
}
