using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralStatusReason;
using WmsHub.Business.Services;
using WmsHub.Provider.Api.Controllers;
using WmsHub.Provider.Api.Models;
using WmsHub.ProviderApi.Tests;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Provider.Api.Tests;

public class ServiceUserControllerTests : TestSetup
{
  private ServiceUserController _serviceUserController;

  public ServiceUserControllerTests()
  {
    _serviceUserController = new ServiceUserController(
      _mockProviderService.Object,
      new MapperConfiguration(cfg => cfg
        .AddMaps(new[] { "WmsHub.Provider.Api" }))
        .CreateMapper());
  }

  public class PutTests : ServiceUserControllerTests
  {
    public PutTests()
    {

      _mockProviderService = new Mock<ProviderService>(
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
        // Arrange
        ServiceUserSubmissionRequest[] requests = JsonConvert
          .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

        IEnumerable<ServiceUserSubmissionResponse> responses =
          new List<ServiceUserSubmissionResponse> {
            new ServiceUserSubmissionResponse
            {
              ResponseStatus = StatusType.Valid
            }
          };

        _mockProviderService.Setup(x => x.ProviderSubmissionsAsync(It.
          IsAny<IEnumerable<ServiceUserSubmissionRequest>>()))
          .Returns(Task.FromResult(responses));

        _serviceUserController = new ServiceUserController(_mockProviderService.Object,
            _mockMapper.Object);

        // Act
        var result = await _serviceUserController.PutV2(requests);

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
        // Arrange
        ServiceUserSubmissionResponse response = new();

        response.SetStatus(StatusType.Invalid, expected);

        ServiceUserSubmissionRequest[] requests = JsonConvert
          .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

        IEnumerable<ServiceUserSubmissionResponse> responses =
          new List<ServiceUserSubmissionResponse> { response };

        _mockProviderService.Setup(x => x.ProviderSubmissionsAsync(It.
        IsAny<IEnumerable<ServiceUserSubmissionRequest>>()))
        .Returns(Task.FromResult(responses));

        _serviceUserController = new ServiceUserController(_mockProviderService.Object,
            _mockMapper.Object);

        // Act
        var result = await _serviceUserController.PutV2(requests);

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
        // Arrange
        string expected = string.Empty;
        ServiceUserSubmissionRequest[] requests = null;

        _mockProviderService.Setup(x => x.ProviderSubmissionsAsync(It.
        IsAny<IEnumerable<ServiceUserSubmissionRequest>>()))
        .Throws(new ArgumentNullException(nameof(requests)));

        var classToTest = new Mock<ServiceUserController>(_mockProviderService.Object,
            _mockMapper.Object)
        { CallBase = true };


        // Act
        var result = await classToTest.Object.PutV2(requests);

        // Assert
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
    //[Fact]
    //public async Task V1_FromParameterInFuture_400()
    //{
    //  // Arrange.
    //  DateTimeOffset from = DateTimeOffset.Now.AddMinutes(10);
    //  ProblemDetails expectedProblemDetails = new()
    //  {
    //    Detail = $"from date cannot be in the future {from}",
    //    Status = 400
    //  };

    //  // Act.
    //  ActionResult<IEnumerable<ServiceUserResponse>> response =
    //    await _serviceUserController.Get(from);

    //  // Assert.
    //  response.Result.Should().BeOfType<ObjectResult>()
    //    .Which.Value.Should().BeOfType<ProblemDetails>()
    //    .Which.Should().BeEquivalentTo(expectedProblemDetails);
    //}
    
    //[Fact]
    //public async Task V1_FromParameterInPast_200()
    //{
    //  // Arrange.
    //  DateTimeOffset from = DateTimeOffset.Now.AddMinutes(-10);
    //  string expectedUbrn = "GP0000000001";

    //  _mockProviderService
    //    .Setup(t => t.GetServiceUsers())
    //    .ReturnsAsync(new List<ServiceUser>
    //    {
    //      new() { ProviderUbrn = expectedUbrn }
    //    });

    //  // Act.
    //  ActionResult<IEnumerable<ServiceUserResponse>> response =
    //    await _serviceUserController.Get(from);

    //  // Assert.
    //  using (new AssertionScope())
    //  {
    //    response.Result.Should().BeOfType<OkObjectResult>()
    //      .Which.Value.Should().BeOfType<List<ServiceUserResponse>>()
    //      .Which.Should().HaveCount(1)
    //      .And.Subject.Single().Ubrn.Should().Be(expectedUbrn);

    //    _mockProviderService.Verify(x => x.GetServiceUsers(), Times.Once);
    //  }
    //}

    [Fact]
    public async Task V2_200()
    {
      // Arrange.
      string expectedUbrn = "GP0000000001";

      _mockProviderService
        .Setup(t => t.GetServiceUsers())
        .ReturnsAsync(new List<ServiceUser>
        {
          new() { ProviderUbrn = expectedUbrn }
        });

      // Act.
      ActionResult<IEnumerable<ServiceUserResponse>> response =
        await _serviceUserController.Get();

      // Assert.
      using (new AssertionScope())
      {
        response.Result.Should().BeOfType<OkObjectResult>()
        .Which.Value.Should().BeOfType<List<ServiceUserResponse>>()
        .Which.Should().HaveCount(1)
        .And.Subject.Single().Ubrn.Should().Be(expectedUbrn);

        _mockProviderService.Verify(x => x.GetServiceUsers(), Times.Once);
      }
    }
  }

  public class GetReferralStatusReasonsTests : ServiceUserControllerTests
  {

    private static ReferralStatusReason[] _referralStatusReasons =
    {
      new ReferralStatusReason
      {
        Description = "Provider Declined Description",
        Groups = ReferralStatusReasonGroup.ProviderDeclined,
        Id = Guid.Parse("410f4dab-8864-4701-b139-8172e903d6dd"),
      },
      new ReferralStatusReason
      {
        Description = "Provider Rejected Description",
        Groups = ReferralStatusReasonGroup.ProviderRejected,
        Id = Guid.Parse("7dc4bb95-34e7-4adf-9277-83ddcfaf3433"),
      },
      new ReferralStatusReason
      {
        Description = "Provider Terminated Description",
        Groups = ReferralStatusReasonGroup.ProviderTerminated,
        Id = Guid.Parse("6e2b57fd-b4f7-4472-8d51-b284a7d62b20"),
      },
      new ReferralStatusReason
      {
        Description = "All Groups Description",
        Groups = ReferralStatusReasonGroup.ProviderDeclined
          | ReferralStatusReasonGroup.ProviderRejected
          | ReferralStatusReasonGroup.ProviderTerminated
          | ReferralStatusReasonGroup.RmcRejected,
        Id = Guid.Parse("93ade43e-2674-4e41-99b0-88a1149e4910"),
      }
    };

    [Fact]
    public async Task OnlyProviderGroupsReturned_200()
    {
      // Arrange.
      string expectedDescription = "All Groups Description";
      string expectedGroups =
        "ProviderRejected, ProviderDeclined, ProviderTerminated";
      Guid expectedId = Guid.Parse("93ade43e-2674-4e41-99b0-88a1149e4910");

      ReferralStatusReason referralStatusReason = new()
      {
        Description = expectedDescription,
        Groups = ReferralStatusReasonGroup.ProviderDeclined
            | ReferralStatusReasonGroup.ProviderRejected
            | ReferralStatusReasonGroup.ProviderTerminated
            | ReferralStatusReasonGroup.RmcRejected,
        Id = expectedId
      };

      _mockProviderService
        .Setup(x => x.GetReferralStatusReasonsAsync())
        .ReturnsAsync(new ReferralStatusReason[] { referralStatusReason });

      // Act.
      ActionResult<IEnumerable<GetReferralStatusReasonsResponse>> result =
        await _serviceUserController.GetReferralStatusReasons(null);

      // Assert.
      using (new AssertionScope())
      {
        GetReferralStatusReasonsResponse response = result
        .Result.Should().BeOfType<OkObjectResult>()
        .Which.Value.Should()
          .BeOfType<List<GetReferralStatusReasonsResponse>>()
        .Which.Should().HaveCount(1)
        .And.Subject.Single();

        response.Description.Should().Be(expectedDescription);
        response.Groups.Should().Be(expectedGroups,
          because: "GetReferralStatusReasonsResponse should remove RmcRejected");
        response.Id.Should().Be(expectedId);
      }
    }

    [Theory]
    [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
    public async Task GetReferralStatusReasonsAsyncCalledWithoutGroupFilter(
      string group)
    {
      // Arrange
      _mockProviderService
        .Setup(x => x.GetReferralStatusReasonsAsync())
        .ReturnsAsync(Array.Empty<ReferralStatusReason>);

      // Act
      ActionResult<IEnumerable<GetReferralStatusReasonsResponse>> response =
        await _serviceUserController.GetReferralStatusReasons(group);

      // Assert
      _mockProviderService.Verify(x =>
        x.GetReferralStatusReasonsAsync(), Times.Once);
    }


    [Theory]
    [InlineData("RmcRejected")]
    [InlineData("4")]
    [InlineData("Unknown")]
    public async Task UnknownGroup_400(string group)
    {
      // Arrange.
      ProblemDetails expectedProblemDetails = new()
      {
        Detail = $"Unknown referral status reason group: '{group}'.",
        Status = 400
      };

      // Act.
      ActionResult<IEnumerable<GetReferralStatusReasonsResponse>> response =
        await _serviceUserController.GetReferralStatusReasons(group.ToString());

      // Assert.
      response.Result.Should().BeOfType<ObjectResult>()
        .Which.Value.Should().BeOfType<ProblemDetails>()
        .Which.Should().BeEquivalentTo(expectedProblemDetails);
    }
  }
}
