using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Polly.Caching;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Extensions;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Referral.Api.Controllers.Admin;
using WmsHub.Referral.Api.Models;
using WmsHub.Referral.Api.Models.Admin;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Referral.Api.Tests.Controllers.Admin;
public class OrganisationControllerTests : IDisposable
{  
  protected Mock<IDateTimeProvider> _mockDateTimeProvider;
  protected ILogger _silentLogger = new LoggerConfiguration().CreateLogger();
  protected OrganisationController _organisationController;
  protected Mock<OrganisationService> _service;

  public OrganisationControllerTests(ITestOutputHelper testOutputHelper)
  {
    Mock<DatabaseContext> mockDbContext = new();
    _service = new(mockDbContext.Object);
    _mockDateTimeProvider = new();
    _organisationController = new(
      _mockDateTimeProvider.Object,
      _silentLogger, 
      _service.Object);
  }

  public void Dispose()
  {
    _service.Reset();
    GC.SuppressFinalize(this);
  }

  public class GetTests : OrganisationControllerTests
  {
    public GetTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    [Fact]
    public async Task SuccessfulGet_Returns200()
    {
      // Arrange.
      IEnumerable<Organisation> organisations = new List<Organisation>();

      _service.Setup(service => service.GetAsync())
        .ReturnsAsync(organisations)
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.Get();

      // Assert.
      _service.Verify();
      result.Should().BeOfType<OkObjectResult>()
        .Subject.Value.Should().Be(organisations);
    }

    [Fact]
    public async Task FailedGet_Returns400()
    {
      // Arrange.
      string exceptionMessage = "Exception message";

      _service.Setup(service => service.GetAsync())
        .ThrowsAsync(new InvalidOperationException(exceptionMessage))
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.Get();

      // Assert.
      _service.Verify();

      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task GetError_Returns500()
    {
      // Arrange.
      string exceptionMessage = "Exception message";

      _service.Setup(service => service.GetAsync())
        .ThrowsAsync(new Exception(exceptionMessage))
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.Get();

      // Assert.
      _service.Verify();

      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }
  }

  public class PostTests : OrganisationControllerTests
  {
    public PostTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    [Fact]
    public async Task SuccessfulAdd_Returns200()
    {
      // Arrange.
      OrganisationPostRequest postRequest = new()
      {
        OdsCode = "ABC",
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      Organisation savedOrganisation = new()
      {
        OdsCode = "ABC",
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      _service.Setup(service => service.AddAsync(
          It.Is<Organisation>(org => IsEquivalent(org, savedOrganisation))))
        .ReturnsAsync(savedOrganisation)
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.Post(postRequest);

      // Assert.
      _service.Verify();

      result.Should().BeOfType<OkObjectResult>()
        .Subject.Value.Should().Be(savedOrganisation);
    }

    [Fact]
    public async Task FailedAdd_Returns400()
    {
      // Arrange.
      string exceptionMessage = "Exception message";

      OrganisationPostRequest postRequest = new()
      {
        OdsCode = "ABC",
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      Organisation expectedParameter = new()
      {
        OdsCode = "ABC",
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      _service.Setup(service => service.AddAsync(
          It.Is<Organisation>(org => IsEquivalent(org, expectedParameter))))
        .ThrowsAsync(new InvalidOperationException(exceptionMessage))
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.Post(postRequest);

      // Assert.
      _service.Verify();

      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task AddError_Returns500()
    {
      // Arrange.
      string exceptionMessage = "Exception message";

      OrganisationPostRequest postRequest = new()
      {
        OdsCode = "ABC",
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      Organisation expectedParameter = new()
      {
        OdsCode = "ABC",
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      _service.Setup(service => service.AddAsync(
          It.Is<Organisation>(org => IsEquivalent(org, expectedParameter))))
        .ThrowsAsync(new Exception(exceptionMessage))
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.Post(postRequest);

      // Assert.
      _service.Verify();

      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }
  }

  public class PutTests : OrganisationControllerTests
  {
    private const string ODS_CODE = "ABC";
    public PutTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    [Fact]
    public async Task OdsCodeOnly_SuccessfulDelete_Returns200()
    {
      // Arrange.
      _service.Setup(service => service.DeleteAsync(ODS_CODE)).Verifiable();

      // Act.
      IActionResult result = await _organisationController.Put(ODS_CODE);

      // Assert.
      _service.Verify();
      result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task OdsCodeOnly_FailedDelete_Returns400()
    {
      // Arrange.
      string exceptionMessage = "Exception message";

      _service.Setup(service => service.DeleteAsync(ODS_CODE))
        .ThrowsAsync(new InvalidOperationException(exceptionMessage))
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.Put(ODS_CODE);

      // Assert.
      _service.Verify();

      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task OdsCodeOnly_DeleteError_Returns500()
    {
      // Arrange.
      string exceptionMessage = "Exception message";

      _service.Setup(service => service.DeleteAsync(ODS_CODE))
        .ThrowsAsync(new Exception(exceptionMessage))
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.Put(ODS_CODE);

      // Assert.
      _service.Verify();

      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task OdsCodeAndOrganisationPutRequest_SuccessfulUpdate_Returns200()
    {
      // Arrange.
      Organisation expectedResult = new()
      {
        OdsCode = ODS_CODE,
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      _service.Setup(service => service.UpdateAsync(
          It.Is<Organisation>(org => IsEquivalent(org, expectedResult))))
        .ReturnsAsync(expectedResult)
        .Verifiable();

      OrganisationPutRequest putRequest = new()
      {
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      // Act.
      IActionResult result = await _organisationController.Put(ODS_CODE, putRequest);

      // Assert.
      _service.Verify();

      result.Should().BeOfType<OkObjectResult>()
        .Subject.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task OdsCodeAndOrganisationPutRequest_FailedUpdate_Returns400()
    {
      // Arrange.
      string exceptionMessage = "Exception message";

      Organisation expectedParameter = new()
      {
        OdsCode = ODS_CODE,
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      _service.Setup(service => service.UpdateAsync(
          It.Is<Organisation>(org => IsEquivalent(org, expectedParameter))))
        .ThrowsAsync(new InvalidOperationException(exceptionMessage))
        .Verifiable();

      OrganisationPutRequest putRequest = new()
      {
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      // Act.
      IActionResult result = await _organisationController.Put(ODS_CODE, putRequest);
        
      // Assert.
      _service.Verify();

      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task OdsCodeAndOrganisationPutRequest_UpdateError_Returns500()
    {
      // Arrange.
      string exceptionMessage = "Exception message";

      Organisation expectedParameter = new()
      {
        OdsCode = ODS_CODE,
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      _service.Setup(service => service.UpdateAsync(
          It.Is<Organisation>(org => IsEquivalent(org, expectedParameter))))
        .ThrowsAsync(new Exception(exceptionMessage))
        .Verifiable();

      OrganisationPutRequest putRequest = new()
      {
        QuotaRemaining = 100,
        QuotaTotal = 100
      };

      // Act.
      IActionResult result = await _organisationController.Put(ODS_CODE, putRequest);

      // Assert.
      _service.Verify();

      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }
  }

  public class ResetOrganisationQuotasTests : OrganisationControllerTests
  {
    public ResetOrganisationQuotasTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    [Fact]
    public async Task ValidDate_Returns200()
    {
      // Arrange.
      _mockDateTimeProvider.Setup(d => d.Now)
        .Returns(new DateTime(2023, 1, 1));

      _service.Setup(service => service.ResetOrganisationQuotas())
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.ResetOrganisationQuotas();

      // Assert.
      _service.Verify();
      result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task InvalidDateWithOverride_Returns200()
    {
      // Arrange.
      _mockDateTimeProvider.Setup(d => d.Now)
        .Returns(new DateTime(2023, 1, 2));

      _service.Setup(service => service.ResetOrganisationQuotas())
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.ResetOrganisationQuotas(true);

      // Assert.
      _service.Verify();
      result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task InvalidDateNoOverride_Returns422()
    {
      // Arrange.
      _mockDateTimeProvider.Setup(d => d.Now)
        .Returns(new DateTime(2023, 1, 2));
      string expectedExceptionMessage = "Error only able to reset organisation quotas on 1st of " +
        "month.";

      _service.Setup(service => service.ResetOrganisationQuotas());

      // Act.
      IActionResult result = await _organisationController.ResetOrganisationQuotas();

      // Assert.
      _service.Verify(x => x.ResetOrganisationQuotas(), Times.Never);

      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public async Task ResetError_Returns500()
    {
      // Arrange.
      string exceptionMessage = "Exception message";
      _service.Setup(service => service.ResetOrganisationQuotas())
        .ThrowsAsync(new Exception(exceptionMessage))
        .Verifiable();

      // Act.
      IActionResult result = await _organisationController.ResetOrganisationQuotas();

      // Assert.
      _service.Verify();

      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }
  }

  protected bool IsEquivalent(Organisation organisation1, Organisation organisation2)
  {
    return organisation1.OdsCode == organisation2.OdsCode
      && organisation1.QuotaRemaining == organisation2.QuotaRemaining
      && organisation1.QuotaTotal == organisation2.QuotaTotal;
  }
}
