using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Common.Api.Models;
using WmsHub.Referral.Api.Controllers;
using Xunit;

namespace WmsHub.Referral.Api.Tests
{
  public class ReferralExceptionControllerTests : TestSetup
  {
    private readonly ReferralExceptionController _classToTest;

    public ReferralExceptionControllerTests()
    {
      _classToTest = new ReferralExceptionController(
        _mockReferralService.Object,
        Mapper);
    }

    public void AddReferralServiceNameClaimToControllerContext(
      string name = CLAIM_NAME_REFERRAL_SERVICE)
    {
      _classToTest.ControllerContext = new ControllerContext()
      {
        HttpContext = new DefaultHttpContext
        {
          User = GetUnknownClaimsPrincipal(name)
        }
      };
    }

    public class PostMissingAttachment : ReferralExceptionControllerTests
    {
      [Fact]
      public async Task Valid()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        var referralMissingAttachmentPost = new ReferralMissingAttachmentPost()
        {
          Ubrn = TEST_UBRN
        };

        IReferralExceptionCreate referralExCreate = null;
        var expReferral = new Business.Models.Referral() {Ubrn = TEST_UBRN};

        _mockReferralService
          .Setup(t => t.CreateException(It.IsAny<IReferralExceptionCreate>()))
          .Callback<IReferralExceptionCreate>(obj => referralExCreate = obj)
          .Returns(Task.FromResult<IReferral>(expReferral));

        //Act
        var response = await _classToTest
          .PostMissingAttachment(referralMissingAttachmentPost);

        //Assert
        referralExCreate.ExceptionType.Should().Be(
          CreateReferralException.MissingAttachment);
        referralExCreate.Should().BeEquivalentTo(referralMissingAttachmentPost);

        var result = Assert.IsType<OkObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        var referral = Assert.IsType<Business.Models.Referral>(result.Value);
        referral.Should().BeEquivalentTo(expReferral);
      }

      [Fact]
      public async Task ReferralNotUniqueException()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        var referralMissingAttachmentPost = new ReferralMissingAttachmentPost();
        var expectedExceptionMessage = "ReferralNotUnique";

        _mockReferralService.Setup(t =>
            t.CreateException(It.IsAny<IReferralExceptionCreate>()))
          .ThrowsAsync(
            new ReferralNotUniqueException(expectedExceptionMessage));

        //Act
        var response = await _classToTest
          .PostMissingAttachment(referralMissingAttachmentPost);

        //Assert
        var result = Assert.IsType<ObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        problem.Detail.Should().Be(expectedExceptionMessage);
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
      }

      [Fact]
      public async Task InternalServerError()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        var referralMissingAttachmentPost = new ReferralMissingAttachmentPost();
        var expectedExceptionMessage = "InternalServerError";

        _mockReferralService.Setup(t =>
            t.CreateException(It.IsAny<IReferralExceptionCreate>()))
          .ThrowsAsync(new Exception(expectedExceptionMessage));

        //Act
        var response = await _classToTest
          .PostMissingAttachment(referralMissingAttachmentPost);

        //Assert
        var result = Assert.IsType<ObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        problem.Detail.Should().Be(expectedExceptionMessage);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
      }

      [Fact]
      public async Task Unauthorized_NoUser()
      {
        //Arrange
        var referralMissingAttachmentPost = new ReferralMissingAttachmentPost();

        //Act
        var response = await _classToTest
          .PostMissingAttachment(referralMissingAttachmentPost);

        //Assert
        var result = Assert.IsType<UnauthorizedObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
      }

      [Fact]
      public async Task Unauthorized_UserInvalidClaim()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext("Invalid");
        var referralMissingAttachmentPost = new ReferralMissingAttachmentPost();

        //Act
        var response = await _classToTest
          .PostMissingAttachment(referralMissingAttachmentPost);

        //Assert
        var result = Assert.IsType<UnauthorizedObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
      }
    }

    public class PostInvalidAttachment : ReferralExceptionControllerTests
    {
      [Fact]
      public async Task Valid()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        var postMethod = new ReferralInvalidAttachmentPost()
        {
          Ubrn = TEST_UBRN
        };

        IReferralExceptionCreate referralExCreate = null;
        var expReferral = new Business.Models.Referral() { Ubrn = TEST_UBRN };

        _mockReferralService
          .Setup(t => t.CreateException(It.IsAny<IReferralExceptionCreate>()))
          .Callback<IReferralExceptionCreate>(obj => referralExCreate = obj)
          .Returns(Task.FromResult<IReferral>(expReferral));

        //Act
        var response = await _classToTest
          .PostInvalidAttachment(postMethod);

        //Assert
        referralExCreate.ExceptionType.Should().Be(
          CreateReferralException.InvalidAttachment);
        referralExCreate.Should().BeEquivalentTo(postMethod);

        var result = Assert.IsType<OkObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        var referral = Assert.IsType<Business.Models.Referral>(result.Value);
        referral.Should().BeEquivalentTo(expReferral);
      }

      [Fact]
      public async Task ReferralNotUniqueException()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        var postMethod = new ReferralInvalidAttachmentPost();
        var expectedExceptionMessage = "ReferralNotUnique";

        _mockReferralService.Setup(t =>
            t.CreateException(It.IsAny<IReferralExceptionCreate>()))
          .ThrowsAsync(
            new ReferralNotUniqueException(expectedExceptionMessage));

        //Act
        var response = await _classToTest
          .PostInvalidAttachment(postMethod);

        //Assert
        var result = Assert.IsType<ObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        problem.Detail.Should().Be(expectedExceptionMessage);
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
      }

      [Fact]
      public async Task InternalServerError()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        var postMethod = new ReferralInvalidAttachmentPost();
        var expectedExceptionMessage = "InternalServerError";

        _mockReferralService.Setup(t =>
            t.CreateException(It.IsAny<IReferralExceptionCreate>()))
          .ThrowsAsync(new Exception(expectedExceptionMessage));

        //Act
        var response = await _classToTest
          .PostInvalidAttachment(postMethod);

        //Assert
        var result = Assert.IsType<ObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        problem.Detail.Should().Be(expectedExceptionMessage);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
      }

      [Fact]
      public async Task Unauthorized_NoUser()
      {
        //Arrange
        var referralMissingAttachmentPost = new ReferralMissingAttachmentPost();

        //Act
        var response = await _classToTest
          .PostMissingAttachment(referralMissingAttachmentPost);

        //Assert
        var result = Assert.IsType<UnauthorizedObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
      }

      [Fact]
      public async Task Unauthorized_UserInvalidClaim()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext("Invalid");
        var referralMissingAttachmentPost = new ReferralMissingAttachmentPost();

        //Act
        var response = await _classToTest
          .PostMissingAttachment(referralMissingAttachmentPost);

        //Assert
        var result = Assert.IsType<UnauthorizedObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
      }
    }

    public class PostNhsNumberMismatch : ReferralExceptionControllerTests
    {
      [Fact]
      public async Task Valid()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        var referralNhsNumberMismatchPost = new ReferralNhsNumberMismatchPost()
        {
          NhsNumberAttachment = "9999999999",
          NhsNumberWorkList = "1111111111",
          Ubrn = TEST_UBRN
        };

        IReferralExceptionCreate referralExCreate = null;
        var expReferral = new Business.Models.Referral() {Ubrn = TEST_UBRN};

        _mockReferralService
          .Setup(t => t.CreateException(It.IsAny<IReferralExceptionCreate>()))
          .Callback<IReferralExceptionCreate>(obj => referralExCreate = obj)
          .Returns(Task.FromResult<IReferral>(expReferral));

        //Act
        var response = await _classToTest
          .PostNhsNumberMismatch(referralNhsNumberMismatchPost);

        //Assert
        referralExCreate.ExceptionType.Should().Be(
          CreateReferralException.NhsNumberMismatch);
        referralExCreate.Should().BeEquivalentTo(referralNhsNumberMismatchPost);

        var result = Assert.IsType<OkObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        var referral = Assert.IsType<Business.Models.Referral>(result.Value);
        referral.Should().BeEquivalentTo(expReferral);
      }

      [Fact]
      public async Task ReferralNotUniqueException()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        var referralNhsNumberMismatchPost = new ReferralNhsNumberMismatchPost();
        var expectedExceptionMessage = "ReferralNotUnique";

        _mockReferralService.Setup(t =>
            t.CreateException(It.IsAny<IReferralExceptionCreate>()))
          .ThrowsAsync(
            new ReferralNotUniqueException(expectedExceptionMessage));

        //Act
        var response = await _classToTest
          .PostNhsNumberMismatch(referralNhsNumberMismatchPost);

        //Assert
        var result = Assert.IsType<ObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        problem.Detail.Should().Be(expectedExceptionMessage);
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
      }

      [Fact]
      public async Task InternalServerError()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        var referralNhsNumberMismatchPost = new ReferralNhsNumberMismatchPost();
        var expectedExceptionMessage = "InternalServerError";

        _mockReferralService.Setup(t =>
            t.CreateException(It.IsAny<IReferralExceptionCreate>()))
          .ThrowsAsync(new Exception(expectedExceptionMessage));

        //Act
        var response = await _classToTest
          .PostNhsNumberMismatch(referralNhsNumberMismatchPost);

        //Assert
        var result = Assert.IsType<ObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        problem.Detail.Should().Be(expectedExceptionMessage);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
      }

      [Fact]
      public async Task Unauthorized_NoUser()
      {
        //Arrange
        var referralNhsNumberMismatchPost = new ReferralNhsNumberMismatchPost();

        //Act
        var response = await _classToTest
          .PostNhsNumberMismatch(referralNhsNumberMismatchPost);

        //Assert
        var result = Assert.IsType<UnauthorizedObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
      }

      [Fact]
      public async Task Unauthorized_UserInvalidClaim()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext("Invalid");
        var referralNhsNumberMismatchPost = new ReferralNhsNumberMismatchPost();

        //Act
        var response = await _classToTest
          .PostNhsNumberMismatch(referralNhsNumberMismatchPost);

        //Assert
        var result = Assert.IsType<UnauthorizedObjectResult>(response);
        result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
      }
    }

    public class PutMissingAttachmentTests : ReferralExceptionControllerTests
    {
      [Fact]
      public async Task ValidUpdate_Status_200()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        Mock<IReferral> referral = new();
        referral.Object.Ubrn = ubrn;
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .ReturnsAsync(referral.Object);
        //Act
        var response = await _classToTest.PutMissingAttachment(ubrn);
        //Assert
        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
      }

      [Fact]
      public async Task ReferralUpdateException_Test_Return_Status_500()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .Throws(new ReferralUpdateException("Test Exception"));

        //Act
        var response = await _classToTest.PutMissingAttachment(ubrn);
        //Assert
        ObjectResult outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
      }

      [Fact]
      public async Task ReferralNotFoundException_Test_Return_Status_404()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .Throws(new ReferralNotFoundException("Test Exception"));

        //Act
        var response = await _classToTest.PutMissingAttachment(ubrn);
        //Assert
        ObjectResult outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
      }

      [Fact]
      public async Task ReferralInvalidStatusException_Test_Return_Status_409()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .Throws(new ReferralInvalidStatusException("Test Exception"));

        //Act
        var response = await _classToTest.PutMissingAttachment(ubrn);
        //Assert
        ObjectResult outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
      }
    }

    public class PutInvalidAttachmentTests : ReferralExceptionControllerTests
    {
      [Fact]
      public async Task ValidUpdate_Status_200()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        Mock<IReferral> referral = new();
        referral.Object.Ubrn = ubrn;
        IReferralExceptionUpdate referralExUpdate = null;
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .Callback<IReferralExceptionUpdate>(obj => referralExUpdate = obj)
          .ReturnsAsync(referral.Object);
        //Act
        var response = await _classToTest.PutInvalidAttachment(ubrn);
        //Assert
        referralExUpdate.ExceptionType.Should().Be(
          CreateReferralException.InvalidAttachment);

        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
      }

      [Fact]
      public async Task ReferralUpdateException_Test_Return_Status_500()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .Throws(new ReferralUpdateException("Test Exception"));

        //Act
        var response = await _classToTest.PutMissingAttachment(ubrn);
        //Assert
        ObjectResult outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
      }

      [Fact]
      public async Task ReferralNotFoundException_Test_Return_Status_404()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .Throws(new ReferralNotFoundException("Test Exception"));

        //Act
        var response = await _classToTest.PutMissingAttachment(ubrn);
        //Assert
        ObjectResult outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
      }

      [Fact]
      public async Task ReferralInvalidStatusException_Test_Return_Status_409()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .Throws(new ReferralInvalidStatusException("Test Exception"));

        //Act
        var response = await _classToTest.PutMissingAttachment(ubrn);
        //Assert
        ObjectResult outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
      }
    }

    public class PutNhsNumberMismatchTest : ReferralExceptionControllerTests
    {
      [Fact]
      public async Task ValidUpdate_Status_200()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        Mock<IReferral> referral = new();
        referral.Object.Ubrn = ubrn;
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .ReturnsAsync(referral.Object);
        var putMethod = new ReferralNhsNumberMismatchPost()
        {
          Ubrn = ubrn,
          NhsNumberAttachment = "9999888801",
          NhsNumberWorkList = "9999888802"
        };

        //Act
        var response = await _classToTest.PutNhsNumberMismatch(putMethod);
        //Assert
        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
      }


      [Fact]
      public async Task ReferralUpdateException_Test_Return_Status_500()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .Throws(new ReferralUpdateException("Test Exception"));

        var putMethod = new ReferralNhsNumberMismatchPost()
        {
          Ubrn = ubrn,
          NhsNumberAttachment = "9999888801",
          NhsNumberWorkList = "9999888802"
        };

        //Act
        var response = await _classToTest.PutNhsNumberMismatch(putMethod);
        //Assert
        ObjectResult outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
      }

      [Fact]
      public async Task ReferralNotFoundException_Test_Return_Status_404()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .Throws(new ReferralNotFoundException("Test Exception"));

        var putMethod = new ReferralNhsNumberMismatchPost()
        {
          Ubrn = ubrn,
          NhsNumberAttachment = "9999888801",
          NhsNumberWorkList = "9999888802"
        };

        //Act
        var response = await _classToTest.PutNhsNumberMismatch(putMethod);
        //Assert
        ObjectResult outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
      }

      [Fact]
      public async Task ReferralInvalidStatusException_Test_Return_Status_409()
      {
        //Arrange
        AddReferralServiceNameClaimToControllerContext();
        string ubrn = "123456123456";
        _mockReferralService.Setup(t =>
            t.UpdateReferralToStatusExceptionAsync(
              It.IsAny<IReferralExceptionUpdate>()))
          .Throws(new ReferralInvalidStatusException("Test Exception"));

        var putMethod = new ReferralNhsNumberMismatchPost()
        {
          Ubrn = ubrn,
          NhsNumberAttachment = "9999888801",
          NhsNumberWorkList = "9999888802"
        };

        //Act
        var response = await _classToTest.PutNhsNumberMismatch(putMethod);
        //Assert
        ObjectResult outputResult = Assert.IsType<ObjectResult>(response);
        response.Should().NotBeNull();
        outputResult.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
      }
    }
  }
}