using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Models.BusinessIntelligence;
using WmsHub.Business.Models.ElectiveCareReferral;
using WmsHub.Business.Models.Tracing;

namespace WmsHub.Business.Services.Interfaces;

public interface IBusinessIntelligenceService : IServiceBase
{
  Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferrals(
    DateTimeOffset? fromDate = null, 
    DateTimeOffset? toDate = null);

  Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsBySubmissionDate(
    DateTimeOffset? fromDate = null,
    DateTimeOffset? toDate = null);

  Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsForUbrn(
    string ubrn);

  Task<IEnumerable<ReprocessedReferral>> 
    GetAnonymisedReprocessedReferralsBySubmissionDate(
      DateTimeOffset? fromDate = null,
      DateTimeOffset? toDate = null);

  Task<IEnumerable<ProviderBiData>> GetProviderBiDataAsync(
    DateTimeOffset? fromDate,
    DateTimeOffset? toDate);

  /// <summary>
  /// Get a list of the count of referrals by each month.
  /// </summary>
  /// <returns>An enumerable of ReferralCountByMonth.</returns>
  Task<IEnumerable<ReferralCountByMonth>> GetReferralCountsByMonthAsync();

  /// <summary>
  /// Get a list of referral Ids and ReferringGpPracticeNumber where a referral
  /// Status has been set to DischargeAwaitingTrace more than once.
  /// </summary>
  /// <returns>An enumerable of TraceIssueReferral</returns>
  Task<IEnumerable<TraceIssueReferral>> GetTraceIssueReferralsAsync();

  /// <summary>
  /// Returns an enumerable of user action logs
  /// </summary>
  /// <param name="start">The first action date to return</param>
  /// <param name="end">The last action date to return</param>
  Task<IEnumerable<BiRmcUserInformation>> GetRmcUsersInformation(
   DateTimeOffset start,
   DateTimeOffset end);

  /// <summary>
  /// Returns a IEnumerable of active NhsNumberTrace objects consisting of
  /// referrals whose NHS number property is null NHS Number traceing is 
  /// continual depending on ReferralStatus or max 2 years after completion.
  /// Linq extensions and ReferralStatus extensions use attributes to set
  /// the restrictions on the list returned.
  /// </summary>
  /// <returns>IEnumerable of NhsNumberTrace objects</returns>
  Task<IEnumerable<NhsNumberTrace>> GetUntracedNhsNumbers();

  /// <summary>
  ///   Updates the TraceCount, LastTraceDate, ModifiedAt and ModifiedByUserId
  ///   properties of each referal matched on the Id of each of the
  ///   SpineTraceResult objects.
  ///   If the trace was successful: NhsNumber, ReferringGpPracticeNumber
  ///   and ReferringGpPracticeName propeties are updated to their traced
  ///   values. If this is the first trace and the traced NHS number
  ///   is already associated with another referral then the referral's status
  ///   will be set to CancelledDuplicateTextMessage. If the referral's status
  ///   is ProviderAwaitingTrace then it will be updated to
  ///   ProviderAwaitingStart.
  ///   If the trace was unsuccessful: ReferringGpPracticeNumber
  ///   and ReferringGpPracticeName propeties are updated their unknown
  ///   constants.
  /// </summary>
  Task<List<SpineTraceResponse>> UpdateSpineTraced(
    IEnumerable<SpineTraceResult> spineTraceResults);

  /// <summary>
  /// Returns a list of Service user UBRN and DateProviderSelected
  /// groupe4d by ProviderId.
  /// </summary>
  /// <param name="referralStatus">ReferralStatus is used 
  /// to return a list.</param>
  /// <returns>List of Service user UBRN's, 
  /// Dates of provider selection and a count</returns>
  Task<IEnumerable<ProviderAwaitingStartReferral>>
    GetProviderAwaitingStartReferralsAsync();

  /// <summary>
  /// Returns an enumerable of questionnaires
  /// </summary>
  /// <param name="start">The first action date to return</param>
  /// <param name="end">The last action date to return</param>
  Task<List<BiQuestionnaire>>
    GetQuestionnaires(DateTimeOffset start, DateTimeOffset end);

  /// <summary>
  /// Returns an enumerable of AnonymisedReferral for 
  /// Referrals ModifiedAt date greater or equals 
  /// parameter lastModifiedDate
  /// </summary>
  /// <param name="lastModifiedDate">The last download date</param>
  Task<IEnumerable<AnonymisedReferral>> 
    GetAnonymisedReferralsByModifiedAt(
      DateTimeOffset lastModifiedDate);

  /// <summary>
  /// Returns a trimmed set of anonymised referrals based on a given date
  /// range, which is defaulted to 31 days.  
  /// </summary>
  /// <param name="fromDate">The start of filter date.</param>
  /// <param name="toDate">End of filter date.</param>
  /// <param name="status">ReferralStatus.ProviderReject, 
  /// ReferralStatus.ProviderDeclinedByServiceUser or
  /// ReferralStatus.ProviderTerminated</param>
  /// <returns></returns>
  Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsByProviderReason(
    DateTimeOffset? fromDate, 
    DateTimeOffset? toDate, 
    ReferralStatus status);

  /// <summary>
  /// Returns an enumerable of AnonymisedReferral for 
  /// ProviderSubmissions ModifiedAt date greater or equals
  /// parameter lastModifiedDate
  /// </summary>
  /// <param name="lastModifiedDate">The last download date</param>
  Task<IEnumerable<AnonymisedReferral>>
    GetAnonymisedReferralsByProviderSubmissionsModifiedAt(
      DateTimeOffset lastModifiedDate);

  /// <summary>
  /// Returns an enumerable of AnonymisedReferral from Referrals with ModifiedAt date equal to or 
  /// after the fromDate parameter and Referrals with ProviderSubmissions with a Date equal to or
  /// after the fromDate parameter. Duplicates are removed.
  /// </summary>
  /// <param name="fromDate">The date to which ModifiedAt and ProviderSubmissions.Date will be 
  /// compared.</param>
  Task<IEnumerable<AnonymisedReferral>>
    GetAnonymisedReferralsChangedFromDate(DateTimeOffset fromDate);

  /// <summary>
  /// When an elective care file is processed and if it has any errors, these
  /// are recorded in the ElectiveCarePostErrors table.  This will return the
  /// raw saved data based on the filters.
  /// </summary>
  /// <param name="fromDate">From date will default to 31 days in the past
  /// </param>
  /// <param name="toDate">To date will default to today.</param>
  /// <param name="trustOdsCode">If the ODS code is not supplied,
  /// then the result will not be filtered by ODS code</param>
  /// <param name="trustUserId">If the Trust User Id is not supplied, or is of
  /// Guid.Empty, then results are not filtered by the user.</param>
  /// <returns>The raw table data is return in descending order with the
  /// latest at the top of the file.  The columns return are:
  /// <list type="bullet">
  /// <item><description>Id - Integer as a generic counter</description></item>
  /// <item><description>PostError - is the raw error as raised through
  /// validation.</description></item>
  /// <item><description>RowNumber - is the row number of the uploaded
  /// spreadsheet.</description></item>
  /// <item><description>TrustOdsCode - ODS code as supplied.
  /// </description></item>
  /// <item><description>TrustUserId is the GUID User ID as supplied.
  /// </description></item>
  /// </list>
  /// </returns>
  Task<IEnumerable<ElectiveCarePostError>> GetElectiveCarePostErrorsAsync(
    DateTimeOffset? fromDate,
    DateTimeOffset? toDate,
    string trustOdsCode,
    Guid? trustUserId);

  /// <summary>
  /// Returns a trimmed set of pseudonymised referrals based on a given date
  /// range, which is defaulted to 31 days.  
  /// </summary>
  /// <param name="fromDate">The start of filter date.</param>
  /// <param name="toDate">End of filter date.</param>
  /// <returns></returns>
  Task<IEnumerable<PseudonymisedReferral>> GetPseudonymisedReferralsAsync(
    DateTimeOffset? fromDate,
    DateTimeOffset? toDate);



  /// <summary>
  /// Get all udal extracts between the from and to dates.
  /// </summary>
  /// <param name="from">Include extracts from this date.</param>
  /// <param name="to">Include extracts to this date.</param>
  Task<IEnumerable<UdalExtract>> GetUdalExtractsAsync(DateTime from, DateTime to);

  /// <summary>
  /// Returns a the number of ProviderDeclinedByServiceUser, ProviderRejected
  /// and ProviderTerminated submissions within a defined date period.
  /// </summary>
  /// <param name="from"></param>
  /// <param name="to"></param>
  /// <returns></returns>
  ProviderEndedData ProviderEndedReasonStats(DateTimeOffset? from, DateTimeOffset? to);
}
