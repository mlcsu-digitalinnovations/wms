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
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public partial class ReferralServiceTests : ServiceTestsBase
  {
    public class CreateSelfReferralTests : ReferralServiceTests, IDisposable
    {
      Mock<IProviderService> _mockProviderService = new();
      ReferralService _serviceMockProviderService;

      public CreateSelfReferralTests(ServiceFixture serviceFixture)
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
      public async Task ParamNull_ArgumentNullException()
      {
        // arrange
        SelfReferralCreate model = null;

        // act
        var ex = await Record.ExceptionAsync(
          () => _service.CreateSelfReferral(model));

        // assert
        ex.Should().BeOfType<ArgumentNullException>();
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
          .CreateSelfReferral(_validSelfReferralCreate);

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

      [Fact]
      public async Task ReferralContactEmailException_NotRealEmail()
      {
        // arrange
        DateTimeOffset methodCallTime = DateTimeOffset.Now;
        SelfReferralCreate model = _validSelfReferralCreate;
        model.Email = "Incorrect_EmailAddress";

        //string expectedErrorMessage = 
        //  $"Unable to create referral: The Email is not a valid email.," +
        //  $"The Email is not a valid NHS email.";

        var ex = await Assert.ThrowsAsync<SelfReferralValidationException>(
          async () => await _service.CreateSelfReferral(model));

        // TODO - Improve test
      }

      [Fact]
      public async Task ReferralContactEmailException_DomainIncorrect()
      {
        // arrange
        DateTimeOffset methodCallTime = DateTimeOffset.Now;
        SelfReferralCreate model = _validSelfReferralCreate;
        model.Email = "Incorrect@Email.com";

        //string expectedErrorMessage = 
        //  $"Unable to create referral: The Email is not a valid NHS email.";

        var ex = await Assert.ThrowsAsync<SelfReferralValidationException>(
          async () => await _service.CreateSelfReferral(model));

        // TODO - Improve test
      }

      [Fact]
      public async Task ReferralContactEmailException_NoStaffRole()
      {
        // arrange
        DateTimeOffset methodCallTime = DateTimeOffset.Now;
        SelfReferralCreate model = _validSelfReferralCreate;
        model.StaffRole = "Unfound StaffRole";

        RemoveReferralBeforeTest(model.Email);
        var ex = await Assert.ThrowsAsync<SelfReferralValidationException>(
          async () => await _service.CreateSelfReferral(model));

        // TODO - Improve test
      }

      [Fact]
      public async Task CreateSelfReferral_Valid()
      {
        // arrange
        DateTimeOffset methodCallTime = DateTimeOffset.Now;

        var staffRole = RandomEntityCreator.CreateRandomStaffRole();
        _context.StaffRoles.Add(staffRole);

        var provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        SelfReferralCreate model = _validSelfReferralCreate;
        model.StaffRole = staffRole.DisplayName;
        
        try
        {
          // act
          IReferral result = await _service.CreateSelfReferral(model);

          // assert
          result.Should().BeOfType<Referral>();
          result.IsActive.Should().BeTrue();
          result.Status.Should().Be(ReferralStatus.New.ToString());
          result.StatusReason.Should().BeNull();
          result.ModifiedAt.Should().BeAfter(methodCallTime);
          result.ModifiedByUserId.Should().Be(_service.User.GetUserId());
          result.DateCompletedProgramme.Should().BeNull();
          result.DateOfProviderSelection.Should().BeNull();
          result.DateStartedProgramme.Should().BeNull();
          result.ProgrammeOutcome.Should().BeNull();
          result.TriagedCompletionLevel.Should().Be("3");
          result.TriagedWeightedLevel.Should().Be("2");
          result.ReferringGpPracticeNumber.Should()
            .Be(Constants.UNKNOWN_GP_PRACTICE_NUMBER);
          result.ConsentForFutureContactForEvaluation.Should().BeTrue();
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
        finally
        {
          //clean up
          _context.Providers.Remove(provider);
          _context.StaffRoles.Remove(staffRole);
          await _context.SaveChangesAsync();
        }
      }

      [Fact]
      public async Task CreateSelfReferral_Invalid_NoConsent()
      {
        // arrange
        string expected =
          "The ConsentForFutureContactForEvaluation field is required.";
        Random rnd = new Random();
        string staffRole = "Ambulance Worker";
        DateTimeOffset methodCallTime = DateTimeOffset.Now;
        SelfReferralCreate model = UniqueValidReferralCreate();
        model.ConsentForFutureContactForEvaluation = null;
        model.StaffRole = staffRole;

        Entities.StaffRole staffAmbulanceWorker = new Entities.StaffRole()
        {
          DisplayName = staffRole,
          IsActive = true,
          DisplayOrder = 1
        };

        _context.StaffRoles.Add(staffAmbulanceWorker);
        _context.SaveChanges();

        // act
        try
        {
          IReferral result = await _service.CreateSelfReferral(model);

          // assert
          Assert.True(false, "Expected SelfReferralValidationException");

        }
        catch (SelfReferralValidationException ex)
        {
          Assert.True(true, ex.Message);
          ex.ValidationResults.FirstOrDefault().Value[0].Should()
            .Be(expected);
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
        finally
        {

          //clean up
          _context.StaffRoles.Remove(staffAmbulanceWorker);
          _context.SaveChanges();
        }
      }

      public class CheckNewSelfReferralIsUniqueAsyncTests
        : CreateSelfReferralTests
      {
        public CheckNewSelfReferralIsUniqueAsyncTests(
          ServiceFixture serviceFixture) : base(serviceFixture)
        { }

        [Fact]
        public async Task Invalid_ReferralExists()
        {
          //arrange
          Random rnd = new Random();
          Entities.Referral referral = CreateUniqueReferral();
          referral.ReferralSource = ReferralSource.SelfReferral.ToString();
          referral.Ubrn = Generators.GenerateSelfUbrn(rnd);
          referral.StaffRole = "Ambulance Worker";
          referral.Email = Generators.GenerateNhsEmail(rnd);
          referral.Status = ReferralStatus.New.ToString();
          _context.Referrals.Add(referral);
          _context.SaveChanges();

          DateTimeOffset methodCallTime = DateTimeOffset.Now;
          SelfReferralCreate model = _validSelfReferralCreate;
          model.Email = referral.Email;
          model.StaffRole = referral.StaffRole;

          Entities.StaffRole staffAmbulanceWorker = new Entities.StaffRole()
          {
            DisplayName = referral.StaffRole,
            IsActive = true,
            DisplayOrder = 1
          };

          _context.StaffRoles.Add(staffAmbulanceWorker);
          await _context.SaveChangesAsync();

          try
          {
            IReferral result = await _service.CreateSelfReferral(model);
            Assert.True(false, "ReferralNotUniqueException expected");
          }
          catch (ReferralNotUniqueException ex)
          {
            Assert.True(true, ex.Message);
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected ReferralNotUniqueException but got {ex.Message}");
          }
          finally
          {
            _context.Referrals.Remove(referral);
            _context.StaffRoles.Remove(staffAmbulanceWorker);
            _context.SaveChanges();
          }

        }
      }

      public class ValidateReferralTests : CreateSelfReferralTests
      {
        public ValidateReferralTests(ServiceFixture serviceFixture)
          : base(serviceFixture)
        { }

        [Fact]
        public async Task SelfReferralValidationException_NotInStaffRoll()
        {
          //arrange
          var expected = "The StaffRole field is invalid.";
          Random rnd = new Random();
          string staffRole = "Ambulance Worker";
          DateTimeOffset methodCallTime = DateTimeOffset.Now;
          SelfReferralCreate model = _validSelfReferralCreate;
          model.Email = Generators.GenerateNhsEmail(rnd);
          model.StaffRole = "outside contractor";

          Entities.StaffRole staffAmbulanceWorker = new Entities.StaffRole()
          {
            DisplayName = staffRole,
            IsActive = true,
            DisplayOrder = 1
          };

          _context.StaffRoles.Add(staffAmbulanceWorker);
          await _context.SaveChangesAsync();
          RemoveReferralBeforeTest(model.Email);
          try
          {
            IReferral result = await _service.CreateSelfReferral(model);
            Assert.True(false, "SelfReferralValidationException expected");
          }
          catch (SelfReferralValidationException ex)
          {
            Assert.True(true, ex.Message);
            ex.ValidationResults.Count.Should().Be(1);
            foreach (KeyValuePair<string, string[]> validationResult
              in ex.ValidationResults)
            {
              validationResult.Value.Length.Should().Be(1);
              validationResult.Value[0].Should().Be(expected);
            }
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected SelfReferralValidationException " +
              $"but got {ex.Message}");
          }
          finally
          {
            _context.StaffRoles.Remove(staffAmbulanceWorker);
            _context.SaveChanges();
          }
        }

        [Fact]
        public async Task SelfReferralValidationException_EthnicityGroup()
        {
          //arrange
          SelfReferralCreate model = _validSelfReferralCreate;
          model.ServiceUserEthnicityGroup = "Klingon";
          var expected = new string[] {
            $"The {nameof(model.ServiceUserEthnicityGroup)} field is invalid."};

          RemoveReferralBeforeTest(model.Email);
          try
          {
            IReferral result = await _service.CreateSelfReferral(model);
            Assert.True(false, "SelfReferralValidationException expected");
          }
          catch (SelfReferralValidationException ex)
          {
            Assert.True(true, ex.Message);
            ex.ValidationResults.Count.Should().BeGreaterThan(0);
            var count = 0;
            foreach (KeyValuePair<string, string[]> validationResult
              in ex.ValidationResults)
            {
              validationResult.Value.Length.Should().Be(1);
              validationResult.Value[0].Should().Be(expected[count]);
              count++;
            }
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected SelfReferralValidationException " +
              $"but got {ex.Message}");
          }
        }

        [Fact]
        public async Task SelfReferralValidationException_Ethnicity()
        {
          //arrange          
          SelfReferralCreate model = _validSelfReferralCreate;
          model.ServiceUserEthnicity = "Klingon";
          var expected = new string[] {
            $"The {nameof(model.ServiceUserEthnicity)} field is invalid."};

          RemoveReferralBeforeTest(model.Email);

          try
          {
            IReferral result = await _service.CreateSelfReferral(model);
            Assert.True(false, "SelfReferralValidationException expected");
          }
          catch (SelfReferralValidationException ex)
          {
            Assert.True(true, ex.Message);
            ex.ValidationResults.Count.Should().BeGreaterThan(0);
            var count = 0;
            foreach (KeyValuePair<string, string[]> validationResult
              in ex.ValidationResults)
            {
              validationResult.Value.Length.Should().Be(1);
              validationResult.Value[0].Should().Be(expected[count]);
              count++;
            }
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected SelfReferralValidationException " +
              $"but got {ex.Message}");
          }
        }

        [Fact]
        public async Task SelfReferralValidationException_BMI()
        {
          //arrange
          Random rnd = new Random();
          string staffRole = "Ambulance Worker";
          DateTimeOffset methodCallTime = DateTimeOffset.Now;
          SelfReferralCreate model = _validSelfReferralCreate;
          model.Email = Generators.GenerateNhsEmail(rnd);
          model.StaffRole = staffRole;
          model.HeightCm = 181;
          model.WeightKg = 78;
          var expected = "The calculated BMI of 23.8 is too low, the " +
            $"minimum for an ethnicity of {model.Ethnicity} is 30.00";

          Entities.StaffRole staffAmbulanceWorker = new Entities.StaffRole()
          {
            DisplayName = staffRole,
            IsActive = true,
            DisplayOrder = 1
          };

          _context.StaffRoles.Add(staffAmbulanceWorker);
          await _context.SaveChangesAsync();
          RemoveReferralBeforeTest(model.Email);
          try
          {
            IReferral result = await _service.CreateSelfReferral(model);
            Assert.True(false, "SelfReferralValidationException expected");
          }
          catch (SelfReferralValidationException ex)
          {
            Assert.True(true, ex.Message);
            ex.ValidationResults.Count.Should().Be(1);
            foreach (KeyValuePair<string, string[]> validationResult
              in ex.ValidationResults)
            {
              validationResult.Value.Length.Should().Be(1);
              validationResult.Value[0].Should().Be(expected);
            }
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected SelfReferralValidationException " +
              $"but got {ex.Message}");
          }
          finally
          {
            _context.StaffRoles.Remove(staffAmbulanceWorker);
            _context.SaveChanges();
          }
        }
      }

      public class TraiageReferralUpdateAsyncTests : CreateSelfReferralTests
      {
        public TraiageReferralUpdateAsyncTests(ServiceFixture serviceFixture)
          : base(serviceFixture)
        {
        }

        [Fact]

        public async Task Failed_Id_Empty()
        {
          //arrange
          Guid id = Guid.Empty;
          //act
          try
          {
            await _service.TriageReferralUpdateAsync(id);
            Assert.True(false, "Expected ArgumentException");
          }
          catch (ArgumentException)
          {
            Assert.True(true);
          }
          catch (Exception ex)
          {
            Assert.True(false,
              $"Expected ArgumentException but got {ex.Message}");
          }
        }

        [Fact]
        public async Task Valid_Referral_Triage()
        {
          // arrange
          Random rnd = new Random();
          Entities.Referral referral = CreateUniqueReferral(
            ubrn: Generators.GenerateSelfUbrn(rnd),
            heightCm: 181,
            weightKg: 108,
            calculatedBmiAtRegistration: 31);

          _context.Referrals.Add(referral);
          await _context.SaveChangesAsync();

          //act
          try
          {
            referral.TriagedCompletionLevel.Should().BeNull();
            referral.TriagedWeightedLevel.Should().BeNull();
            await _service.TriageReferralUpdateAsync(referral.Id);
            referral.TriagedCompletionLevel.Should().NotBeNullOrWhiteSpace();
            referral.TriagedWeightedLevel.Should().NotBeNullOrWhiteSpace();
          }
          catch (Exception ex)
          {
            Assert.True(false, ex.Message);
          }
          finally
          {
            _context.Referrals.Remove(referral);
            await _context.SaveChangesAsync();
          }
        }
      }
    }
  }
}
