using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Extensions;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class ProviderServiceUnitTests : ServiceTestsBase, IDisposable
{
  private readonly DatabaseContext _context;
  private readonly ProviderService _providerService;

  public ProviderServiceUnitTests(ServiceFixture serviceFixture) : base(serviceFixture)
  {
    _context = new DatabaseContext(_serviceFixture.Options);

    _context.Providers.Add(RandomEntityCreator.CreateRandomProvider(
      details: new() 
      { 
        new() 
        { 
          IsActive = true,
          Section = "Level1TestSection",
          TriageLevel = 1,
          Value = "Level1Value"
        } 
      },
      isLevel1: true,
      isLevel2: false,
      isLevel3: false,
      name: "Level1"));

    _context.Providers.Add(RandomEntityCreator.CreateRandomProvider(
      details: new()
      {
        new()
        {
          IsActive = true,
          Section = "Level2TestSection",
          TriageLevel = 2,
          Value = "Level2Value"
        }
      },

      isLevel1: false,
      isLevel2: true,
      isLevel3: false,
      name: "Level2"));

    _context.Providers.Add(RandomEntityCreator.CreateRandomProvider(
      details: new()
      {
        new()
        {
          IsActive = true,
          Section = "Level3TestSection",
          TriageLevel = 3,
          Value = "Level3Value"
        }
      },

      isLevel1: false,
      isLevel2: false,
      isLevel3: true,
      name: "Level3"));

    _context.Providers.Add(RandomEntityCreator.CreateRandomProvider(
      details: new()
      {
        new()
        {
          IsActive = true,
          Section = "Level123TestSection1",
          TriageLevel = 1,
          Value = "Level123Value1"
        },
        new()
        {
          IsActive = true,
          Section = "Level123TestSection2",
          TriageLevel = 2,
          Value = "Level123Value2"
        },
        new()
        {
          IsActive = true,
          Section = "Level123TestSection4",
          TriageLevel = 3,
          Value = "Level123Value3"
        }
      },
      isLevel1: true,
      isLevel2: true,
      isLevel3: true,
      name: "Level123"));

    _context.SaveChanges();

    _providerService = new ProviderService(
      _context,
      _serviceFixture.Mapper,
      TestConfiguration.CreateProviderOptions())
    {
      User = GetClaimsPrincipal(),
    };
  }

  public void Dispose()
  {
    _context.ProviderDetails.RemoveRange(_context.ProviderDetails);
    _context.Providers.RemoveRange(_context.Providers);
    _context.SaveChanges();
  }

  public class GetProvidersAsyncTests : ProviderServiceUnitTests
  {
    public GetProvidersAsyncTests(ServiceFixture serviceFixture) : base(serviceFixture)
    { }

    [Fact]
    public async Task HighTriageLevel_ReturnsHighLevelProviders()
    {
      // Arrange.
      int expectedCount = 2;

      // Act.
      IEnumerable<Provider> results = await _providerService.GetProvidersAsync(TriageLevel.High);

      // Assert.
      results.Should().HaveCount(expectedCount)
        .And.AllSatisfy(x => x.Level3.Should().BeTrue());
    }

    [Fact]
    public async Task LowTriageLevel_ReturnsLowLevelProviders()
    {
      // Arrange.
      int expectedCount = 2;

      // Act.
      IEnumerable<Provider> results = await _providerService.GetProvidersAsync(TriageLevel.Low);

      // Assert.
      results.Should().HaveCount(expectedCount)
        .And.AllSatisfy(x => x.Level1.Should().BeTrue());
    }

    [Fact]
    public async Task MediumTriageLevel_ReturnsMediumLevelProviders()
    {
      // Arrange.
      int expectedCount = 2;

      // Act.
      IEnumerable<Provider> results = await _providerService.GetProvidersAsync(TriageLevel.Medium);

      // Assert.
      results.Should().HaveCount(expectedCount)
        .And.AllSatisfy(x => x.Level2.Should().BeTrue());
    }
  }



  public class GetServiceUsersTests : ProviderServiceUnitTests
  {
    public GetServiceUsersTests(ServiceFixture serviceFixture)
    : base(serviceFixture)
    {
      // Arrange.
      _context.Referrals.RemoveRange(_context.Referrals);
    }

    [Fact]
    public async Task ReferralWithoutProviderUbrn_NotReturned()
    {
      // Arrange.
      Entities.Referral referralWithoutProviderUbrn = RandomEntityCreator
        .CreateRandomReferral(
          providerId: Guid.Parse(TEST_USER_ID),
          status: Enums.ReferralStatus.ProviderAwaitingStart);
      referralWithoutProviderUbrn.ProviderUbrn = null;
      _context.Referrals.Add(referralWithoutProviderUbrn);

      Entities.Referral referralWithProviderUbrn = RandomEntityCreator
        .CreateRandomReferral(
          providerId: Guid.Parse(TEST_USER_ID),
          status: Enums.ReferralStatus.ProviderAwaitingStart);
      _context.Referrals.Add(referralWithProviderUbrn);

      _context.SaveChanges();

      // Act.
      IEnumerable<ServiceUser> result = await _providerService
        .GetServiceUsers();

      // Assert.
      result.Should().HaveCount(1);
      result.Single().Ubrn.Should().Be(referralWithProviderUbrn.Ubrn);
    }

    [Fact]
    public async Task GpReferralProviderAwaitingStart()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        consentForFutureContactForEvaluation: true,
        hasALearningDisability: true,
        hasAPhysicalDisability: true,
        hasDiabetesType1: true,
        hasDiabetesType2: true,
        hasHypertension: true,
        hasRegisteredSeriousMentalIllness: true,
        isVulnerable: true,
        // These properties are set when the referral has reached the status
        // of GpReferralProviderAwaitingStart.
        dateOfProviderSelection: DateTimeOffset.Now,
        offeredCompletionLevel: ((int)Enums.TriageLevel.Low).ToString(),
        providerId: Guid.Parse(TEST_USER_ID),
        status: Enums.ReferralStatus.ProviderAwaitingStart,
        triagedCompletionLevel: ((int)Enums.TriageLevel.High).ToString());

      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.Entry(referral).State =
        Microsoft.EntityFrameworkCore.EntityState.Detached;

      // Act.
      IEnumerable<ServiceUser> result = await _providerService
        .GetServiceUsers();

      // Assert.
      result.Count().Should().Be(1);
      StandardPropertyEquivalentAsserts(result.Single(), referral);
    }

    [Fact]
    public async Task SelfReferralProviderAwaitingStart()
    {
      // Arrange.
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        consentForFutureContactForEvaluation: true,
        hasALearningDisability: true,
        hasAPhysicalDisability: true,
        hasDiabetesType1: true,
        hasDiabetesType2: true,
        hasHypertension: true,
        hasRegisteredSeriousMentalIllness: true,
        isVulnerable: true,
        // these properties are set when the referral has reached the status
        // of GpReferralProviderAwaitingStart
        dateOfProviderSelection: DateTimeOffset.Now,
        offeredCompletionLevel: ((int)Enums.TriageLevel.Low).ToString(),
        providerId: Guid.Parse(TEST_USER_ID),
        status: Enums.ReferralStatus.ProviderAwaitingStart,
        triagedCompletionLevel: ((int)Enums.TriageLevel.High).ToString());
      _context.Referrals.Add(referral);
      _context.SaveChanges();
      _context.Entry(referral).State =
        Microsoft.EntityFrameworkCore.EntityState.Detached;

      // Act.
      IEnumerable<ServiceUser> result = await _providerService
        .GetServiceUsers();

      // Assert.
      result.Count().Should().Be(1);
      StandardPropertyEquivalentAsserts(result.Single(), referral);
    }

    private void StandardPropertyEquivalentAsserts(
      ServiceUser serviceUser, Entities.Referral referral)
    {
      serviceUser.Address1.Should().Be(referral.Address1);
      serviceUser.Address2.Should().Be(referral.Address2);
      serviceUser.Address3.Should().Be(referral.Address3);
      serviceUser.Age.Should().Be(referral.DateOfBirth.GetAge());
      serviceUser.Bmi.Should().Be(referral.CalculatedBmiAtRegistration);
      serviceUser.BmiDate.Should().Be(referral.DateOfBmiAtRegistration.Value);
      serviceUser.DateOfReferral.Should().Be(referral.DateOfReferral.Value);
      serviceUser.Email.Should().Be(referral.Email);
      serviceUser.Ethnicity.Should().Be(referral.Ethnicity);
      serviceUser.FamilyName.Should().Be(referral.FamilyName);
      serviceUser.GivenName.Should().Be(referral.GivenName);
      serviceUser.HasDiabetesType1.Should()
        .Be(referral.HasDiabetesType1.Value);
      serviceUser.HasDiabetesType2.Should()
        .Be(referral.HasDiabetesType1.Value);
      serviceUser.HasHypertension.Should()
        .Be(referral.HasHypertension.Value);
      serviceUser.HasLearningDisability.Should()
        .Be(referral.HasALearningDisability.Value);
      serviceUser.HasPhysicalDisability.Should()
        .Be(referral.HasAPhysicalDisability.Value);
      serviceUser.HasRegisteredSeriousMentalIllness.Should()
        .Be(referral.HasRegisteredSeriousMentalIllness.Value);
      serviceUser.Height.Should().Be(referral.HeightCm.Value);
      serviceUser.IsVulnerable.Should().Be(referral.IsVulnerable.Value);
      serviceUser.Mobile.Should().Be(referral.Mobile);
      serviceUser.Postcode.Should().Be(referral.Postcode);
      serviceUser.ProviderSelectedDate.Should()
        .Be(referral.DateOfProviderSelection.Value);
      serviceUser.ReferringGpPracticeNumber.Should()
        .Be(referral.ReferringGpPracticeNumber);
      serviceUser.SexAtBirth.Should().Be(referral.Sex);
      serviceUser.Telephone.Should().Be(referral.Telephone);
      serviceUser.TriagedLevel.Should()
        .Be((int)Enum.Parse(typeof(Enums.TriageLevel),
          referral.OfferedCompletionLevel));
      serviceUser.Ubrn.Should().Be(referral.Ubrn);
      serviceUser.Ubrn.Should().NotBeNullOrEmpty();
    }
  }
}

