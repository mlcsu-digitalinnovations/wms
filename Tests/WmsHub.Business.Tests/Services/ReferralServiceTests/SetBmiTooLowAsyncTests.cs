using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Services;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {
    public class SetBmiTooLowAsyncTests : ReferralServiceTests, IDisposable
    {
      new readonly ReferralService _service;
      const string REFERRALNOTFOUNDEXCEPTION =
        "Unable to find a referral with an id of {0}.";
      const string PROVIDERSELECTEDEXCEPTION =
        "The referral {0} has previously had its provider selected {1}.";
      const string ETHNICITYNOTFOUNDEXCEPTION =
        "Unable to find an ethnicity with a Display Name of {0}.";
      const string STATUSREASON =
        "BMI of {0} is below the minimum of {1} for the selected " +
        "ethnicity {2}, {3}.";

      public SetBmiTooLowAsyncTests(ServiceFixture serviceFixture) :
        base(serviceFixture)
      {
        _service = new ReferralService(
          _context,
          _serviceFixture.Mapper,
          null, // provider service
          _mockDeprivationService.Object,
          _mockPostcodeService.Object,
          _mockPatientTriageService.Object,
          _mockOdsOrganisationService.Object)
        {
          User = GetClaimsPrincipal()
        };

        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      public void Dispose()
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();
      }

      [Fact]
      public async Task ReferralDoesNotExist_ReferralNotFoundException()
      {
        // arrange
        var id = Guid.NewGuid();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .SetBmiTooLowAsync(id));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(string.Format(REFERRALNOTFOUNDEXCEPTION, id));
      }

      [Fact]
      public async Task ReferralIsInactive_ReferralNotFoundException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          isActive: false);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .SetBmiTooLowAsync(referral.Id));

        // assert
        ex.Should().BeOfType<ReferralNotFoundException>();
        ex.Message.Should().Be(
          string.Format(REFERRALNOTFOUNDEXCEPTION, referral.Id));
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
          .SetBmiTooLowAsync(referral.Id));

        // assert
        ex.Should().BeOfType<ReferralProviderSelectedException>();
        ex.Message.Should().Be(string.Format(
          PROVIDERSELECTEDEXCEPTION, 
          referral.Id, 
          referral.ProviderId.ToString()));
      }

      [Fact]
      public async Task EthnicityNotFound_EthnicityNotFoundException()
      {
        // arrange
        var referral = RandomEntityCreator.CreateRandomReferral(
          serviceUserEthnicity: "Unknown Ethnicity");
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .SetBmiTooLowAsync(referral.Id));

        // assert
        ex.Should().BeOfType<EthnicityNotFoundException>();
        ex.Message.Should().Be(string.Format(
          ETHNICITYNOTFOUNDEXCEPTION,
          referral.ServiceUserEthnicity));
      }

      [Fact]
      public async Task EthnicityIsInactive_EthnicityNotFoundException()
      {
        // arrange
        var ethnicity = RandomEntityCreator.CreateRandomEthnicty(
          isActive: false,
          displayName: "Inactive Ethnicity");
        var referral = RandomEntityCreator.CreateRandomReferral(
          serviceUserEthnicity: ethnicity.DisplayName);
        _context.Ethnicities.Add(ethnicity);
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        // act
        var ex = await Record.ExceptionAsync(() => _service
          .SetBmiTooLowAsync(referral.Id));

        // assert
        ex.Should().BeOfType<EthnicityNotFoundException>();
        ex.Message.Should().Be(string.Format(
          ETHNICITYNOTFOUNDEXCEPTION,
          referral.ServiceUserEthnicity));

        _context.Ethnicities.Remove(ethnicity);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Valid()
      {
        // arrange
        var ethnicity = _context.Ethnicities.First();
        var createdReferral = RandomEntityCreator.CreateRandomReferral(
          calculatedBmiAtRegistration: ethnicity.MinimumBmi - 1,
          serviceUserEthnicity: ethnicity.DisplayName,
          serviceUserEthnicityGroup: ethnicity.GroupName);
        _context.Referrals.Add(createdReferral);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // act
        var result = await _service
          .SetBmiTooLowAsync(createdReferral.Id);
        var updatedReferral = _context.Referrals
          .Single(r => r.Id == createdReferral.Id);

        // assert
        updatedReferral.Should()
          .BeEquivalentTo(createdReferral, options => options
            .Excluding(r => r.Audits)
            .Excluding(r => r.ModifiedAt)
            .Excluding(r => r.ModifiedByUserId)
            .Excluding(r => r.Status)
            .Excluding(r => r.StatusReason));
        updatedReferral.Audits.Count.Should().Be(1);
        updatedReferral.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.Status.Should()
          .Be(ReferralStatus.RejectedToEreferrals.ToString());
        updatedReferral.StatusReason.Should().Be(string.Format(
          STATUSREASON,
          createdReferral.CalculatedBmiAtRegistration,
          ethnicity.MinimumBmi,
          ethnicity.GroupName,
          ethnicity.DisplayName));

      }
    }
  }
}
