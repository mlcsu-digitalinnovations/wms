using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services.Interfaces
{
  public interface IBusinessIntelligenceService : IServiceBase
  {
    Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferrals(
      DateTimeOffset? fromDate = null,
      DateTimeOffset? toDate = null);
    
    Task<IEnumerable<AnonymisedReferral>> 
      GetAnonymisedReferralsBySubmissionDate(
      DateTimeOffset? fromDate = null,
      DateTimeOffset? toDate = null);
    
    Task<IEnumerable<ReprocessedReferral>> 
      GetAnonymisedReprocessedReferralsBySubmissionDate(
      DateTimeOffset? fromDate = null,
      DateTimeOffset? toDate = null);
    
    Task<IEnumerable<ProviderBiData>> GetProviderBiDataAsync(
      DateTimeOffset? fromDate,
      DateTimeOffset? toDate);
	  
    Task<IEnumerable<NhsNumberTrace>> GetUntracedNhsNumbers();

    Task<IEnumerable<AnonymisedReferral>>
      GetAnonymisedReferralsForUbrn(string ubrn);

    Task UpdateSpineTraced(IEnumerable<SpineTraceResult> spineTraceResults);
  }
}
