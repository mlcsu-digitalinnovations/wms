using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {
   
    public class UpdateGeneralReferralTests : ReferralServiceTests, IDisposable
    {
      Mock<IProviderService> _mockProviderService = new();
      ReferralService _serviceMockProviderService;

      public UpdateGeneralReferralTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
        _serviceMockProviderService = new ReferralService(
          _context,
          _serviceFixture.Mapper,
          _mockProviderService.Object,
          _mockDeprivationService.Object,
          _mockPostcodeService.Object,
          _mockPatientTriageService.Object,
          _mockOdsOrganisationService.Object);
      }

      public void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async Task ParamNull_Exception()
      {
        // arrange
        GeneralReferralUpdate model = null;

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.UpdateGeneralReferral(model));

        // assert
        ex.Should().BeOfType<ArgumentNullException>();
      }

      [Fact]
      public async Task ReferralIdDoesNotExist_Exception()
      {
        // arrange
        GeneralReferralUpdate model = new()
        {
          Id = Guid.Empty
        };

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.UpdateGeneralReferral(model));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
      }

      [Fact]
      public async Task NhsNumberChanged_Exception()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        GeneralReferralUpdate model = new()
        {
          Id = referral.Id,
          NhsNumber = Generators.GenerateNhsNumber(_rnd, referral.NhsNumber)
        };

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.UpdateGeneralReferral(model));

        // assert
        ex.Should().BeOfType<NhsNumberUpdateReferralMismatchException>();
      }

      [Theory]
      [MemberData(nameof(InvalidStatuses))]
      public async Task ReferralInvalidStatus_Exception(
        ReferralStatus referralStatus)
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: referralStatus);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        GeneralReferralUpdate model = new()
        {
          Id = referral.Id,
          NhsNumber = referral.NhsNumber
        };

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.UpdateGeneralReferral(model));

        // assert
        ex.Should().BeOfType<ReferralInvalidStatusException>();
      }

      public static TheoryData<ReferralStatus> InvalidStatuses
      {
        get
        {
          List<ReferralStatus> validStatuses = new()
          {
            ReferralStatus.New,
            ReferralStatus.RmcCall,
            ReferralStatus.RmcDelayed,
            ReferralStatus.TextMessage1,
            ReferralStatus.TextMessage2,
            ReferralStatus.ChatBotCall1,
            ReferralStatus.ChatBotCall2,
            ReferralStatus.ChatBotTransfer
          };

          var invalidStatuses = new TheoryData<ReferralStatus>();
          foreach (var referralStatus in Enum.GetValues<ReferralStatus>())
          {
            if (!validStatuses.Contains(referralStatus))
            {
              invalidStatuses.Add(referralStatus);
            }
          }
          return invalidStatuses;
        }
      }

      [Fact]
      public async Task NoProvidersAvailable_Exception()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral();
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        _mockProviderService
          .Setup(x => x.GetProvidersAsync(It.IsAny<TriageLevel>()))
          .ReturnsAsync(new List<Business.Models.Provider>());

        var model = RandomModelCreator.CreateRandomGeneralReferralUpdate(
          id: referral.Id,
          nhsNumber: referral.NhsNumber);

        // act
        var ex = await Record.ExceptionAsync(
          () => _serviceMockProviderService.UpdateGeneralReferral(model));

        // assert
        ex.Should().BeOfType<NoProviderChoicesFoundException>();
      }

      [Theory]
      [InlineData(TriageLevel.High)]
      [InlineData(TriageLevel.Medium)]
      public async Task NoLevel3or2Providers_OfferedCompletionLevel1(
        TriageLevel triageLevel)
      {
        // arrange...

        // ...the referral to be triaged to the tested level
        _mockScoreResult.Setup(t => t.TriagedCompletionLevel)
          .Returns(triageLevel);

        // ...there to be no providers available at tested level
        var noProviders = new List<Business.Models.Provider>();
        _mockProviderService
          .Setup(x => x.GetProvidersAsync(
            It.Is<TriageLevel>(t => t == triageLevel)))
          .ReturnsAsync(noProviders);

        // ...there to be one level 1 (Low) provider
        var level1Provider = RandomModelCreator.CreateRandomProvider(
          level1: true, level2: false, level3: false);
        var level1Providers = new List<Business.Models.Provider>()
          { level1Provider };
        _mockProviderService
          .Setup(x => x.GetProvidersAsync(
            It.Is<TriageLevel>(t => t == TriageLevel.Low)))
          .ReturnsAsync(level1Providers);

        // act
        var result = await _serviceMockProviderService
          .CreateGeneralReferral(_validGeneralReferralCreate);

        // assert...

        // ...the referral returned is the level 1 provider
        result.Should().BeOfType<Referral>();
        result.Providers.Count().Should().Be(1);
        result.Providers.Single().Should().BeEquivalentTo(level1Provider);

        // ...the tested triaged level providers are requested
        _mockProviderService.Verify(
          x => x.GetProvidersAsync(It.Is<TriageLevel>(
            t => t == triageLevel)),
          Times.Once);

        // ...the level 1 providers are requested
        _mockProviderService.Verify(
          x => x.GetProvidersAsync(It.Is<TriageLevel>(
            t => t == TriageLevel.Low)),
          Times.Once);

        // ... the created referral has the expected triaged and offered level
        var referral = _context.Referrals.Single(r => r.Id == result.Id);
        referral.TriagedCompletionLevel = $"{(int)triageLevel}";
        referral.OfferedCompletionLevel = $"{(int)TriageLevel.Low}";
      }
    }
  }
}
