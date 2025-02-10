using System;
using System.Threading.Tasks;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Interface;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Models.Results;
using static WmsHub.ReferralsService.Enums;

namespace WmsHub.ReferralsService.Interfaces;
public interface IReferralsDataProvider
{
  Task<bool> CompleteDischarge(Guid id);
  Task<CreateCriRecordResult> CreateCriRecord(CriCreateRequest criRecord);
  Task<CreateReferralResult> CreateReferral(ReferralPost referral);
  Task<GetCriResult> GetCriDocument(
    ErsSession activeSession,
    string nhsNumber,
    string ubrn);
  Task<GetDischargeListResult> GetDischarges();
  Task<IErsReferral> GetErsReferralByUbrn(
    IErsSession session,
    string ubrn,
    string nhsNumber = "Unknown");
  Task<AvailableActionResult> GetAvailableActions(
    IErsSession session,
    IErsReferral ersReferral,
    string nhsNumber);
  Task<ErsReferralResult> GetRegistration(
    ErsWorkListEntry ersWorkListEntry,
    string attachmentId,
    string overrideAttachmentId,
    DateTimeOffset? mostRecentAttachmentDate,
    ErsSession activeSession, 
    bool showDiagnostics);
  Task<RegistrationListResult> GetReferralList(bool useServiceId);
  Task<IReviewCommentResult> RecordOutcome(string comment,
    IErsReferral ersReferral,
    string nhsNumber,
    Outcome outcome,
    IErsSession session);
  Task<WorkListResult> GetWorkListFromErs(ErsSession session);
  Task<bool> SetErsReferralClosed(Guid referralId);
  Task<bool> SetErsReferralClosed(string ubrn);
  Task<UpdateCriRecordResult> UpdateCriRecord(
    CriUpdateRequest criRecord,
    string ubrn);
  Task UpdateInvalidAttachment(
    ReferralInvalidAttachmentPost invalidAttachmentPost);
  Task UpdateMissingAttachment(
    ReferralMissingAttachmentPost request);
  Task<UpdateReferralResult> UpdateReferral(
    ReferralPut referral,
    string ubrn);
  Task<UpdateReferralResult> UpdateReferralCancelledByEReferral(string ubrn);
  Task UpdateNhsNumberMismatch(ReferralNhsNumberMismatchPost request);
  Task NewInvalidAttachment(ReferralInvalidAttachmentPost invalidAttachmentPost);
  Task NewMissingAttachment(ReferralMissingAttachmentPost request);
  Task NewNhsNumberMismatch(ReferralNhsNumberMismatchPost request);
}

