using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Models.ChatBotService;

namespace WmsHub.Business.Services
{
  public interface IChatBotService : IServiceBase
  {
    Task<bool> AddReferrals(List<Referral> referrals);
    Task<GetReferralCallListResponse> GetReferralCallList(
      GetReferralCallListRequest request);
    Task<List<Referral>> GetReferralsWithCalls(
      Expression<Func<Referral, bool>> predicate);
    Task<string> RemoveReferrals(List<Referral> referrals);
    Task<UpdateReferralWithCallResponse> UpdateReferralWithCall(
      UpdateReferralWithCallRequest request);
    
  }
}