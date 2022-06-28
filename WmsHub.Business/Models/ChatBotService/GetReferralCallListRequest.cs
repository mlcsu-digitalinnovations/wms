using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ChatBotService
{
  [ExcludeFromCodeCoverage]
  public class GetReferralCallListRequest
  {
    public Guid Id { get; set; }
    public string Filter { get; set; }
    public FailType FailOn { get; set; } = FailType.FailOnly;

    public Expression<Func<Entities.Referral, bool>> Predicate { get; set; }
  }
}