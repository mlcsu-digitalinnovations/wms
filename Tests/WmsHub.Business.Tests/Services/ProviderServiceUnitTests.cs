using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Extensions;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  [Collection("Service collection")]
  public class ProviderServiceUnitTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly ProviderService _providerService;

    public ProviderServiceUnitTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _context = new DatabaseContext(_serviceFixture.Options);

      _providerService = new ProviderService(
        _context,
        _serviceFixture.Mapper,
        TestConfiguration.CreateProviderOptions())
      {
        User = GetClaimsPrincipal()
      };

    }

    public class GetServiceUsersTests : ProviderServiceUnitTests
    {
      public GetServiceUsersTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
      {
        // arrange
        _context.Referrals.RemoveRange(_context.Referrals);
      }

      [Fact]
      public async Task GpReferralProviderAwaitingStart()
      {
        // arrange
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

        // act
        IEnumerable<ServiceUser> result =
          await _providerService.GetServiceUsers();

        //assert
        result.Count().Should().Be(1);
        StandardPropertyEquivalentAsserts(result.Single(), referral);
      }

      [Fact]
      public async Task SelfReferralProviderAwaitingStart()
      {
        // arrange
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

        // act
        IEnumerable<ServiceUser> result =
          await _providerService.GetServiceUsers();

        //assert
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
      }
    }


    public class ProviderSubmissionsAsyncTests : ProviderServiceUnitTests
    {

      public ProviderSubmissionsAsyncTests(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
      }

      // THIS NEEDS TO BE REFACTORED
      //[Fact]
      //public async Task ReferralIdNotFoundUsingUbrn_InvalidStatus()
      //{
      //  //arrange
      //  string ubrn = "877666555444";
      //  string error = $"Unable to find a referral with a UBRN of {ubrn} " +
      //   $"for provider {_sid}" ;
      //  IEnumerable<ServiceUserSubmissionRequest> requests = 
      //    new List<ServiceUserSubmissionRequest>
      //  {
      //    new ServiceUserSubmissionRequest
      //    {
      //      Date = DateTimeOffset.Now,
      //      Type = "Started",
      //      Ubrn = ubrn
      //    }
      //  };
      //  try
      //  {

      //    //act
      //    List<ServiceUserSubmissionResponse> response = 
      //      await _classToTest.Object
      //      .ProviderSubmissionsAsync(requests) as 
      //      List<ServiceUserSubmissionResponse>;

      //    //Assert
      //  }
      //  catch(Exception)
      //  {

      //  }
      //}
    }
  }

}
