using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Services;
using WmsHub.Provider.Api.Controllers;
using WmsHub.Provider.Api.Models;
using WmsHub.Tests.Helper;
using Xunit;


namespace WmsHub.ProviderApi.Tests
{
  public class ServiceUserControllerTests : TestSetup
  {
    private ServiceUserController _classToTest;

    public class PutTests : ServiceUserControllerTests
    {
      public PutTests()
      {

        _mockService = new Mock<ProviderService>(
          _mockContext.Object,
          _mockMapper.Object,
          TestConfiguration.CreateProviderOptions());
      }
      public class ReturnOkTests : PutTests
      {
        [Theory]
        [InlineData(ValidServiceUserSubmissionRequest.SubmissionStarted1)]
        [InlineData(ValidServiceUserSubmissionRequest
          .SubmissionStartedWithUpdates1)]
        [InlineData(ValidServiceUserSubmissionRequest.SubmissionRejected1)]
        [InlineData(ValidServiceUserSubmissionRequest.SubmissionRejected2)]
        [InlineData(ValidServiceUserSubmissionRequest.SubmissionComplete1)]
        [InlineData(ValidServiceUserSubmissionRequest
          .SubmissionCompletedWithUpdates)]
        [InlineData(ValidServiceUserSubmissionRequest.SumbmissionTerminated)]
        [InlineData(ValidServiceUserSubmissionRequest.SubmissionUpdate)]
        public async Task ValidRequests(string json)
        {
          //Arrange
          ServiceUserSubmissionRequest[] requests = JsonConvert
            .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

          IEnumerable<ServiceUserSubmissionResponse> responses =
            new List<ServiceUserSubmissionResponse> {
              new ServiceUserSubmissionResponse
              {
                ResponseStatus = Business.Enums.StatusType.Valid
              }
            };

          _mockService.Setup(x => x.ProviderSubmissionsAsync(It.
            IsAny<IEnumerable<ServiceUserSubmissionRequest>>()))
            .Returns(Task.FromResult(responses));

          _classToTest = new ServiceUserController(_mockService.Object,
              _mockMapper.Object);

          //Act
          var result = await _classToTest.Put(requests);

          // assert
          Assert.NotNull(result);
          Assert.IsType<OkResult>(result);
          var okResult = result as OkResult;
          Assert.Equal(200, okResult.StatusCode);

        }
      }

      public class ReturnErrorsTests : PutTests
      {
        [Theory]
        [InlineData(InValidServiceUserSubmissionRequests
          .SubmissionStartedMissinguUbrn, "The Ubrn field is required.")]
        [InlineData(InValidServiceUserSubmissionRequests
          .SubmissionStartedMissinguDate, "The Date field is required.")]
        [InlineData(InValidServiceUserSubmissionRequests
          .SubmissionStartedMissinguType, "The Type field is required.")]
        [InlineData(InValidServiceUserSubmissionRequests
          .SubmissionStartedWithUpdatesMissingDate, 
          "The Date field is required.")]
        [InlineData(InValidServiceUserSubmissionRequests
          .SubmissionStartedWithUpdatesMissingWeight,
          "The Updates field '0 occurances of Weight, Measure or Coaching' " +
          "is invalid.")]
        [InlineData(InValidServiceUserSubmissionRequests
          .SubmissionUpdateLocked, "ProviderSubmission is locked")]
        public async Task InvalidRequests(string json, string expected)
        {
          //Arrange
          ServiceUserSubmissionResponse response = new();
          
          response.SetStatus(Business.Enums.StatusType.Invalid, expected);

          ServiceUserSubmissionRequest[] requests = JsonConvert
            .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

          IEnumerable<ServiceUserSubmissionResponse> responses =
            new List<ServiceUserSubmissionResponse> { response };

          _mockService.Setup(x => x.ProviderSubmissionsAsync(It.
          IsAny<IEnumerable<ServiceUserSubmissionRequest>>()))
          .Returns(Task.FromResult(responses));

          _classToTest = new ServiceUserController(_mockService.Object,
              _mockMapper.Object);

          //Act
          var result = await _classToTest.Put(requests);

          // assert
          Assert.NotNull(result);
          Assert.IsType<ObjectResult>(result);
          var okResult = result as ObjectResult;
          var problem = okResult.Value as ValidationProblemDetails;
          Assert.Equal(400, problem.Status);
          foreach (KeyValuePair<string, string[]> kvp in problem.Errors)
          {
            Assert.Equal(expected, kvp.Value[0]);
          }
        }

        [Fact]
        public async Task ServiceThrowsInternalServerError()
        {
          //Arrange
          string expected = string.Empty;
          ServiceUserSubmissionRequest[] requests = null;

          _mockService.Setup(x => x.ProviderSubmissionsAsync(It.
          IsAny<IEnumerable<ServiceUserSubmissionRequest>>()))
          .Throws(new ArgumentNullException(nameof(requests)));

          var classToTest = new Mock<ServiceUserController>(_mockService.Object,
              _mockMapper.Object)
          { CallBase = true };


          //Act
          var result = await classToTest.Object.Put(requests);

          //Assert
          Assert.NotNull(result);
          Assert.IsType<ObjectResult>(result);
          var okResult = result as ObjectResult;
          var problem = okResult.Value as ProblemDetails;
          Assert.Equal(500, problem.Status);
        }
      }


    }

    public class GetTests : ServiceUserControllerTests
    {
      public GetTests()
      {
        _mockService = new Mock<ProviderService>(
          _mockContext.Object,
          _mockMapper.Object,
          TestConfiguration.CreateProviderOptions());
      }

      [Fact]
      public async Task Problem_Returned_Date_In_Future()
      {
        //arrange
        DateTimeOffset from = DateTimeOffset.Now.AddMinutes(10);
        int expected = 400;
        string error = $"from date cannot be in the future {from}";
        _classToTest = new ServiceUserController(_mockService.Object,
          _mockMapper.Object);
        //act

        var response = await _classToTest.Get(from);
        //assert
        response.Should().NotBeNull();
        response.Result.Should().BeOfType<ObjectResult>();

        var result = response.Result as ObjectResult;
        var detail = result.Value as ProblemDetails;
        detail.Detail.Should().Be(error);
        detail.Status.Should().Be(expected);
      }

      [Fact]
      public async Task Valid_Returns_ServiceUserResponse_200()
      {
        //arrange
        var mockServiceUser = new Mock<ServiceUser>();
        IEnumerable<ServiceUser> serviceUsers = new[] {mockServiceUser.Object};
        _mockService.Setup(t => t.GetServiceUsers())
          .ReturnsAsync(serviceUsers);

        _classToTest = new ServiceUserController(
          _mockService.Object,
          new MapperConfiguration(cfg => cfg
            .AddMaps(new[] { "WmsHub.Provider.Api" }))
              .CreateMapper());
        //act
        var response = await _classToTest.Get(null);
        //assert
        response.Should().NotBeNull();
        response.Result.Should().BeOfType<OkObjectResult>();

        var result = response.Result as OkObjectResult;
        var list = result.Value as IEnumerable<ServiceUserResponse>;
      }

      [Fact]
      public async Task Valid_Returns_ServiceUserResponse_200_AllBoolsAreNull()
      {
        //arrange
        var mockServiceUser = new Mock<ServiceUser>();
        IEnumerable<ServiceUser> serviceUsers = new[] { mockServiceUser.Object };
        _mockService.Setup(t => t.GetServiceUsers())
          .ReturnsAsync(serviceUsers);
        _classToTest = new ServiceUserController(
          _mockService.Object,
          new MapperConfiguration(cfg => cfg
            .AddMaps(new[] { "WmsHub.Provider.Api" }))
              .CreateMapper());
        //act
        var response = await _classToTest.Get(null);
        //assert
        response.Should().NotBeNull();
        response.Result.Should().BeOfType<OkObjectResult>();

        var result = response.Result as OkObjectResult;
        var list = result.Value as IEnumerable<ServiceUserResponse>;
        foreach (var item in list)
        {
          item.HasDiabetesType1.Should().BeNull();
          item.HasDiabetesType2.Should().BeNull();
          item.HasHypertension.Should().BeNull();
          item.HasLearningDisability.Should().BeNull();
          item.HasPhysicalDisability.Should().BeNull();
          item.HasRegisteredSeriousMentalIllness.Should().BeNull();
          item.IsVulnerable.Should().BeNull();
        }
      }
    }
  }
}
