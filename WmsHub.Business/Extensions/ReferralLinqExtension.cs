using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Common.Extensions;
using Referral = WmsHub.Business.Entities.Referral;

namespace WmsHub.Business.Extensions
{
  public static class ReferralLinqExtension
  {
    public static Task<List<NhsNumberTraceReferral>> WhereDateComparesAsync(
      this IQueryable<Referral> source, int daysDif)
    {
      DateTimeOffset diff = DateTimeOffset.Now.AddDays(-daysDif);
      IQueryable<NhsNumberTraceReferral> r = source.Where(
        t => t.DateCompletedProgramme == null ||
             t.DateCompletedProgramme > diff).Select(r => new NhsNumberTraceReferral
      {
        DateOfBirth = r.DateOfBirth.Value,
        FamilyName = r.FamilyName,
        GivenName = r.GivenName,
        Id = r.Id,
        Postcode = r.Postcode,
        LastTraceDate = r.LastTraceDate,
        TraceCount = r.TraceCount,
        Status = r.Status
      });
      return r.ToListAsync();

    }
  }
}
