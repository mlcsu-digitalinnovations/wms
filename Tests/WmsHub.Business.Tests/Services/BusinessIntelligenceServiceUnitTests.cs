using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.BusinessIntelligence;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using Xunit;
using Provider = WmsHub.Business.Entities.Provider;
using ProviderSubmission = WmsHub.Business.Entities.ProviderSubmission;
using Referral = WmsHub.Business.Entities.Referral;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class BusinessIntelligenceServiceUnitTests
  : ServiceTestsBase, IDisposable
{
  protected readonly DatabaseContext _context;
  private readonly Mock<ILogger> _mockLogger = new();
  private readonly Mock<IMapper> _mockMapper = new();
  private readonly Mock<IOptions<BusinessIntelligenceOptions>> _mockOptions =
    new();
  private readonly Mock<BusinessIntelligenceOptions> _mockOptionsValues =
    new();
  private BusinessIntelligenceService _service;

  protected Referral _referral1;
  protected Referral _referral2;
  protected EthnicityGrouping _ethnicityGrouping1;
  protected EthnicityGrouping _ethnicityGrouping2;

  public BusinessIntelligenceServiceUnitTests(ServiceFixture serviceFixture)
    : base(serviceFixture)
  {
    _context = new DatabaseContext(_serviceFixture.Options);
    _mockLogger = new Mock<ILogger>();
    _mockMapper = new Mock<IMapper>();
    _mockOptionsValues.Setup(t => t.ProviderSubmissionEndedStatusesValue)
      .Returns($"{ReferralStatus.ProviderRejected}," +
        $"{ReferralStatus.ProviderDeclinedByServiceUser}," +
        $"{ReferralStatus.ProviderTerminated}");
    _mockOptions.Setup(t => t.Value).Returns(_mockOptionsValues.Object);

    CleanUp();
  }

  public void Dispose()
  {
    CleanUp();
  }

  protected void CleanUp()
  {
    _context.ProviderSubmissions
      .RemoveRange(_context.ProviderSubmissions);
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.Providers.RemoveRange(_context.Providers);
    _context.StaffRoles.RemoveRange(_context.StaffRoles);
    _context.SaveChanges();
    _context.ProviderSubmissions.Count().Should().Be(0);
    _context.Referrals.Count().Should().Be(0);
    _context.Providers.Count().Should().Be(0);
    _context.StaffRoles.Count().Should().Be(0);
  }

  protected void AddStaffRolesInDatabase()
  {
    _context.Add(RandomEntityCreator.CreateRandomStaffRole(
      displayName: "Doctor",
      displayOrder: 1,
      isActive: true));
    _context.Add(RandomEntityCreator.CreateRandomStaffRole(
      displayName: "Ambulance Worker",
      displayOrder: 2,
      isActive: true));
    _context.Add(RandomEntityCreator.CreateRandomStaffRole(
      displayName: "Nurse",
      displayOrder: 3,
      isActive: true));
    _context.Add(RandomEntityCreator.CreateRandomStaffRole(
      displayName: "Porter",
      displayOrder: 4,
      isActive: true));

    _context.SaveChanges();
  }

  protected Provider AddProviderInDatabase(string providerName)
  {
    Provider provider = RandomEntityCreator.CreateRandomProvider(
      id: Guid.NewGuid(),
      isActive: true,
      isLevel1: true,
      isLevel2: true,
      isLevel3: true,
      name: providerName);

    _context.Add(provider);
    _context.SaveChanges();

    return provider;
  }

  protected Referral AddReferralInDatabase(
    Guid providerId,
    EthnicityGrouping ethnictyGrouping,
    int offset,
    Guid id = default,
    string ubrn = null,
    DateTimeOffset modifiedAt = default)
  {
    Referral referral = RandomEntityCreator.CreateRandomReferral(
      dateOfReferral: DateTime.Now.AddDays(offset),
      ethnicity: ethnictyGrouping.Ethnicity,
      serviceUserEthnicity: ethnictyGrouping.ServiceUserEthnicity,
      serviceUserEthnicityGroup: ethnictyGrouping.ServiceUserEthnicityGroup,
      hasDiabetesType1: true,
      hasDiabetesType2: false,
      hasHypertension: true,
      modifiedByUserId: Guid.Parse("fafc7655-89b7-42a3-bdf7-c57c72cd1d41"),
      referringGpPracticeName: "Referrer One",
      deprivation: "IMD2",
      ubrn: ubrn,
      id: id,
      modifiedAt: modifiedAt,
      providerId: providerId);

    _context.Add(referral);
    _context.SaveChanges();

    return referral;
  }

  protected ProviderSubmission AddProviderSubmissionInDatabase(
    Guid providerId,
    Guid referralId,
    int? coaching = null,
    DateTimeOffset? date = null,
    int? measure = null,
    decimal? weight = null,
    DateTimeOffset modifiedAt = default)
  {
    ProviderSubmission providerSubmission =
      RandomEntityCreator.CreateProviderSubmission(
        coaching: coaching ?? 5,
        date: date ?? DateTime.Now.AddDays(-28),
        measure: measure ?? 1,
        weight: weight ?? 87,
        providerId: providerId,
        isActive: true,
        referralId: referralId,
        modifiedAt: modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt
        );

    _context.Add(providerSubmission);
    _context.SaveChanges();

    return providerSubmission;
  }

  protected void SetupAsync()
  {
    AddStaffRolesInDatabase();

    Provider p1 = AddProviderInDatabase("Provider One");
    Provider p2 = AddProviderInDatabase("Provider Two");

    _ethnicityGrouping1 =
      Generators.GenerateEthnicityGrouping(new Random());
    _ethnicityGrouping2 =
      Generators.GenerateEthnicityGrouping(new Random());

    // Add Referrals
    _referral1 =
      AddReferralInDatabase(p1.Id, _ethnicityGrouping1, -30, Guid.NewGuid());
    _referral2 =
      AddReferralInDatabase(p2.Id, _ethnicityGrouping2, -30, Guid.NewGuid());

    AddProviderSubmissionInDatabase(
      p1.Id,
      _referral1.Id,
      5,
      DateTime.Now.AddDays(-28), 1, 87);
    AddProviderSubmissionInDatabase(
      p1.Id,
      _referral1.Id,
      6,
      DateTime.Now.AddDays(-14), 7, 97);
    AddProviderSubmissionInDatabase(
      p2.Id,
      _referral2.Id,
      5,
      DateTime.Now.AddDays(-28), 1, 87);
    AddProviderSubmissionInDatabase(
      p2.Id,
      _referral2.Id,
      6,
      DateTime.Now.AddDays(-14), 7, 97);

    _service =
      new BusinessIntelligenceService(
        _context,
        _mockMapper.Object,
        _mockOptions.Object,
        _mockLogger.Object);
  }

  public class GetAnonymisedReferralsBySubmissionDateTests :
    BusinessIntelligenceServiceUnitTests
  {
    public GetAnonymisedReferralsBySubmissionDateTests(
      ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task Returns_valid_referral()
    {
      // Arrange.
      SetupAsync();

      // Act.
      IEnumerable<AnonymisedReferral> result =
        await _service.GetAnonymisedReferralsBySubmissionDate();

      // Assert.
      using (new AssertionScope())
      {
        result.Any().Should().BeTrue();
      }
    }
  }

  public class GetAnonymisedReferralsByProviderSubmissionModifiedAtTests :
    BusinessIntelligenceServiceUnitTests
  {
    public GetAnonymisedReferralsByProviderSubmissionModifiedAtTests(
      ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Theory]
    [InlineData(-20, 2)]
    [InlineData(-30, 2)]
    public async Task Returns_valid_referral(int offset, int expectedCount)
    {
      // Arrange.
      SetupAsync();

      // Act.
      IEnumerable<AnonymisedReferral> result =
        await _service.GetAnonymisedReferralsByProviderSubmissionsModifiedAt(
          DateTimeOffset.Now.AddDays(offset));

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNullOrEmpty();
        result.First().ProviderSubmissions.Should()
          .NotBeNullOrEmpty()
          .And.HaveCount(expectedCount);
      }
    }

    [Fact]
    public async Task Returns_zero_referral_when_no_changes()
    {
      // Arrange.
      SetupAsync();

      // Act.
      IEnumerable<AnonymisedReferral> result =
        await _service.GetAnonymisedReferralsByProviderSubmissionsModifiedAt(
          DateTimeOffset.Now);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeNullOrEmpty();
      }
    }
  }

  public class GetAnonymisedReprocessedReferralsBySubmissionDateTests :
    BusinessIntelligenceServiceUnitTests
  {
    public GetAnonymisedReprocessedReferralsBySubmissionDateTests(
      ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task Returns_valid_referral()
    {
      // Arrange.
      SetupAsync();

      // Act.
      IEnumerable<AnonymisedReferral> result =
        await _service.GetAnonymisedReferralsBySubmissionDate();

      // Assert.
      using (new AssertionScope())
      {
        result.Any().Should().BeTrue();
      }
    }

    [Fact]
    public async Task Returns_valid_referral_with_ethnicities()
    {
      // Arrange.
      SetupAsync();

      // Act.
      IEnumerable<AnonymisedReferral> results =
        await _service.GetAnonymisedReferralsBySubmissionDate();

      // Assert.
      using (new AssertionScope())
      {
        results.Should().NotBeNull();
        Referral referral1 = _context.Referrals.First();
        Referral referral2 = _context.Referrals.Skip(1).Take(1).Single();
        foreach (AnonymisedReferral r in results)
        {
          r.Ethnicity.Should().NotBeNullOrWhiteSpace();
          r.ServiceUserEthnicity.Should().NotBeNullOrWhiteSpace();
          r.ServiceUserEthnicityGroup.Should().NotBeNullOrWhiteSpace();
          if (r.Ubrn == _referral1.Ubrn)
          {
            r.Ethnicity.Should().Be(_ethnicityGrouping1.Ethnicity);
            r.ServiceUserEthnicity.Should()
              .Be(_ethnicityGrouping1.ServiceUserEthnicity);
            r.ServiceUserEthnicityGroup.Should()
              .Be(_ethnicityGrouping1.ServiceUserEthnicityGroup);
          }
          else if (r.Ubrn == _referral2.Ubrn)
          {
            r.Ethnicity.Should().Be(_ethnicityGrouping2.Ethnicity);
            r.ServiceUserEthnicity.Should()
              .Be(_ethnicityGrouping2.ServiceUserEthnicity);
            r.ServiceUserEthnicityGroup.Should()
              .Be(_ethnicityGrouping2.ServiceUserEthnicityGroup);
          }
        }
      }
    }
  }

  public class GetAnonymisedReferralsUnitTests :
    BusinessIntelligenceServiceUnitTests
  {
    public GetAnonymisedReferralsUnitTests(
      ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task Returns_valid_referral()
    {
      // Arrange.
      SetupAsync();

      // Act.
      IEnumerable<AnonymisedReferral> result =
        await _service.GetAnonymisedReferrals();

      // Assert.
      using (new AssertionScope())
      {
        result.Any().Should().BeTrue();
      }
    }

    [Fact]
    public async Task Returns_valid_referral_with_ethnicities()
    {
      // Arrange.
      SetupAsync();

      // Act.
      IEnumerable<AnonymisedReferral> results =
        await _service.GetAnonymisedReferrals();

      // Assert.
      using (new AssertionScope())
      {
        results.Should().NotBeNull();
        foreach (AnonymisedReferral r in results)
        {
          r.Ethnicity.Should().NotBeNullOrWhiteSpace();
          r.ServiceUserEthnicity.Should().NotBeNullOrWhiteSpace();
          r.ServiceUserEthnicityGroup.Should().NotBeNullOrWhiteSpace();
          if (r.Ubrn == _referral1.Ubrn)
          {
            r.Ethnicity.Should().Be(_ethnicityGrouping1.Ethnicity);
            r.ServiceUserEthnicity.Should()
              .Be(_ethnicityGrouping1.ServiceUserEthnicity);
            r.ServiceUserEthnicityGroup.Should()
              .Be(_ethnicityGrouping1.ServiceUserEthnicityGroup);
          }
          else if (r.Ubrn == _referral2.Ubrn)
          {
            r.Ethnicity.Should().Be(_ethnicityGrouping2.Ethnicity);
            r.ServiceUserEthnicity.Should()
              .Be(_ethnicityGrouping2.ServiceUserEthnicity);
            r.ServiceUserEthnicityGroup.Should()
              .Be(_ethnicityGrouping2.ServiceUserEthnicityGroup);
          }
        }
      }
    }
  }

  public class GetAnonymisedReferralsByModifiedAtUnitTests :
    BusinessIntelligenceServiceUnitTests
  {
    public GetAnonymisedReferralsByModifiedAtUnitTests(
      ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Theory]
    [InlineData(-20, 2)]
    [InlineData(-30, 2)]
    [InlineData(0, 0)]
    public async Task Returns_valid_referral(int offset, int expectedCount)
    {
      // Arrange.
      SetupAsync();

      // Act.
      IEnumerable<AnonymisedReferral> result =
        await _service.GetAnonymisedReferralsByModifiedAt(
          DateTimeOffset.Now.AddDays(offset));

      // Assert.
      using (new AssertionScope())
      {
        result.Count().Should().Be(expectedCount);
      }
    }

    [Fact]
    public async Task Returns_valid_referral_with_ethnicities()
    {
      // Arrange.
      SetupAsync();

      // Act.
      IEnumerable<AnonymisedReferral> results =
        await _service.GetAnonymisedReferralsByModifiedAt(
          DateTimeOffset.Now.AddDays(-30));

      // Assert.
      using (new AssertionScope())
      {
        results.Should().NotBeNull();
        foreach (AnonymisedReferral r in results)
        {
          r.Ethnicity.Should().NotBeNullOrWhiteSpace();
          r.ServiceUserEthnicity.Should().NotBeNullOrWhiteSpace();
          r.ServiceUserEthnicityGroup.Should().NotBeNullOrWhiteSpace();
          if (r.Ubrn == _referral1.Ubrn)
          {
            r.Ethnicity.Should().Be(_ethnicityGrouping1.Ethnicity);
            r.ServiceUserEthnicity.Should()
              .Be(_ethnicityGrouping1.ServiceUserEthnicity);
            r.ServiceUserEthnicityGroup.Should()
              .Be(_ethnicityGrouping1.ServiceUserEthnicityGroup);
          }
          else if (r.Ubrn == _referral2.Ubrn)
          {
            r.Ethnicity.Should().Be(_ethnicityGrouping2.Ethnicity);
            r.ServiceUserEthnicity.Should()
              .Be(_ethnicityGrouping2.ServiceUserEthnicity);
            r.ServiceUserEthnicityGroup.Should()
              .Be(_ethnicityGrouping2.ServiceUserEthnicityGroup);
          }
        }
      }
    }
  }

  public class GetAnonymisedReferralsChangedFromDateTests :
   BusinessIntelligenceServiceUnitTests
  {
    public GetAnonymisedReferralsChangedFromDateTests(
      ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _service = new(_context, _mockMapper.Object, _mockOptions.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ReturnsReferral_ModifiedAfterDate()
    {
      // Arrange.
      Referral storedReferral = RandomEntityCreator.CreateRandomReferral(
        modifiedAt: DateTimeOffset.UtcNow.AddDays(-2));

      _context.Referrals.Add(storedReferral);

      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<AnonymisedReferral> output = await _service
        .GetAnonymisedReferralsChangedFromDate(DateTimeOffset.UtcNow.AddDays(-5));

      // Assert.
      output.Should().HaveCount(1);
      output.Single().Id.Should().Be(storedReferral.Id);
    }

    [Fact]
    public async Task ReturnsReferral_ReceivedSubmissionAfterDate()
    {
      // Arrange.
      Referral storedReferral = RandomEntityCreator.CreateRandomReferral(
        modifiedAt: DateTimeOffset.UtcNow.AddDays(-10));

      storedReferral.ProviderSubmissions.Add(
        RandomEntityCreator.CreateProviderSubmission(
          referralId: storedReferral.Id, date: DateTimeOffset.UtcNow.AddDays(-2)));

      _context.Referrals.Add(storedReferral);

      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<AnonymisedReferral> output = await _service
        .GetAnonymisedReferralsChangedFromDate(DateTimeOffset.UtcNow.AddDays(-5));

      // Assert.
      output.Should().HaveCount(1);
      output.Single().Id.Should().Be(storedReferral.Id);
    }

    [Fact]
    public async Task ReturnsReferralsWithoutDuplicates_ModifiedAndReceivedSubmisssionAfterDate()
    {
      // Arrange.
      Referral referral1 = RandomEntityCreator.CreateRandomReferral(
        modifiedAt: DateTimeOffset.UtcNow.AddDays(-2));

      _context.Referrals.Add(referral1);

      Referral referral2 = RandomEntityCreator.CreateRandomReferral(
        modifiedAt: DateTimeOffset.UtcNow.AddDays(-10));

      referral2.ProviderSubmissions.Add(
        RandomEntityCreator.CreateProviderSubmission(
          referralId: referral2.Id, date: DateTimeOffset.UtcNow.AddDays(-2)));

      _context.Referrals.Add(referral2);

      Referral referral3 = RandomEntityCreator.CreateRandomReferral(
        modifiedAt: DateTimeOffset.UtcNow.AddDays(-2));

      referral3.ProviderSubmissions.Add(
        RandomEntityCreator.CreateProviderSubmission(
          referralId: referral3.Id, date: DateTimeOffset.UtcNow.AddDays(-2)));

      _context.Referrals.Add(referral3);

      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<AnonymisedReferral> output = await _service
        .GetAnonymisedReferralsChangedFromDate(DateTimeOffset.UtcNow.AddDays(-5));

      // Assert.
      output.Should().HaveCount(3);
      output.Select(r => r.Id).Should().Contain(
        new List<Guid>() { referral1.Id, referral2.Id, referral3.Id });
    }
  }
}
