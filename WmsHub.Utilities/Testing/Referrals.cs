using System;
using System.Collections.Generic;
using System.Linq;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;

namespace WmsHub.Utilities.Testing;
internal class Referrals
{
  private readonly DatabaseContext _databaseContext;
  List<PublicReferralTestData> _testData = new();

  internal Referrals(DatabaseContext databaseContext)
  {
    _databaseContext = databaseContext;
  }

  internal void PublicReferralUi()
  {
    LoadData();
    DeleteTestData();
    CreateTestData();
  }

  void CreateTestData()
  {
    foreach (var item in _testData)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: item.DateOfProviderSelection,
        dateOfReferral: item.DateOfReferral,
        dateStartedProgramme: item.DateStartedProgramme,
        familyName: item.FamilyName,
        givenName: item.GivenName,
        nhsNumber: item.NhsNumber,
        providerId: item.ProviderId ?? default,
        referralSource: item.ReferralSource,
        status: item.Status);

      _databaseContext.Referrals.Add(referral);
    }
    int creations = _databaseContext.SaveChanges();

    Console.WriteLine($"Created {creations} referrals.");
  }

  void DeleteTestData()
  {
    foreach (var item in _testData)
    {
      _databaseContext.Referrals
        .Where(x => x.NhsNumber == item.NhsNumber)
        .ToList()
        .ForEach(x => _databaseContext.Referrals.Remove(x));
    }

    int deletions = _databaseContext.SaveChanges();

    Console.WriteLine($"Deleted {deletions} referrals.");
  }

  void LoadData()
  {
    _testData = new()
    {
      new("Julius JOYCE","9686367454","Cancelled","ElectiveCare","2023-04-12",null,null,null),
      new("Joanne RIGBY","9686367535","Complete","ElectiveCare","2023-04-12",null,null,null),
      new("Tania ROUE","9686367586","Complete","ElectiveCare","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12",null),
      new("Brett GADD","9686368914","Complete","ElectiveCare","2023-03-02","2D11868B-6200-4C14-9F49-2A17D735A573","2023-03-02",null),
      new("Angela SHAIN","9686367594","Complete","ElectiveCare","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12","2023-04-12"),
      new("Joel MELLOY","9686368892","Complete","ElectiveCare","2022-08-04","2D11868B-6200-4C14-9F49-2A17D735A573","2022-08-04","2022-08-04"),
      new("Cecil DOLBY","9686367497","New","ElectiveCare","2023-04-12",null,null,null),
      new("Arnold BOOKER","9686367500","ProviderAwaitingStart","ElectiveCare","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12",null),
      new("Eli MILNER","9686367462","Complete","GeneralReferral","2023-04-12",null,null,null),
      new("Dwayne CASE","9686367403","Complete","GeneralReferral","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12",null),
      new("Tamsyn FAHEY","9686367527","Complete","GeneralReferral","2023-03-02","2D11868B-6200-4C14-9F49-2A17D735A573","2023-03-02",null),
      new("Martin KNEALE","9686367446","Complete","GeneralReferral","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12","2023-04-12"),
      new("Edwin EVERY","9686367470","Complete","GeneralReferral","2022-08-04","2D11868B-6200-4C14-9F49-2A17D735A573","2022-08-04","2022-08-04"),
      new("Carl HOYTE","9686367438","TextMessage1","GeneralReferral","2023-04-12",null,null,null),
      new("Adam TYNAN","9686367489","ProviderAccepted","GeneralReferral","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12",null),
      new("Gemma POLLEY","9686367543","TextMessage2","GpReferral","2023-04-12",null,null,null),
      new("Iain HUGHES","9686368906","ProviderStarted","GpReferral","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12","2023-04-12"),
      new("Zoe VIGGOR","9686367578","Complete","GpReferral","2023-04-12",null,null,null),
      new("Lilly POOLEY","9686367608","Complete","GpReferral","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12",null),
      new("Andrew ELLERY","9686367411","Complete","GpReferral","2023-03-02","2D11868B-6200-4C14-9F49-2A17D735A573","2023-03-02",null),
      new("Alice COFFEY","9686367616","Complete","GpReferral","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12","2023-04-12"),
      new("Elaine HANKEY","9686367519","Complete","GpReferral","2022-08-04","2D11868B-6200-4C14-9F49-2A17D735A573","2022-08-04","2022-08-04"),
      new("Dinah WEIR","9686368450","ChatBotCall1","Msk","2023-04-12",null,null,null),
      new("Otis NORMAN","9686368183","ProviderCompleted","Msk","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12","2023-04-12"),
      new("Doreas BRIDGE","9686368418","Complete","Msk","2023-04-12",null,null,null),
      new("Julie PALMAS","9686368426","Complete","Msk","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12",null),
      new("Mary ASHBY","9686368434","Complete","Msk","2023-03-02","2D11868B-6200-4C14-9F49-2A17D735A573","2023-03-02",null),
      new("Elisa POLAND","9686368469","Complete","Msk","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12","2023-04-12"),
      new("Jayne DEAVES","9686368442","Complete","Msk","2022-08-04","2D11868B-6200-4C14-9F49-2A17D735A573","2022-08-04","2022-08-04"),
      new("Joanne COLE","9686368701","ChatBotCall2","Pharmacy","2023-04-12",null,null,null),
      new("Antony FLYNN","9686368140","ProviderCompleted","Pharmacy","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12","2023-04-12"),
      new("Scott ANSTEY","9686368167","Complete","Pharmacy","2023-04-12",null,null,null),
      new("Roland READON","9686368175","Complete","Pharmacy","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12",null),
      new("Lloyd PENDRY","9686368191","Complete","Pharmacy","2023-03-02","2D11868B-6200-4C14-9F49-2A17D735A573","2023-03-02",null),
      new("Yasmin RILEY","9686368485","Complete","Pharmacy","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12","2023-04-12"),
      new("Rufus HUSTON","9686368132","Complete","Pharmacy","2022-08-04","2D11868B-6200-4C14-9F49-2A17D735A573","2022-08-04","2022-08-04"),
      new("Edwin TOBIN","9686368159","ChatBotTransfer","SelfReferral","2023-04-12",null,null,null),
      new("Edmond FORD","9686368205","ProviderCompleted","SelfReferral","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12","2023-04-12"),
      new("Greta HANNAH","9686368477","Complete","SelfReferral","2023-04-12",null,null,null),
      new("Ivan LYON","9686368647","Complete","SelfReferral","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12",null),
      new("Ramona WATKIN","9686369031","Complete","SelfReferral","2023-03-02","2D11868B-6200-4C14-9F49-2A17D735A573","2023-03-02",null),
      new("Alan MILLAR","9686368965","Complete","SelfReferral","2023-04-12","2D11868B-6200-4C14-9F49-2A17D735A573","2023-04-12","2023-04-12"),
      new("Rhoda DOBBIN","9686368663","Complete","SelfReferral","2022-08-04","2D11868B-6200-4C14-9F49-2A17D735A573","2022-08-04","2022-08-04")
    };

    Console.Write($"Loaded {_testData.Count} referrals.");
  }

  class PublicReferralTestData
  {
    public PublicReferralTestData(
      string name,
      string nhsNumber,
      string status,
      string referralSource,
      string dateOfReferralString,
      string providerId,
      string dateOfProviderSelectionString,
      string dateStartedProgrammeString)
    {
      if (dateOfProviderSelectionString != null)
      {
        DateOfProviderSelection = DateTime.Parse(dateOfProviderSelectionString);
      }
      if (dateStartedProgrammeString != null)
      {
        DateStartedProgramme = DateTime.Parse(dateStartedProgrammeString);
      }
      if (dateOfReferralString != null)
      {
        DateOfReferral = DateTime.Parse(dateOfReferralString);
      }

      string[] nameSplit = name.Split(" ");
      GivenName = nameSplit[0];
      FamilyName = nameSplit[1];

      NhsNumber = nhsNumber;
      if (providerId != null)
      {
        ProviderId = Guid.Parse(providerId);
      }
      ReferralSource = (ReferralSource)Enum
        .Parse(typeof(ReferralSource), referralSource);

      Status = (ReferralStatus)Enum
        .Parse(typeof(ReferralStatus), status);
    }

    public DateTimeOffset? DateOfProviderSelection { get; set; }
    public DateTimeOffset DateOfReferral { get; set; }
    public DateTimeOffset? DateStartedProgramme { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }
    public string NhsNumber { get; set; }
    public Guid? ProviderId { get; set; }
    public ReferralSource ReferralSource { get; set; }
    public ReferralStatus Status { get; set; }
  }
}
