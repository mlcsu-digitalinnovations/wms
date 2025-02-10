using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Helpers;
using WmsHub.Common.Models;
using WmsHub.Common.Validation;
using WmsHub.Referral.Api.Controllers;
using Xunit;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Referral.Api.Tests
{
  public class ReferralControllerTests : TestSetup
  {
    private ReferralController _classToTest;

    public ReferralControllerTests()
    {
      _mockProcessStatusService = new Mock<IProcessStatusService>();
      _mockProcessStatusService.Setup(s => s.StartedAsync()).Returns(Task.CompletedTask);
      _mockProcessStatusService.Setup(s => s.SuccessAsync()).Returns(Task.CompletedTask);
      _mockProcessStatusService.Setup(s => s.FailureAsync(It.IsAny<string>()))
        .Returns(Task.CompletedTask);
      _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object);
    }
    public class DeleteReferralTests : ReferralControllerTests
    {
      [Fact]
      public async Task UpdateReferralCancelledByEReferralAsync_Valid_200()
      {
        //Arrange
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralCancelledByEReferralAsync(ubrn))
         .Returns(Task.FromResult(1));
        //Act
        var response = await _classToTest.DeleteReferral(ubrn);
        //Assert
        OkResult outputResult = Assert.IsType<OkResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);
      }

      [Fact]
      public async Task ReferralNotFoundException_404()
      {
        //Arrange
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralCancelledByEReferralAsync(ubrn))
         .Throws(new ReferralNotFoundException("test"));
        //Act
        var response = await _classToTest.DeleteReferral(ubrn);
        //Assert
        ObjectResult outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
      }

      [Fact]
      public async Task ReferralInvalidStatusException_409()
      {
        //Arrange
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralCancelledByEReferralAsync(ubrn))
         .Throws(new ReferralInvalidStatusException("test"));
        //Act
        IActionResult response = await _classToTest.DeleteReferral(ubrn);
        //Assert
        var outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
      }
    }

    public class GetActiveUbrnsTests : ReferralControllerTests
    {
      private readonly Mock<ActiveReferralAndExceptionUbrn> _mockReferral =
        new Mock<ActiveReferralAndExceptionUbrn>();

      private readonly List<ActiveReferralAndExceptionUbrn> _ubrnList;

      public GetActiveUbrnsTests()
      {
        _ubrnList = new List<ActiveReferralAndExceptionUbrn> { _mockReferral.Object };
      }

      [Fact]
      public async Task Valid_Returns_MappedActiveReferralAndExceptionUbrn_200()
      {
        // Arrange.
        string actualServiceId = null;
        int expectedStatusCode = 200;
        string expectedServiceId = "123456";

        ActiveReferralAndExceptionUbrn expectedActiveReferralAndExceptionUbrn =
          RandomModelCreator.ActiveReferralAndExceptionUbrn(
            mostRecentAttachmentDate: DateTimeOffset.Now);

        List<ActiveReferralAndExceptionUbrn> activeReferralAndExceptionUbrns = new()
        {
          expectedActiveReferralAndExceptionUbrn
        };

        _mockReferralService
          .Setup(_ => _.GetOpenErsGpReferralsThatAreNotCancelledByEreferals(It.IsAny<string>()))
          .Callback(new InvocationAction(i => actualServiceId = (string)i.Arguments[0]))
          .Returns(Task.FromResult(activeReferralAndExceptionUbrns))
          .Verifiable(Times.Once);

        _classToTest = new(
          _mockReferralService.Object,
          Mapper,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new()
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.GetActiveUbrns(expectedServiceId);

        // Assert.
        actualServiceId.Should().Be(expectedServiceId,
          because: "it should be passed into GetOpenErsGpReferralsThatAreNotCancelledByEreferals");

        OkObjectResult result = response.Should().BeOfType<OkObjectResult>().Subject;
        result.StatusCode.Should().Be(expectedStatusCode);

        List<GetActiveUbrnResponse> getActiveUbrnResponses = result
          .Value.Should().BeOfType<List<GetActiveUbrnResponse>>().Subject;

        getActiveUbrnResponses.Should().HaveCount(1,
          because: "only 1 ActiveReferralAndExceptionUbrn is returned by the mock setup")
          .And.Subject.Single().Should().BeEquivalentTo(expectedActiveReferralAndExceptionUbrn,
            o => o.Excluding(x => x.ReferralStatus));

        _mockReferralService.VerifyAll();
      }

      [Fact]
      public async Task Invalid_Claim_Return_401()
      {
        //Arrange
        int expected = 401;
        string serviceId = "123456";

        _mockReferralService
          .Setup(t => t
            .GetOpenErsGpReferralsThatAreNotCancelledByEreferals(serviceId))
         .Returns(Task.FromResult(_ubrnList));
        _classToTest =
          new ReferralController(
            _mockReferralService.Object,
            _mockMapper.Object,
            _mockProcessStatusService.Object,
            _mockProcessStatusOptions.Object)
          {
            ControllerContext = new ControllerContext
            {
              HttpContext =
              new DefaultHttpContext
              {
                User = GetUnknownClaimsPrincipal("Unknown")
              }
            }
          };

        //Act
        var response = await _classToTest.GetActiveUbrns(serviceId);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<UnauthorizedObjectResult>();
        UnauthorizedObjectResult result =
          response as UnauthorizedObjectResult;
        result.StatusCode.Should().Be(expected);
      }


      /// <summary>
      /// All EFCore exceptions are wrapped up in DbUpdateException
      /// </summary>
      /// <returns></returns>
      [Fact]
      public async Task Invalid_Service_Throws_DbUpdateException_Returns_500()
      {
        string serviceId = "123456";
        //Arrange
        string expectedMessage = "Return InternalServerError";
        _mockReferralService
          .Setup(t => t
            .GetOpenErsGpReferralsThatAreNotCancelledByEreferals(serviceId))
         .Throws(new DbUpdateException(expectedMessage));
        _classToTest =
          new ReferralController(
            _mockReferralService.Object,
            _mockMapper.Object,
            _mockProcessStatusService.Object,
            _mockProcessStatusOptions.Object)
          {
            ControllerContext = new ControllerContext
            {
              HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
            }
          };

        try
        {
          //Act
          var response = await _classToTest.GetActiveUbrns(serviceId);
          //Assert
          Assert.Fail("Test to be written");
        }
        catch (DbUpdateException ex)
        {
          Assert.True(true, ex.Message);
          ex.Message.Should().Be(expectedMessage);
        }
        catch (Exception ex)
        {
          Assert.Fail(ex.Message);
        }
      }
    }

    public class PutTests : ReferralControllerTests
    {
      [Fact]
      public async Task Valid_UpdateGpReferralReceivesMappedReferralUpdate_Returns_200()
      {
        // Arrange.        
        int expectedStatusCode = 200;
        string expectedUbrn = Generators.GenerateUbrn();
        ReferralPut referralPut = RandomModelCreator.ReferralPut();
        ReferralUpdate referralUpdate = null;

        _mockReferralService
          .Setup(_ => _.UpdateGpReferral(It.IsAny<ReferralUpdate>()))
          .Callback(new InvocationAction(i => referralUpdate = (ReferralUpdate)i.Arguments[0]))
          .Returns(Task.FromResult(new Mock<IReferral>().Object))
          .Verifiable(Times.Once);

        _classToTest = new ReferralController(
          _mockReferralService.Object,
          Mapper,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.Put(referralPut, expectedUbrn);

        // Assert.
        response.Should().NotBeNull()
          .And.BeOfType<OkObjectResult>()
          .Which.StatusCode.Should().Be(expectedStatusCode);

        referralUpdate.Should().NotBeNull()
          .And.BeEquivalentTo(referralPut, o => o.Excluding(x => x.PdfParseLog));
        referralUpdate.Ubrn.Should().Be(expectedUbrn);

        _mockReferralService.VerifyAll();
      }

      [Fact]
      public async Task Invalid_Claim_Returns_401()
      {
        //Arrange
        Random random = new Random();
        int expected = 401;
        Mock<ReferralPut> request = new Mock<ReferralPut>();
        string ubrn = Generators.GenerateUbrn(random);
        _classToTest =
          new ReferralController(
            _mockReferralService.Object,
            _mockMapper.Object,
            _mockProcessStatusService.Object,
            _mockProcessStatusOptions.Object)
          {
            ControllerContext = new ControllerContext
            {
              HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Unknown")
            }
            }
          };

        //Act
        var response = await _classToTest.Put(request.Object, ubrn);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<UnauthorizedObjectResult>();
        UnauthorizedObjectResult result =
          response as UnauthorizedObjectResult;
        result.StatusCode.Should().Be(expected);
      }


      [Fact]
      public async Task Invalid_Update_ReferralNotFoundException_400()
      {
        //Arrange
        int expected = 400;
        Random random = new Random();
        Mock<ReferralPut> request = new Mock<ReferralPut>();
        string ubrn = Generators.GenerateUbrn(random);
        Mock<ReferralUpdate> referralUpdate = new Mock<ReferralUpdate>();
        _mockMapper.Setup(t => t.Map<ReferralUpdate>
          (It.IsAny<ReferralPut>())).Returns(referralUpdate.Object);

        Mock<IReferral> mockReferral = new Mock<IReferral>();
        _mockReferralService
          .Setup(t => t.UpdateGpReferral(It.IsAny<IReferralUpdate>()))
         .Throws(new ReferralNotFoundException("Test"));
        _classToTest =
          new ReferralController(
            _mockReferralService.Object,
            _mockMapper.Object,
            _mockProcessStatusService.Object,
            _mockProcessStatusOptions.Object)
          {
            ControllerContext = new ControllerContext
            {
              HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
            }
          };

        //Act
        var response = await _classToTest.Put(request.Object, ubrn);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task Invalid_Update_Exception_500()
      {
        //Arrange
        int expected = 500;
        Random random = new Random();
        Mock<ReferralPut> request = new Mock<ReferralPut>();
        string ubrn = Generators.GenerateUbrn(random);
        Mock<ReferralUpdate> referralUpdate = new Mock<ReferralUpdate>();
        _mockMapper.Setup(t => t.Map<ReferralUpdate>
          (It.IsAny<ReferralPut>())).Returns(referralUpdate.Object);

        Mock<IReferral> mockReferral = new Mock<IReferral>();
        _mockReferralService
          .Setup(t => t.UpdateGpReferral(It.IsAny<IReferralUpdate>()))
         .Throws(new DbUpdateException("Test"));
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        //Act
        var response = await _classToTest.Put(request.Object, ubrn);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expected);
      }

    }

    public class PostTests : ReferralControllerTests
    {
      [Fact]
      public async Task Valid_CreateReferralReceivesMappedReferralCreate_Returns_200()
      {
        // Arrange.
        ReferralCreate referralCreate = null;
        int expectedStatusCode = 200;
        ReferralPost referralPost = RandomModelCreator.ReferralPost();

        _mockReferralService
          .Setup(t => t.CreateReferral(It.IsAny<ReferralCreate>()))
          .Callback(new InvocationAction(i => referralCreate = (ReferralCreate)i.Arguments[0]))
          .Returns(Task.FromResult(new Mock<IReferral>().Object))
          .Verifiable(Times.Once);

        _classToTest = new ReferralController(
          _mockReferralService.Object,
          Mapper,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.Post(referralPost);

        // Assert.
        response.Should().NotBeNull()
          .And.BeOfType<OkObjectResult>()
          .Which.StatusCode.Should().Be(expectedStatusCode);

        referralCreate.Should().NotBeNull()
          .And.BeEquivalentTo(referralPost, options => options
            .Excluding(x => x.NumberOfMissingEntries)
            .Excluding(x => x.PdfParseLog));

        _mockReferralService.VerifyAll();
      }

      [Theory]
      [InlineData("")]
      [InlineData(null)]
      [InlineData("test.com")]
      [InlineData("test@test@.com")]
      //[InlineData("none@yahoo.co.uk")]
      //[InlineData("aaa@gmail.co.uk")]
      //[InlineData("bbb@yahoo.co.uk")]
      //[InlineData("ccc@yahoo.co.uk")]
      //[InlineData("n0n3@yahoo.co.uk")]
      //[InlineData("qwerty@yahoo.co.uk")]
      //[InlineData("nothing@yahoo.co.uk")]
      //[InlineData("self@gmail.com")]
      [InlineData("None")]
      public void Valid_Convert_NullEmail(string email)
      {
        //Arrange
        var random = new Random();
        var referralPost = new ReferralPost
        {
          Ubrn = Generators.GenerateUbrn(random),
          Email = email,
          Address1 = Generators.GenerateAddress1(random),
          Address2 = Generators.GenerateIpAddress(random),
          DateOfBirth = DateTimeOffset.Now.AddYears(-random.Next(18, 100)),
          DateOfReferral = DateTimeOffset.Now,
          DateOfBmiAtRegistration = DateTimeOffset.Now.AddDays(-1),
          Ethnicity = Generators.GenerateEthnicity(random),
          FamilyName = Generators.GenerateName(random, 10),
          GivenName = Generators.GenerateName(random, 10),
          HeightCm = 181,
          WeightKg = 118,
          NhsNumber = Generators.GenerateNhsNumber(random),
          Postcode = Generators.GeneratePostcode(random),
          Mobile = Generators.GenerateMobile(random),
          Telephone = Generators.GenerateTelephone(random),
          Sex = Generators.GenerateSex(random),
          ReferringGpPracticeName = Generators.GenerateName(random, 20),
          ReferringGpPracticeNumber = Generators
            .GenerateGpPracticeNumber(random),
          ReferralAttachmentId = "0",
          HasRegisteredSeriousMentalIllness = false,
          HasALearningDisability = false,
          HasAPhysicalDisability = false,
          HasDiabetesType1 = false,
          HasDiabetesType2 = false,
          HasHypertension = false,
          IsVulnerable = false
        };

        //Act
        IMapper mapper = new MapperConfiguration(cfg => cfg
          .AddMaps(new[] { "WmsHub.Referral.Api" })).CreateMapper();

        IReferralCreate referralCreate = mapper
          .Map<ReferralCreate>(referralPost);

        //Assert
        referralCreate.Email.Should().BeNullOrWhiteSpace();
      }

      [Theory]
      [InlineData("paul.potts-smith@nhs.net(Unverified)")]
      [InlineData("paul.potts-smith@nhs.net (Verified)")]
      [InlineData("paul.potts-smith@nhs.net verified")]
      [InlineData("paul.potts-smith@nhs.net - Unverified")]
      [InlineData("paul.potts-smith@nhs.net(valid")]
      [InlineData("paul.potts-smith@nhs.net(Invalid)")]
      [InlineData("paul.potts-smith@nhs.net - Invalid")]
      [InlineData("paul.potts-smith@nhs.net(Home)")]
      [InlineData("paul.potts-smith@nhs.net(Work)")]
      [InlineData("paul.potts-smith@nhs.net - Home")]
      [InlineData("paul.potts-smith@nhs.net - Work")]

      public void Valid_Convert_CleanedEmail(string email)
      {
        //Arrange
        string expectedEmail = "paul.potts-smith@nhs.net";

        var random = new Random();
        var referralPost = new ReferralPost
        {
          Ubrn = Generators.GenerateUbrn(random),
          Email = email,
          Address1 = Generators.GenerateAddress1(random),
          Address2 = Generators.GenerateIpAddress(random),
          DateOfBirth = DateTimeOffset.Now.AddYears(-random.Next(18, 100)),
          DateOfReferral = DateTimeOffset.Now,
          DateOfBmiAtRegistration = DateTimeOffset.Now.AddDays(-1),
          Ethnicity = Generators.GenerateEthnicity(random),
          FamilyName = Generators.GenerateName(random, 10),
          GivenName = Generators.GenerateName(random, 10),
          HeightCm = 181,
          WeightKg = 118,
          NhsNumber = Generators.GenerateNhsNumber(random),
          Postcode = Generators.GeneratePostcode(random),
          Mobile = Generators.GenerateMobile(random),
          Telephone = Generators.GenerateTelephone(random),
          Sex = Generators.GenerateSex(random),
          ReferringGpPracticeName = Generators.GenerateName(random, 20),
          ReferringGpPracticeNumber = Generators
            .GenerateGpPracticeNumber(random),
          ReferralAttachmentId = "0",
          HasRegisteredSeriousMentalIllness = false,
          HasALearningDisability = false,
          HasAPhysicalDisability = false,
          HasDiabetesType1 = false,
          HasDiabetesType2 = false,
          HasHypertension = false,
          IsVulnerable = false
        };

        //Act
        IMapper mapper = new MapperConfiguration(cfg => cfg
          .AddMaps(new[] { "WmsHub.Referral.Api" })).CreateMapper();
        IReferralCreate referralCreate = mapper
          .Map<ReferralCreate>(referralPost);

        //Assert
        referralCreate.Email.Should().Be(expectedEmail);
      }

      [Theory]
      [InlineData("paulpotts - smith@nhs.net - Work")]
      [InlineData("paulpotts- smith@nhs.net - Work")]
      [InlineData("paul potts-smith@nhs.net")]
      [InlineData("paulpotts-smith@nhs.net sometext")]
      [InlineData("paulpotts-smith@nhs.net some text")]
      public void Valid_Convert_CleanedEmailWithSpaces(string email)
      {
        //Arrange
        string expectedEmail = "paulpotts-smith@nhs.net";

        var random = new Random();
        var referralPost = new ReferralPost
        {
          Ubrn = Generators.GenerateUbrn(random),
          Email = email,
          Address1 = Generators.GenerateAddress1(random),
          Address2 = Generators.GenerateIpAddress(random),
          DateOfBirth = DateTimeOffset.Now.AddYears(-random.Next(18, 100)),
          DateOfReferral = DateTimeOffset.Now,
          DateOfBmiAtRegistration = DateTimeOffset.Now.AddDays(-1),
          Ethnicity = Generators.GenerateEthnicity(random),
          FamilyName = Generators.GenerateName(random, 10),
          GivenName = Generators.GenerateName(random, 10),
          HeightCm = 181,
          WeightKg = 118,
          NhsNumber = Generators.GenerateNhsNumber(random),
          Postcode = Generators.GeneratePostcode(random),
          Mobile = Generators.GenerateMobile(random),
          Telephone = Generators.GenerateTelephone(random),
          Sex = Generators.GenerateSex(random),
          ReferringGpPracticeName = Generators.GenerateName(random, 20),
          ReferringGpPracticeNumber = Generators
            .GenerateGpPracticeNumber(random),
          ReferralAttachmentId = "0",
          HasRegisteredSeriousMentalIllness = false,
          HasALearningDisability = false,
          HasAPhysicalDisability = false,
          HasDiabetesType1 = false,
          HasDiabetesType2 = false,
          HasHypertension = false,
          IsVulnerable = false
        };

        //Act
        IMapper mapper = new MapperConfiguration(cfg => cfg
          .AddMaps(new[] { "WmsHub.Referral.Api" })).CreateMapper();
        IReferralCreate referralCreate = mapper
          .Map<ReferralCreate>(referralPost);

        //Assert
        referralCreate.Email.Should().Be(expectedEmail);
      }

      [Fact]
      public async Task Invalid_Claim_Returns_401()
      {
        //Arrange
        int expected = 401;
        Mock<ReferralPost> request = new Mock<ReferralPost>();
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Unknown")
            }
          }
        };

        //Act
        var response = await _classToTest.Post(request.Object);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<UnauthorizedObjectResult>();
        UnauthorizedObjectResult result =
          response as UnauthorizedObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      /// <summary>
      /// Also covers same test for PostTests
      /// </summary>
      public class MapUpdateTests : PutTests
      {
        private readonly ReferralCreate _modelToValidate;

        public MapUpdateTests()
        {
          Random random = new Random();
          _modelToValidate = RandomModelCreator.CreateRandomReferralCreate(
            address1: "Address1",
            address2: "Address2",
            address3: "Address3",
            calculatedBmiAtRegistration: 30m,
            dateOfBirth: DateTimeOffset.Now.AddYears(-40),
            dateOfBmiAtRegistration: DateTimeOffset.Now.AddMonths(-6),
            dateOfReferral: DateTimeOffset.Now,
            email: "beaulah.casper37@ethereal.email",
            ethnicity: Business.Enums.Ethnicity.White.ToString(),
            familyName: "FamilyName",
            givenName: "GivenName",
            hasALearningDisability: false,
            hasAPhysicalDisability: false,
            hasDiabetesType1: true,
            hasDiabetesType2: false,
            hasHypertension: true,
            hasRegisteredSeriousMentalIllness: false,
            heightCm: 150m,
            isVulnerable: false,
            mobile: "+447886123456",
            nhsNumber: null,
            postcode: "TF1 4NF",
            referralAttachmentId: "123456",
            referringGpPracticeName: "Marden Medical Practice",
            referringGpPracticeNumber: "M82047",
            sex: "Male",
            telephone: "+441743123456",
            ubrn: null,
            vulnerableDescription: "Not Vulnerable",
            weightKg: 120m
            );
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_NhsNumber()
        {
          //Arrange
          string expected = "The NhsNumber field is required.";
          _modelToValidate.NhsNumber = string.Empty;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData(null, null)]
        public void Invalid_Map_ReferralCreate_Missing_Tel_And_Mobile(
          string mobile, string telephone)
        {
          //Arrange
          string expected = $"One of the fields: " +
            $"{nameof(_modelToValidate.Telephone)} or " +
            $"{nameof(_modelToValidate.Mobile)} is required.";

          _modelToValidate.Telephone = telephone;
          _modelToValidate.Mobile = mobile;

          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_DateOfReferral()
        {
          //Arrange
          string expected = "The DateOfReferral field is required.";
          _modelToValidate.DateOfReferral = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_ReferralPracticeNumber()
        {
          //Arrange
          _modelToValidate.ReferringGpPracticeNumber = string.Empty;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeTrue();
          _modelToValidate.ReferringGpPracticeNumber.Should().Be(
            Constants.UNKNOWN_GP_PRACTICE_NUMBER,
            because: "a blank value is converted to V81999");
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_Ubrn()
        {
          //Arrange
          string expected = "The Ubrn field is required.";
          _modelToValidate.Ubrn = string.Empty;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_FamilyName()
        {
          //Arrange
          string expected = "The FamilyName field is required.";
          _modelToValidate.FamilyName = string.Empty;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_GivenName()
        {
          //Arrange
          string expected = "The GivenName field is required.";
          _modelToValidate.GivenName = string.Empty;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Valid_Map_ReferralCreate_Missing_Address1(
          string address1)
        {
          //Arrange
          _modelToValidate.Address1 = address1;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_Postcode()
        {
          //Arrange
          string expected = "The Postcode field is not a valid postcode.";
          _modelToValidate.Postcode = string.Empty;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_DOB()
        {
          //Arrange
          string expected = "The DateOfBirth field is required.";
          _modelToValidate.DateOfBirth = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_Gender()
        {
          //Arrange
          string expected = "The Sex field is required.";
          _modelToValidate.Sex = string.Empty;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Valid_Map_ReferralCreate_Missing_IsVulnerable()
        {
          //Arrange
          _modelToValidate.IsVulnerable = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Valid_Map_ReferralCreate_Missing_PhysicalDisability()
        {
          //Arrange
          _modelToValidate.HasAPhysicalDisability = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Valid_Map_ReferralCreate_Missing_LearningDisability()
        {
          //Arrange
          _modelToValidate.HasALearningDisability = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Valid_Map_ReferralCreate_Missing_MentalIllness()
        {
          //Arrange
          _modelToValidate.HasRegisteredSeriousMentalIllness = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_Hypertension()
        {
          //Arrange
          string expected = "The HasHypertension field is required.";
          _modelToValidate.HasHypertension = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_Type1Diabetes()
        {
          //Arrange
          string expected = "The HasDiabetesType1 field is required.";
          _modelToValidate.HasDiabetesType1 = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_Type2Diabetes()
        {
          //Arrange
          string expected = "The HasDiabetesType2 field is required.";
          _modelToValidate.HasDiabetesType2 = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_Bmi()
        {
          //Arrange
          string expected =
            "The DateOfBmiAtRegistration field is required.";
          _modelToValidate.DateOfBmiAtRegistration = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_GpPracticeName()
        {
          //Arrange
          string expected = "The ReferringGpPracticeName field is required.";
          _modelToValidate.ReferringGpPracticeName = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Fact]
        public void Invalid_Map_ReferralCreate_Missing_ReferralAttachmentId()
        {
          //Arrange
          string expected = "The ReferralAttachmentId field is required.";
          _modelToValidate.ReferralAttachmentId = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Theory]
        [InlineData(49)]
        [InlineData(251)]
        public void Invalid_Map_ReferralCreate_HeightOutOfRange(decimal height)
        {
          //Arrange
          string expected = "The field HeightCm must be between 50 and 250.";
          _modelToValidate.HeightCm = height;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Theory]
        [InlineData(34)]
        [InlineData(501)]
        public void Invalid_Map_ReferralCreate_WeightOutOfRange(decimal weight)
        {
          //Arrange
          string expected = "The field WeightKg must be between 35 and 500.";
          _modelToValidate.WeightKg = weight;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Theory]
        [InlineData(27)]
        [InlineData(91)]
        public void Invalid_Map_ReferralCreate_BmiOutOfRange(decimal bmi)
        {
          //Arrange
          string expected =
            "The field CalculatedBmiAtRegistration must be" +
            " between 27.5 and 90.";
          _modelToValidate.CalculatedBmiAtRegistration = bmi;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

      }

      [Fact]
      public async Task Invalid_Update_ReferralNotFoundException_400()
      {
        //Arrange
        int expected = 400;
        Mock<ReferralPost> request = new Mock<ReferralPost>();
        Mock<IReferral> mockReferral = new Mock<IReferral>();
        _mockReferralService
          .Setup(t => t.CreateReferral(It.IsAny<IReferralCreate>()))
         .Throws(new ReferralNotUniqueException("expected Exception"));
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };
        //Act
        var response = await _classToTest.Post(request.Object);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task Invalid_Update_Exception_500()
      {
        //Arrange
        int expected = 500;
        Mock<ReferralPost> request = new Mock<ReferralPost>();
        Mock<IReferral> mockReferral = new Mock<IReferral>();
        _mockReferralService
          .Setup(t => t.CreateReferral(It.IsAny<IReferralCreate>()))
         .Throws(new DbUpdateException("expected Exception"));
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };
        //Act
        var response = await _classToTest.Post(request.Object);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expected);
      }
    }

    public class CriPutTests : ReferralControllerTests
    {
      [Fact]
      public async Task Valid_Returns_200()
      {
        //Arrange
        int expected = 200;
        string ubrn = "123456789123";
        Mock<CriUpdateRequest> request = new Mock<CriUpdateRequest>();
        Mock<ReferralClinicalInfo> criDocumentBytes =
          new Mock<ReferralClinicalInfo>();
        Mock<CriCrudResponse> mockResponse = new Mock<CriCrudResponse>();


        _mockMapper.Setup(t => t.Map<ReferralClinicalInfo>
          (It.IsAny<CriUpdateRequest>())).Returns(criDocumentBytes.Object);

        _mockReferralService
          .Setup(expression: t =>
            t.CriCrudAsync(It.IsAny<ReferralClinicalInfo>(), false))
          .ReturnsAsync(mockResponse.Object);
        // .Returns(Task.FromResult(mockResponse.Object));
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };
        //Act
        var response = await _classToTest.CriPut(request.Object, ubrn);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkResult>();
        OkResult result =
          response as OkResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task InValid_Response_Returns_BadRequest()
      {
        //Arrange
        int expected = 400;
        string ubrn = "123456789123";
        string expectedError = "test error";
        Mock<CriUpdateRequest> request = new Mock<CriUpdateRequest>();
        Mock<ReferralClinicalInfo> criDocumentBytes =
          new Mock<ReferralClinicalInfo>();
        Mock<CriCrudResponse> mockResponse = new Mock<CriCrudResponse>();
        mockResponse.Setup(t => t.ResponseStatus)
          .Returns(StatusType.NoRowsUpdated);
        mockResponse.Setup(t => t.GetErrorMessage()).Returns(expectedError);

        _mockMapper.Setup(t => t.Map<ReferralClinicalInfo>
          (It.IsAny<CriUpdateRequest>())).Returns(criDocumentBytes.Object);

        _mockReferralService
          .Setup(expression: t =>
            t.CriCrudAsync(It.IsAny<ReferralClinicalInfo>(), false))
          .ReturnsAsync(mockResponse.Object);
        // .Returns(Task.FromResult(mockResponse.Object));
        _classToTest =
          new ReferralController(
            _mockReferralService.Object,
            _mockMapper.Object,
            _mockProcessStatusService.Object,
            _mockProcessStatusOptions.Object)
          {
            ControllerContext = new ControllerContext
            {
              HttpContext =
            new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
            }
          };
        //Act
        var response = await _classToTest.CriPut(request.Object, ubrn);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expected);
        ((ProblemDetails)result.Value).Detail.Should().Be(expectedError);
      }
    }

    public class DischargesTests : ReferralControllerTests
    {
      [Fact]
      public async Task PostDischargesNotAuthorised()
      {
        // Arrange.
        _mockProcessStatusOptions.Setup(o => o.Value).Returns(new Api.Models.ProcessStatusOptions
        {
          PostDischargesAppName = "WmsHub.Referral.Api.Service.PostDischarges.Hourly"
        });
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.NotAuthorized")
            }
          }
        };
        string message = "Access has not been granted for this endpoint.";

        // Act.
        IActionResult response = await _classToTest.PostDischarges();

        // Assert.
        UnauthorizedObjectResult result = response.Should().NotBeNull()
          .And.Subject.Should().BeOfType<UnauthorizedObjectResult>()
          .Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        result.Value.Should().Be(message);
      }

      [Fact]
      public async Task PostDischargesNoDischargesSuccess()
      {
        // Arrange.
        _mockProcessStatusOptions.Setup(o => o.Value).Returns(new Api.Models.ProcessStatusOptions
        {
          PostDischargesAppName = "WmsHub.Referral.Api.Service.PostDischarges.Hourly"
        });
        _mockReferralService
         .Setup(expression: s => s.GetDischargesForGpDocumentProxy())
         .ReturnsAsync([]);
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext 
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.PostDischarges();

        // Assert.
        response.Should().NotBeNull();
        response.Should().BeOfType<NoContentResult>();
        _mockProcessStatusService.Verify(t => t.StartedAsync(), Times.Once);
        _mockProcessStatusService.Verify(t => t.SuccessAsync(), Times.Once);
      }

      [Fact]
      public async Task PostDischargesSuccess()
      {
        // Arrange.
        _mockProcessStatusOptions.Setup(o => o.Value).Returns(new Api.Models.ProcessStatusOptions
        {
          PostDischargesAppName = "WmsHub.Referral.Api.Service.PostDischarges.Hourly"
        });
        Guid referralId = Guid.NewGuid();
        _mockReferralService
         .Setup(expression: s => s.GetDischargesForGpDocumentProxy())
         .ReturnsAsync(new List<Business.Models.GpDocumentProxy.GpDocumentProxyReferralDischarge>
         {
           new() { Id = referralId }
         });
        _mockReferralService
          .Setup(expression: s => s.PostDischarges(
            It.IsAny<List<Business.Models.GpDocumentProxy.GpDocumentProxyReferralDischarge>>()))
          .ReturnsAsync([referralId]);
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.PostDischarges();

        // Assert.
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        _mockProcessStatusService.Verify(t => t.StartedAsync(), Times.Once);
        _mockProcessStatusService.Verify(t => t.SuccessAsync(), Times.Once);
      }

      [Fact]
      public async Task PostDischargesExceptionFailure()
      {
        // Arrange.
        _mockProcessStatusOptions.Setup(o => o.Value).Returns(new Api.Models.ProcessStatusOptions
        {
          PostDischargesAppName = "WmsHub.Referral.Api.Service.PostDischarges.Hourly"
        });
        Guid referralId = Guid.NewGuid();
        string errorMessage = "Test Error";
        string exceptionMessage = $"Post Discharges ran with errors, latest error: {errorMessage}";
        Business.Models.GpDocumentProxy.GpDocumentProxyReferralDischarge discharge = new()
        {
          Id = referralId
        };
        _mockReferralService
         .Setup(expression: s => s.GetDischargesForGpDocumentProxy())
         .ReturnsAsync(new List<Business.Models.GpDocumentProxy.GpDocumentProxyReferralDischarge>
         {
           new() { Id = referralId }
         });
        _mockReferralService
          .Setup(expression: s => s.PostDischarges(
            It.IsAny<List<Business.Models.GpDocumentProxy.GpDocumentProxyReferralDischarge>>()))
          .ThrowsAsync(new PostDischargesException(errorMessage));
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.PostDischarges();

        // Assert.
        OkObjectResult result = response.Should().NotBeNull()
          .And.Subject.Should().BeOfType<OkObjectResult>()
          .Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        _mockProcessStatusService.Verify(t => t.StartedAsync(), Times.Once);
        _mockProcessStatusService.Verify(t => t.FailureAsync(exceptionMessage), Times.Once);
      }

      [Fact]
      public async Task PostDischargesOtherExceptionFailure()
      {
        // Arrange.
        _mockProcessStatusOptions.Setup(o => o.Value).Returns(new Api.Models.ProcessStatusOptions
        {
          PostDischargesAppName = "WmsHub.Referral.Api.Service.PostDischarges.Hourly"
        });
        Guid referralId = Guid.NewGuid();
        string exceptionMessage = "Test Exception";
        Business.Models.GpDocumentProxy.GpDocumentProxyReferralDischarge discharge = new()
        {
          Id = referralId
        };
        _mockReferralService
         .Setup(expression: s => s.GetDischargesForGpDocumentProxy())
         .ReturnsAsync(new List<Business.Models.GpDocumentProxy.GpDocumentProxyReferralDischarge>
         {
           new() { Id = referralId }
         });
        _mockReferralService
          .Setup(expression: s => s.PostDischarges(
            It.IsAny<List<Business.Models.GpDocumentProxy.GpDocumentProxyReferralDischarge>>()))
          .ThrowsAsync(new InvalidTokenException(exceptionMessage));
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.PostDischarges();

        // Assert.
        ObjectResult result = response.Should().NotBeNull()
          .And.Subject.Should().BeOfType<ObjectResult>()
          .Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        _mockProcessStatusService.Verify(t => t.StartedAsync(), Times.Once);
        _mockProcessStatusService.Verify(t => t.FailureAsync(exceptionMessage), Times.Once);
      }

      [Fact]
      public async Task UpdateDischargesNotAuthorised()
      {
        // Arrange.
        _mockProcessStatusOptions.Setup(o => o.Value).Returns(new Api.Models.ProcessStatusOptions
        {
          PostDischargesAppName = "WmsHub.Referral.Api.Service.UpdateDischarges.Hourly"
        });
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.NotAuthorized")
            }
          }
        };
        string message = "Access has not been granted for this endpoint.";

        // Act.
        IActionResult response = await _classToTest.UpdateDischarges();

        // Assert.
        UnauthorizedObjectResult result = response.Should().NotBeNull()
          .And.Subject.Should().BeOfType<UnauthorizedObjectResult>()
          .Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        result.Value.Should().Be(message);
      }

      [Fact]
      public async Task UpdateDischargesSuccess()
      {
        // Arrange.
        _mockProcessStatusOptions.Setup(o => o.Value).Returns(new Api.Models.ProcessStatusOptions
        {
          PostDischargesAppName = "WmsHub.Referral.Api.Service.UpdateDischarges.Hourly"
        });
        _mockReferralService
          .Setup(expression: s => s.UpdateDischarges())
          .ReturnsAsync(new Business.Models.GpDocumentProxy.GpDocumentProxyUpdateResponse()
          {
            Discharges = new()
            {
              new Business.Models.GpDocumentProxy.GpDocumentProxyUpdateResponseItem()
              {
                Status = DocumentStatus.Accepted.ToString(),
                UpdateStatus = DocumentUpdateStatus.Updated.ToString(),
                Ubrn = "GP000000001"
              }
            }
          });
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.UpdateDischarges();

        // Assert.
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        _mockProcessStatusService.Verify(t => t.StartedAsync(), Times.Once);
        _mockProcessStatusService.Verify(t => t.SuccessAsync(), Times.Once);
      }

      [Fact]
      public async Task UpdateDischargesExceptionFailure()
      {
        // Arrange.
        _mockProcessStatusOptions.Setup(o => o.Value).Returns(new Api.Models.ProcessStatusOptions
        {
          PostDischargesAppName = "WmsHub.Referral.Api.Service.UpdateDischarges.Hourly"
        });
        Business.Models.GpDocumentProxy.GpDocumentProxyUpdateResponse update = new()
        {
          Discharges = new()
            {
              new Business.Models.GpDocumentProxy.GpDocumentProxyUpdateResponseItem()
              {
                UpdateStatus = DocumentUpdateStatus.Error.ToString(),
                Ubrn = "GP000000001"
              }
            }
        };
        string errorMessage = "Test Error";
        string exceptionMessage = 
          $"Update Discharges ran with errors, latest error: {errorMessage}";
        string updateString = JsonSerializer.Serialize(update);
        _mockReferralService
          .Setup(expression: s => s.UpdateDischarges())
          .ThrowsAsync(new UpdateDischargesException(errorMessage));
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.UpdateDischarges();

        // Assert.
        OkObjectResult result = response.Should().NotBeNull()
          .And.Subject.Should().BeOfType<OkObjectResult>()
          .Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        _mockProcessStatusService.Verify(t => t.StartedAsync(), Times.Once);
        _mockProcessStatusService.Verify(t => t.FailureAsync(exceptionMessage), Times.Once);
      }

      [Fact]
      public async Task UpdateDischargesOtherExceptionFailure()
      {
        // Arrange.
        _mockProcessStatusOptions.Setup(o => o.Value).Returns(new Api.Models.ProcessStatusOptions
        {
          PostDischargesAppName = "WmsHub.Referral.Api.Service.UpdateDischarges.Hourly"
        });
        Business.Models.GpDocumentProxy.GpDocumentProxyUpdateResponse update = new()
        {
          Discharges = new()
            {
              new Business.Models.GpDocumentProxy.GpDocumentProxyUpdateResponseItem()
              {
                UpdateStatus = DocumentUpdateStatus.Error.ToString(),
                Ubrn = "GP000000001"
              }
            }
        };
        string exceptionMessage = "Test Exception";
        _mockReferralService
          .Setup(expression: s => s.UpdateDischarges())
          .ThrowsAsync(new InvalidTokenException(exceptionMessage));
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.UpdateDischarges();

        // Assert.
        ObjectResult result = response.Should().NotBeNull()
          .And.Subject.Should().BeOfType<ObjectResult>()
          .Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        _mockProcessStatusService.Verify(t => t.StartedAsync(), Times.Once);
        _mockProcessStatusService.Verify(t => t.FailureAsync(exceptionMessage), Times.Once);
      }
    }

    public class TerminateTests : ReferralControllerTests
    {
      private readonly string _appName = 
        "WmsHub.Referral.Api.Service.TerminateNotStartedProgramme.Daily";

      public TerminateTests()
      {
        _mockProcessStatusOptions.Setup(o => o.Value).Returns(new Api.Models.ProcessStatusOptions
        {
          TerminateNotStartedProgrammeReferralsAppName = _appName
        });
      }

      [Theory]
      [InlineData("")]
      [InlineData("Invalid")]
      public async Task ReasonNotValidBadRequest(string reason)
      {
        // Arrange.
        int expectedStatusCode = StatusCodes.Status400BadRequest;
        string expectedMessage = ReferralApiConstants.INVALIDTERMINATIONREASON;
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest.Terminate(reason);

        // Assert.
        ObjectResult result = response.Should().NotBeNull()
          .And.Subject.Should().BeOfType<ObjectResult>()
          .Subject;
        result.StatusCode.Should().Be(expectedStatusCode);
        ((ProblemDetails)result.Value).Detail.Should().Be(expectedMessage);
        _mockProcessStatusService.Verify(t => t.StartedAsync(), Times.Once);
        _mockProcessStatusService.Verify(t => t.FailureAsync(expectedMessage), Times.Once);
      }

      [Fact]
      public async Task ReasonProgrammeNotStartedOk()
      {
        // Arrange.
        int expectedStatusCode = StatusCodes.Status200OK;
        int expectedNumberOfTerminatedReferrals = 1;
        _mockReferralService
          .Setup(expression: s => s.TerminateNotStartedProgrammeReferralsAsync())
          .ReturnsAsync(expectedNumberOfTerminatedReferrals);
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest
          .Terminate(TerminationReason.ProgrammeNotStarted.ToString());

        // Assert.
        OkObjectResult result = response.Should().NotBeNull()
          .And.Subject.Should().BeOfType<OkObjectResult>()
          .Subject;
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Value.Should().Be(expectedNumberOfTerminatedReferrals);
        _mockProcessStatusService.Verify(t => t.StartedAsync(), Times.Once);
        _mockProcessStatusService.Verify(t => t.SuccessAsync(), Times.Once);
      }

      [Fact]
      public async Task ThrowsExceptionInternalServerError()
      {
        // Arrange.
        int expectedStatusCode = StatusCodes.Status500InternalServerError;
        string exceptionMessage = "Test Exception";
        _mockReferralService
          .Setup(expression: s => s.TerminateNotStartedProgrammeReferralsAsync())
          .ThrowsAsync(new InvalidTokenException(exceptionMessage));
        _classToTest = new ReferralController(
          _mockReferralService.Object,
          _mockMapper.Object,
          _mockProcessStatusService.Object,
          _mockProcessStatusOptions.Object)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext
            {
              User = GetUnknownClaimsPrincipal("Referral.Service")
            }
          }
        };

        // Act.
        IActionResult response = await _classToTest
          .Terminate(TerminationReason.ProgrammeNotStarted.ToString());

        // Assert.
        ObjectResult result = response.Should().NotBeNull()
          .And.Subject.Should().BeOfType<ObjectResult>()
          .Subject;
        result.StatusCode.Should().Be(expectedStatusCode);
        _mockProcessStatusService.Verify(t => t.StartedAsync(), Times.Once);
        _mockProcessStatusService.Verify(t => t.FailureAsync(exceptionMessage), Times.Once);
      }
    }
  }
}
