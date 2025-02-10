using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Common.Apis.Ods;
using WmsHub.Common.Apis.Ods.Models;
using WmsHub.Common.Exceptions;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Common.Tests.Apis.Ods;

public class OdsOrganisationTests : ATheoryData
{
  protected OdsOrganisationService _organisationService;

  public class GetOdsCodeUnitTests : OdsOrganisationTests
  {
    private readonly Mock<ILogger> _logger = new();

    public GetOdsCodeUnitTests()
    {
      _logger.Setup(x => x
        .ForContext<object>())
        .Returns(_logger.Object);

      _organisationService = new(
        _logger.Object,
        Options.Create(new OdsOrganisationOptions()
        {
          // unit tests are mocking the API so this is not required
          Endpoint = "https://notused.com",
        }));
    }

    [Theory]
    [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
    public async Task OdsCode_NullOrWhiteSpace_Exception(string odsCode)
    {
      // Arrange.
      // Act.
      Exception ex = await Record.ExceptionAsync(() =>
        _organisationService.GetOdsOrganisationAsync(odsCode));

      // Assert.
      ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>();
    }

    [Theory]
    [InlineData("V81997")]
    [InlineData("V81998")]
    [InlineData("V81999")]
    public async Task UnknownOdsCodes_WasFoundFalse(string odsCode)
    {
      // Arrange.
      // Act.
      OdsOrganisation odsOrganisation =
        await _organisationService.GetOdsOrganisationAsync(odsCode);

      // Assert.
      odsOrganisation.WasFound.Should().BeFalse();
      odsOrganisation.Organisation.Should().BeNull();
    }

    [Fact]
    public async Task OdsApi_200_WasFoundTrue()
    {
      // Arrange.
      string name = "Test Practice";
      string odsCode = "XXX11";

      _organisationService.HttpMessageHandler =
        GetMockMessageHandler(
          HttpStatusCode.OK,
          JsonSerializer.Serialize(new OdsOrganisation(odsCode, name)))
        .Object;

      // Act.
      OdsOrganisation odsOrganisation =
        await _organisationService.GetOdsOrganisationAsync(odsCode);

      // Assert.
      odsOrganisation.WasFound.Should().BeTrue();
      odsOrganisation.Organisation.Should().NotBeNull();
      odsOrganisation.Organisation.Name.Should().Be(name);
      odsOrganisation.Organisation.OrgId.Should().NotBeNull();
      odsOrganisation.Organisation.OrgId.Extension.Should().Be(odsCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]    
    public async Task OdsApi_Not200_WasFoundFalse(HttpStatusCode httpStatusCode)
    {
      // Arrange.
      string odsCode = "XXX11";

      _organisationService.HttpMessageHandler = GetMockMessageHandler(
        httpStatusCode,
        string.Empty)
        .Object;

      // Act.
      OdsOrganisation odsOrganisation = await _organisationService
        .GetOdsOrganisationAsync(odsCode);

      // Assert.
      odsOrganisation.WasFound.Should().BeFalse();
      odsOrganisation.Organisation.Should().BeNull();
    }

    [Fact]
    public async Task OdsApi_Exception_WasFoundFalse()
    {
      // Arrange.
      string odsCode = "XXX11";

      _organisationService.HttpMessageHandler =
        GetMockMessageHandlerException().Object;

      // Act.
      OdsOrganisation odsOrganisation = await _organisationService
        .GetOdsOrganisationAsync(odsCode);

      // Assert.
      odsOrganisation.WasFound.Should().BeFalse();
      odsOrganisation.Organisation.Should().BeNull();
    }

    private static Mock<HttpMessageHandler> GetMockMessageHandler(
      HttpStatusCode statusCode,
      string content)
    {
      Mock<HttpMessageHandler> handlerMock = new();
      handlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage()
        {
          StatusCode = statusCode,
          Content = new StringContent(content)
        })
        .Verifiable();

      return handlerMock;
    }

    private static Mock<HttpMessageHandler> GetMockMessageHandlerException()
    {
      Mock<HttpMessageHandler> handlerMock = new();

      handlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>()
        )
        .ThrowsAsync(new TimeoutException())
        .Verifiable();

      return handlerMock;
    }
  }

  public class GetOdsCodeIntegrationTests : OdsOrganisationTests
  {
    private readonly Mock<ILogger> _logger = new();

    public GetOdsCodeIntegrationTests()
    {
      _logger.Setup(x => x
        .ForContext<object>())
        .Returns(_logger.Object);

      _organisationService = new(
        _logger.Object,
        Options.Create(new OdsOrganisationOptions()
        {
          Endpoint = "https://uat.directory.spineservices.nhs.uk/" +
            "ORD/2-0-0/organisations/"
        }));
    }

    [Fact]
    public async Task OdsCode_Valid_WasFoundTrue()
    {
      // Arrange.
      string odsCode = "RY448";

      // Act.
      var odsOrganisation = await _organisationService
        .GetOdsOrganisationAsync(odsCode);

      // Assert.
      odsOrganisation.WasFound.Should().BeTrue();
      odsOrganisation.Organisation.Should().NotBeNull();
      odsOrganisation.Organisation.OrgId.Should().NotBeNull();
      odsOrganisation.Organisation.OrgId.Extension.Should().Be(odsCode);
    }

    [Fact]
    public async Task OdsCode_Invalid_WasFoundFalse()
    {
      // Arrange.
      string odsCode = "XXX";

      // Act.
      var odsOrganisation = await _organisationService
        .GetOdsOrganisationAsync(odsCode);

      // Assert.
      odsOrganisation.WasFound.Should().BeFalse();
      odsOrganisation.Organisation.Should().BeNull();
    }
  }
}
