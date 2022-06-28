using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Referral.Api.Controllers;
using WmsHub.Referral.Api.Models;
using Xunit;

namespace WmsHub.Referral.Api.Tests
{
  public class ReferralControllerTests : TestSetup
  {
    private ReferralController _classToTest;

    public ReferralControllerTests()
    {
      _classToTest =
        new ReferralController(_mockReferralService.Object, _mockMapper.Object);
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
        _ubrnList = new List<ActiveReferralAndExceptionUbrn>
          {_mockReferral.Object};
      }

      [Theory]
      [InlineData("123456")]
      [InlineData(null)]
      public async Task Valid_Return_200(string serviceId)
      {
        //Arrange
        int expected = 200;

        _mockReferralService
          .Setup(t => t.GetActiveReferralAndExceptionUbrns(serviceId))
         .Returns(Task.FromResult(_ubrnList));
        _classToTest =
          new ReferralController(
            _mockReferralService.Object, _mockMapper.Object)
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
        var response = await _classToTest.GetActiveUbrns(serviceId);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task Invalid_Claim_Return_401()
      {
        //Arrange
        int expected = 401;
        string serviceId = "123456";

        _mockReferralService
          .Setup(t => t.GetActiveReferralAndExceptionUbrns(serviceId))
         .Returns(Task.FromResult(_ubrnList));
        _classToTest =
          new ReferralController(
            _mockReferralService.Object, _mockMapper.Object)
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
          .Setup(t => t.GetActiveReferralAndExceptionUbrns(serviceId))
         .Throws(new DbUpdateException(expectedMessage));
        _classToTest =
          new ReferralController(
            _mockReferralService.Object, _mockMapper.Object)
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
          Assert.True(false, "Test to be written");
        }
        catch (DbUpdateException ex)
        {
          Assert.True(true, ex.Message);
          ex.Message.Should().Be(expectedMessage);
        }
        catch (Exception ex)
        {
          Assert.True(false, ex.Message);
        }
      }
    }

    public class PutTests : ReferralControllerTests
    {
      [Fact]
      public async Task Valid_Returns_200()
      {
        //Arrange
        Random random = new Random();
        int expected = 200;
        Mock<ReferralPut> request = new Mock<ReferralPut>();
        string ubrn = Generators.GenerateUbrn(random);
        Mock<ReferralUpdate> referralUpdate = new Mock<ReferralUpdate>();
        _mockMapper.Setup(t => t.Map<ReferralUpdate>
          (It.IsAny<ReferralPut>())).Returns(referralUpdate.Object);

        Mock<IReferral> mockReferral = new Mock<IReferral>();
        _mockReferralService.Setup(t => t
          .UpdateGpReferral(It.IsAny<IReferralUpdate>()))
            .Returns(Task.FromResult(mockReferral.Object));

        _classToTest =
          new ReferralController(
            _mockReferralService.Object, _mockMapper.Object)
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
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
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
            _mockReferralService.Object, _mockMapper.Object)
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
            _mockReferralService.Object, _mockMapper.Object)
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
          _mockReferralService.Object, _mockMapper.Object)
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
      public async Task Valid_Returns_200()
      {
        //Arrange
        int expected = 200;
        Mock<ReferralPost> request = new Mock<ReferralPost>();
        Mock<IReferral> mockReferral = new Mock<IReferral>();
        _mockReferralService
          .Setup(t => t.CreateReferral(It.IsAny<IReferralCreate>()))
         .Returns(Task.FromResult(mockReferral.Object));
        _classToTest = new ReferralController(
          _mockReferralService.Object, _mockMapper.Object)
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
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(expected);
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
      public async Task Valid_Convert_NullEmail(string email)
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
          ReferralAttachmentId = 0,
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
          .AddMaps(new[] {"WmsHub.Referral.Api"})).CreateMapper();

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

      public async Task Valid_Convert_CleanedEmail(string email)
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
          ReferralAttachmentId = 0,
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
      public async Task Valid_Convert_CleanedEmailWithSpaces(string email)
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
          ReferralAttachmentId = 0,
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
          _mockReferralService.Object, _mockMapper.Object)
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
            referralAttachmentId: 123456,
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
        public async Task Invalid_Map_ReferralCreate_Missing_NhsNumber()
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
        public async Task Invalid_Map_ReferralCreate_Missing_Tel_And_Mobile(
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
        public async Task Invalid_Map_ReferralCreate_Missing_DateOfReferral()
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
        public async Task
          Invalid_Map_ReferralCreate_Missing_ReferralPracticeNumber()
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
        public async Task Invalid_Map_ReferralCreate_Missing_Ubrn()
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
        public async Task Invalid_Map_ReferralCreate_Missing_FamilyName()
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
        public async Task Invalid_Map_ReferralCreate_Missing_GivenName()
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
        public async Task Valid_Map_ReferralCreate_Missing_Address1(
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
        public async Task Invalid_Map_ReferralCreate_Missing_Postcode()
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
        public async Task Invalid_Map_ReferralCreate_Missing_DOB()
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
        public async Task Invalid_Map_ReferralCreate_Missing_Gender()
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
        public async Task Valid_Map_ReferralCreate_Missing_IsVulnerable()
        {
          //Arrange
          _modelToValidate.IsVulnerable = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Valid_Map_ReferralCreate_Missing_PhysicalDisability()
        {
          //Arrange
          _modelToValidate.HasAPhysicalDisability = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Valid_Map_ReferralCreate_Missing_LearningDisability()
        {
          //Arrange
          _modelToValidate.HasALearningDisability = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Valid_Map_ReferralCreate_Missing_MentalIllness()
        {
          //Arrange
          _modelToValidate.HasRegisteredSeriousMentalIllness = null;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Invalid_Map_ReferralCreate_Missing_Hypertension()
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
        public async Task Invalid_Map_ReferralCreate_Missing_Type1Diabetes()
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
        public async Task Invalid_Map_ReferralCreate_Missing_Type2Diabetes()
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
        public async Task Invalid_Map_ReferralCreate_Missing_Bmi()
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
        public async Task Invalid_Map_ReferralCreate_Missing_GpPracticeName()
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
        public async Task
          Invalid_Map_ReferralCreate_Missing_ReferralAttachmentId()
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

        [Fact]
        public async Task
          Invalid_Map_ReferralCreate_ReferralAttachmentId_Above0()
        {
          //Arrange
          string expected =
            "The field ReferralAttachmentId must be between 0" +
            " and 9.223372036854776E+18.";
          _modelToValidate.ReferralAttachmentId = -100;
          //Act
          ValidateModelResult result = ValidateModel(_modelToValidate);
          //Assert
          result.IsValid.Should().BeFalse();
          result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
        }

        [Theory]
        [InlineData(49)]
        [InlineData(251)]
        public async Task
          Invalid_Map_ReferralCreate_HeightOutOfRange(decimal height)
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
        public async Task Invalid_Map_ReferralCreate_WeightOutOfRange(
          decimal weight)
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
        public async Task Invalid_Map_ReferralCreate_BmiOutOfRange(decimal bmi)
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

        [Theory]        
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("111", "222")]
        public async Task Invalid_Map_ReferralCreate_No_Telephone_Or_Mobile(
          string telephone, string mobile)
        {
          //Arrange
          string expected =
            "One of the fields: Telephone or Mobile is required.";
          _modelToValidate.Telephone = telephone;
          _modelToValidate.Mobile = mobile;
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
          _mockReferralService.Object, _mockMapper.Object)
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
          _mockReferralService.Object, _mockMapper.Object)
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
        _classToTest =new ReferralController(
          _mockReferralService.Object, _mockMapper.Object)
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
            _mockReferralService.Object, _mockMapper.Object)
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

  
  }
}
