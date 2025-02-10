using FluentAssertions;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Services;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {
    public class UpdateServiceUserEthnicityGroupAsync
      : ReferralServiceTests, IDisposable
    {
      private new readonly ReferralService _service;
      private readonly string _referralNotFoundExceptionMessage =
        "Unable to find a referral with an id of {referralId}.";
      private readonly string _referralProviderSelectedException =
        "The referral {referralId} has previously had its provider " +
        "selected {providerId}.";
      private readonly string _ethnicityNotFoundException =
        "An ethnicity group with a display name of {ethnicityGroup} cannot " +
        "be found.";

      public UpdateServiceUserEthnicityGroupAsync(
        ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      {
        _service = new ReferralService(
          _context,
          _serviceFixture.Mapper,
          null, // provider service
          _mockDeprivationService.Object,
          _mockLinkIdService.Object,
          _mockPostcodeIoService.Object,
          _mockPatientTriageService.Object,
          _mockOdsOrganisationService.Object,
          _mockGpDocumentProxyOptions.Object,
          _mockReferralTimelineOptions.Object,
          null,
          _log)
        {
          User = GetClaimsPrincipal()
        };

        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      public new void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async Task EmptyGuid_ArgumentException()
      {
        // arrange
        var referralId = Guid.Empty;

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityGroupAsync(referralId,
            EthnicityGroup.White.ToString("d")));

        // assert
        ex.Should().BeOfType<ArgumentException>();
        ex.Message.Should().Be("id cannot be empty. (Parameter 'id')");
      }

      [Theory]
      [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
      public async Task NoEthnicity_ArgumentException(string ethnicityGroup)
      {
        // arrange
        var referralId = Guid.NewGuid();

        // act & assert
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityGroupAsync(referralId, ethnicityGroup));

        // assert
        ex.Should().BeOfType<ArgumentException>();
        ex.Message.Should().Be("ethnicityGroup cannot be null or white " +
          "space. (Parameter 'ethnicityGroup')");
      }

      [Fact]
      public async Task ReferralNotFound_ReferralNotFoundException()
      {
        // arrange
        var id = Guid.NewGuid();
        var ethnicity = EthnicityGroup.White.ToString("d");

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityGroupAsync(id, ethnicity));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(_referralNotFoundExceptionMessage
          .Replace("{referralId}", id.ToString()));
      }

      [Fact]
      public async Task ReferralInactive_ReferralNotFoundException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: false);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityGroupAsync(referral.Id,
            referral.ServiceUserEthnicityGroup));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(_referralNotFoundExceptionMessage
          .Replace("{referralId}", referral.Id.ToString()));
      }

      [Fact]
      public async Task ReferralProviderIdIsNotNull_ReferralProviderSelectedException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          providerId: Guid.NewGuid());
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityGroupAsync(referral.Id,
            referral.ServiceUserEthnicityGroup));

        // assert
        ex.Should().BeOfType<ReferralProviderSelectedException>();
        ex.Message.Should().Be(_referralProviderSelectedException
          .Replace("{referralId}", referral.Id.ToString())
          .Replace("{providerId}", referral.ProviderId.ToString()));
      }

      [Fact]
      public async Task EthnicityNotFound_EthnicityNotFoundException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          serviceUserEthnicityGroup: "Unknown Ethnicity");
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .UpdateServiceUserEthnicityGroupAsync(referral.Id,
            referral.ServiceUserEthnicityGroup));

        // assert
        ex.Should().BeOfType<EthnicityNotFoundException>();
        ex.Message.Should().Be(_ethnicityNotFoundException
          .Replace("{ethnicityGroup}", referral.ServiceUserEthnicityGroup));
      }
    }
  }
}
