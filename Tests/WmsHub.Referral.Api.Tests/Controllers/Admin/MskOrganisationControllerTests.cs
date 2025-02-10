using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Referral.Api.Controllers.Admin;
using WmsHub.Referral.Api.Models.Admin;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Referral.Api.Tests.Controllers.Admin;
public class MskOrganisationControllerTests : IDisposable
{
  protected Mock<MskOrganisationService> _service;
  protected ILogger _silentLogger;
  protected const string ODS_CODE = "ABCDE";

  public MskOrganisationControllerTests(ITestOutputHelper testOutputHelper)
  {
    Mock<DatabaseContext> mockDbContext = new();
    _service = new(mockDbContext.Object);
    _silentLogger = new LoggerConfiguration().CreateLogger();
  }

  public void Dispose()
  {
    _service.Reset();
    GC.SuppressFinalize(this);
  }

  public class DeleteTests : MskOrganisationControllerTests
  {
    public DeleteTests(ITestOutputHelper testOutputHelper)
      : base(testOutputHelper)
    { }

    [Fact]
    public async Task Valid_Returns200Response()
    {
      // Arrange.
      string expectedResponse = $"Referral with ODS code ${ODS_CODE} deleted.";
      _service.Setup(x => x.DeleteAsync(ODS_CODE))
        .ReturnsAsync(expectedResponse);
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Delete(ODS_CODE);

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      using (new AssertionScope())
      {
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().Be(expectedResponse);
      }
    }

    [Fact]
    public async Task Invalid_Returns404Response()
    {
      // Arrange.
      string expectedExceptionMessage =
        $"An organisation with the ODS code {ODS_CODE} does not exist.";
      _service.Setup(x => x.DeleteAsync(ODS_CODE)).ThrowsAsync(
        new MskOrganisationNotFoundException(expectedExceptionMessage));
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Delete(ODS_CODE);

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      ProblemDetails details = (ProblemDetails)objectResult.Value;
      using (new AssertionScope())
      {
        objectResult.StatusCode.Should().Be(404);
        details.Detail.Should().Be(expectedExceptionMessage);
      }
    }

    [Fact]
    public async Task Error_Returns500Response()
    {
      // Arrange.
      _service.Setup(x => x.DeleteAsync(ODS_CODE)).ThrowsAsync(
        new Exception());
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Delete(ODS_CODE);

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      objectResult.StatusCode.Should().Be(500);
    }
  }

  public class GetTests : MskOrganisationControllerTests
  {
    public GetTests(ITestOutputHelper testOutputHelper)
      : base(testOutputHelper)
    { }

    [Fact]
    public async Task Valid_Returns200Response()
    {
      // Arrange.
      List<MskOrganisation> mskOrganisations = new()
      {
        new MskOrganisation()
        {
          OdsCode = ODS_CODE,
          SendDischargeLetters = true
        }
      };
      _service.Setup(x => x.GetAsync())
        .ReturnsAsync(mskOrganisations);
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Get();

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      using (new AssertionScope())
      {
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().BeEquivalentTo(mskOrganisations.ToArray());
      }
    }

    [Fact]
    public async Task Error_Returns500Response()
    {
      // Arrange.
      _service.Setup(x => x.GetAsync()).ThrowsAsync(
        new Exception());
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Get();

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      objectResult.StatusCode.Should().Be(500);
    }
  }

  public class PostTests : MskOrganisationControllerTests
  {
    public PostTests(ITestOutputHelper testOutputHelper)
      : base(testOutputHelper)
    { }

    [Fact]
    public async Task Valid_Returns200Response()
    {
      // Arrange.
      MskOrganisationPostRequest request = new()
      {
        OdsCode = ODS_CODE,
        SendDischargeLetters = true
      };
      MskOrganisation organisation = new()
      {
        OdsCode = ODS_CODE,
        SendDischargeLetters = true
      };
      _service.Setup(x => x.AddAsync(It.IsAny<MskOrganisation>()))
        .ReturnsAsync(organisation);
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Post(request);

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      using (new AssertionScope())
      {
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().BeEquivalentTo(organisation);
      }
    }

    [Fact]
    public async Task Invalid_Returns400Response()
    {
      // Arrange.
      MskOrganisationPostRequest request = new()
      {
        OdsCode = ODS_CODE,
        SendDischargeLetters = true
      };
      MskOrganisation organisation = new()
      {
        OdsCode = ODS_CODE,
        SendDischargeLetters = true
      };
      string expectedErrorMessage = $"An organisation with " +
        $"the ODS code {ODS_CODE} already exists.";
      _service.Setup(x => x.AddAsync(It.IsAny<MskOrganisation>()))
        .ThrowsAsync(new InvalidOperationException(expectedErrorMessage));
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Post(request);

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      ProblemDetails details = (ProblemDetails)objectResult.Value;
      using (new AssertionScope())
      {
        objectResult.StatusCode.Should().Be(400);
        details.Detail.Should().Be(expectedErrorMessage);
      }
    }

    [Fact]
    public async Task Error_Returns500Response()
    {
      // Arrange.
      _service.Setup(x => x.AddAsync(It.IsAny<MskOrganisation>()))
        .ThrowsAsync(
          new Exception());
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Post(
        It.IsAny<MskOrganisationPostRequest>());

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      objectResult.StatusCode.Should().Be(500);
    }
  }

  public class PutTests : MskOrganisationControllerTests
  {
    public PutTests(ITestOutputHelper testOutputHelper)
      : base(testOutputHelper)
    { }

    [Fact]
    public async Task Valid_Returns200Response()
    {
      // Arrange.
      MskOrganisationPutRequest request = new()
      {
        SendDischargeLetters = false
      };
      MskOrganisation organisation = new()
      {
        OdsCode = ODS_CODE,
        SendDischargeLetters = false
      };
      _service.Setup(x => x.UpdateAsync(It.IsAny<MskOrganisation>()))
        .ReturnsAsync(organisation);
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Put(ODS_CODE, request);

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      using (new AssertionScope())
      {
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().BeEquivalentTo(organisation);
      }
    }

    [Fact]
    public async Task Invalid_Returns404Response()
    {
      // Arrange.
      MskOrganisationPutRequest request = new()
      {
        SendDischargeLetters = false
      };
      string expectedErrorMessage = $"An organisation with the ODS code " +
        $"{ODS_CODE} does not exist.";
      _service.Setup(x => x.UpdateAsync(It.IsAny<MskOrganisation>()))
        .ThrowsAsync(
          new MskOrganisationNotFoundException(expectedErrorMessage));
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Put(ODS_CODE, request);

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      ProblemDetails details = (ProblemDetails)objectResult.Value;
      using (new AssertionScope())
      {
        objectResult.StatusCode.Should().Be(404);
        details.Detail.Should().Be(expectedErrorMessage);
      }
    }

    [Fact]
    public async Task NullOdsCode_Returns400Response()
    {
      // Arrange.
      ArgumentNullException exception = new("odsCode");
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Put(
        null,
        It.IsAny<MskOrganisationPutRequest>());

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      using (new AssertionScope())
      {
        objectResult.StatusCode.Should().Be(400);
        objectResult.Value.Should().BeEquivalentTo(exception);
      }
    }

    [Fact]
    public async Task Error_Returns500Response()
    {
      // Arrange.
      _service.Setup(x => x.UpdateAsync(It.IsAny<MskOrganisation>()))
        .ThrowsAsync(new Exception());
      MskOrganisationController controller = new(_service.Object, _silentLogger);

      // Act.
      IActionResult result = await controller.Put(
        ODS_CODE,
        It.IsAny<MskOrganisationPutRequest>());

      // Assert.
      ObjectResult objectResult = result as ObjectResult;
      objectResult.StatusCode.Should().Be(500);
    }
  }
}
