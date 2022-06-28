using FluentAssertions;
using Serilog;
using System;
using System.Linq;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {
    public class GetEthnicitiesAsyncTests : ReferralServiceTests
    {
      public GetEthnicitiesAsyncTests(
        ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture)
      {
        Log.Logger = new LoggerConfiguration()
        .WriteTo.TestOutput(testOutputHelper)
        .CreateLogger();
      }

      private void RemoveEthnicity(Guid Id)
      {
        _context.Ethnicities
          .Remove(_context.Ethnicities.Single(e => e.Id == Id));
        _context.SaveChanges();
      }

      [Fact]
      public async void MskReferralSource_OverrideDisplayAndGroupName()
      {
        // arrange
        Entities.Ethnicity ethnicity = RandomEntityCreator
          .CreateRandomEthnicty();
        _context.Ethnicities.Add(ethnicity);
        _context.SaveChanges();

        Entities.EthnicityOverride pharmacyOverride = RandomEntityCreator
          .CreateRandomEthnictyOverride(
            ethnicity.Id,
            displayName: "OverrideDisplayNameMsk",
            groupName: "OverrideGroupNameMsk",
            referralSource: ReferralSource.Pharmacy);
        _context.EthnicityOverrides.Add(pharmacyOverride);

        Entities.EthnicityOverride mskOverride = RandomEntityCreator
          .CreateRandomEthnictyOverride(
            ethnicity.Id,
            displayName: "OverrideDisplayNameMsk",
            groupName: "OverrideGroupNameMsk",
            referralSource: ReferralSource.Msk);
        _context.EthnicityOverrides.Add(mskOverride);

        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var results = await _service.GetEthnicitiesAsync(
          mskOverride.ReferralSource);

        // assert
        var result = results.Single(e => e.Id == ethnicity.Id);
        result.Should().BeEquivalentTo(ethnicity, options => options          
          .Excluding(x => x.DisplayName)
          .Excluding(x => x.GroupName)
          .Excluding(x => x.Overrides));
        result.DisplayName.Should().Be(mskOverride.DisplayName);
        result.GroupName.Should().Be(mskOverride.GroupName);

        // clean up
        RemoveEthnicity(ethnicity.Id);
      }

      [Fact]
      public async void MskReferralSource_InactiveOverride_NoOverride()
      {
        // arrange
        Entities.Ethnicity ethnicity = RandomEntityCreator
          .CreateRandomEthnicty();
        _context.Ethnicities.Add(ethnicity);
        _context.SaveChanges();

        Entities.EthnicityOverride mskOverride = RandomEntityCreator
          .CreateRandomEthnictyOverride(
            ethnicity.Id,
            displayName: "OverrideDisplayNameMsk",
            groupName: "OverrideGroupNameMsk",            
            isActive: false,
            referralSource: ReferralSource.Msk);
        _context.EthnicityOverrides.Add(mskOverride);

        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var results = await _service.GetEthnicitiesAsync(
          mskOverride.ReferralSource);

        // assert
        var result = results.Single(e => e.Id == ethnicity.Id);
        result.Should().BeEquivalentTo(ethnicity, options => options
          .Excluding(x => x.Overrides));

        // clean up
        RemoveEthnicity(ethnicity.Id);
      }
    }
  }
}
