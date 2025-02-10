using AutoMapper;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Profiles;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;

namespace WmsHub.Utilities
{
  public class QueryPerformanceTests
  {
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;
    private readonly IProviderService _providerService;
    private ProviderOptions _options = new ProviderOptions
    { CompletionDays = 84, NumDaysPastCompletedDate = 10 };
    private readonly Mock<IOptions<ProviderOptions>> _mockOptions =
      new Mock<IOptions<ProviderOptions>>();
    public QueryPerformanceTests(DatabaseContext context)
    {
      _mockOptions.Setup(x => x.Value).Returns(_options);
      _context = context;
      MapperConfiguration mapperConfiguration = new MapperConfiguration(cfg =>
        cfg.AddMaps(new[] { typeof(ReferralProfile) }));

      _mapper = mapperConfiguration.CreateMapper();

      _providerService = new ProviderService(_context, _mapper
        , _mockOptions.Object);
    }

    public async Task RunAllTests()
    {
      await ReferralSearchTest();
    }

    public async Task ReferralSearchTest()
    {
      IReferralService service = new ReferralService(
        _context, 
        null,
        _mapper,
        _providerService,
        null,
        null);

      List<Timing> timings = new List<Timing>();
      Stopwatch sw = new Stopwatch();

      sw.Start();
      string[] items =
        (await service.Search(new ReferralSearch() { Postcode = "BR99KV" }))
        .Referrals.Select(r => r.Postcode)
        .ToArray();
      sw.Stop();
      timings.Add(new Timing(
        sw.ElapsedMilliseconds, items.Length, "postcode", items));
      sw.Reset();

      sw.Start();
      items = (await service.Search(new ReferralSearch()
      {
        TelephoneNumber = "+441743004298"
      }))
        .Referrals.Select(r => r.Telephone)
        .ToArray();
      sw.Stop();
      timings.Add(new Timing(
        sw.ElapsedMilliseconds, items.Length, "telephone", items));
      sw.Reset();

      sw.Start();
      items = (await service.Search(new ReferralSearch()
      {
        MobileNumber = "+447886006489"
      }))
        .Referrals.Select(r => r.Mobile)
        .ToArray();
      sw.Stop();
      timings.Add(new Timing(
        sw.ElapsedMilliseconds, items.Length, "mobile", items));
      sw.Reset();

      sw.Start();
      items = (await service.Search(new ReferralSearch()
        {
          EmailAddress = "query.perf1128@test.xom"
        }))
        .Referrals.Select(r => r.Email)
        .ToArray();
      sw.Stop();
      timings.Add(new Timing(
        sw.ElapsedMilliseconds, items.Length, "email", items));
      sw.Reset();

      sw.Start();
      items = (await service.Search(new ReferralSearch()
        {
          Ubrn = "0fa60e3c-10ec-4038-bd16-9505285264cc"
        }))
        .Referrals.Select(r => r.Ubrn)
        .ToArray();
      sw.Stop();
      timings.Add(new Timing(
        sw.ElapsedMilliseconds, items.Length, "ubrn", items));
      sw.Reset();

      sw.Start();
      items = (await service.Search(new ReferralSearch()
        {
          NhsNumber = "3641863066"
        }))
        .Referrals.Select(r => r.Ubrn)
        .ToArray();
      sw.Stop();
      timings.Add(new Timing(
        sw.ElapsedMilliseconds, items.Length, "nhsnumber", items));

      sw.Start();
      items = (await service.Search(new ReferralSearch()
        {
          FamilyName= "Lyrotole"
        }))
        .Referrals.Select(r => r.FamilyName)
        .ToArray();
      sw.Stop();
      timings.Add(new Timing(
        sw.ElapsedMilliseconds, items.Length, "familyName", items));

      timings.ForEach(t =>
      {
        Log.Information(
          "Took {milliseconds}ms to find {count} {type}(s): {items}",
          t.milliseconds,
          t.count,
          t.type,
          t.items);
      });
    }

    private struct Timing
    {
      public Timing(long m, long c, string t, string[] i)
      {
        milliseconds = m;
        count = c;
        type = t;
        items = i;
      }

      public long milliseconds;
      public long count;
      public string type;
      public string[] items;
    }
  }
}
