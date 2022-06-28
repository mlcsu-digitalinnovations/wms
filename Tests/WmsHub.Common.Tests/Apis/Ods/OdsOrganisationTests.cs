using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Common.Apis.Ods;
using WmsHub.Common.Apis.Ods.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Common.Tests.Apis.Ods
{
  public class OdsOrganisationTests : ATheoryData
  {
    protected OdsOrganisationService _organisation;

    public class GetOdsCodeUnitTests : OdsOrganisationTests
    {
      public GetOdsCodeUnitTests()
      {
        _organisation = new(Options.Create(new OdsOrganisationOptions()
        {
          // unit tests are mocking the API so this is not required
          Endpoint = "https://notused.com"
        }));
      }

      [Theory]
      [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
      public async void OdsCode_NullOrWhiteSpace_Exception(string odsCode)
      {
        // arrange
        // act
        var ex = await Record.ExceptionAsync(() =>
          _organisation.GetOdsOrganisationAsync(odsCode));

        // assert
        ex.Should().BeOfType<ArgumentException>();
      }

      [Fact]
      public async void OdsApi_200_WasFoundTrue()
      {
        // arrange
        string odsCode = "XXX11";

        _organisation.HttpMessageHandler = GetMockMessageHandler(
          HttpStatusCode.OK,
          JsonSerializer.Serialize(new OdsOrganisation(odsCode)))
          .Object;

        // act
        OdsOrganisation response =
          await _organisation.GetOdsOrganisationAsync(odsCode);

        // assert
        response.WasFound.Should().BeTrue();
        response.Organisation.Should().NotBeNull();
        response.Organisation.OrgId.Should().NotBeNull();
        response.Organisation.OrgId.Extension.Should().Be(odsCode);
      }

      [Fact]
      public async void OdsApi_404_WasFoundFalse()
      {
        // arrange
        _organisation.HttpMessageHandler = GetMockMessageHandler(
          HttpStatusCode.NotFound,
          string.Empty)
          .Object;

        // act
        var odsOrganisation = await _organisation.GetOdsOrganisationAsync("XXX11");

        // assert
        odsOrganisation.WasFound.Should().BeFalse();
        odsOrganisation.Organisation.Should().BeNull();
      }

      [Fact]
      public async void OdsApi_500_Exception()
      {
        // arrange
        _organisation.HttpMessageHandler = GetMockMessageHandler(
          HttpStatusCode.InternalServerError,
          string.Empty)
          .Object;

        // act
        var ex = await Record.ExceptionAsync(() =>
          _organisation.GetOdsOrganisationAsync("XXX11"));

        // assert
        ex.Should().BeOfType<HttpRequestException>();
      }

      private static Mock<HttpMessageHandler> GetMockMessageHandler(
        HttpStatusCode statusCode,
        string content)
      {
        var handlerMock = new Mock<HttpMessageHandler>();
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
    }

    public class GetOdsCodeIntegrationTests : OdsOrganisationTests
    {
      public GetOdsCodeIntegrationTests()
      {
        _organisation = new(Options.Create(new OdsOrganisationOptions()
        {
          Endpoint = "https://uat.directory.spineservices.nhs.uk/" +
            "ORD/2-0-0/organisations/"
        }));
      }

      [Fact]
      public async void OdsCode_Valid_WasFoundTrue()
      {
        // arrange
        string odsCode = "RY448";

        // act
        var odsOrganisation = await _organisation.GetOdsOrganisationAsync(odsCode);

        // assert
        odsOrganisation.WasFound.Should().BeTrue();
        odsOrganisation.Organisation.Should().NotBeNull();
        odsOrganisation.Organisation.OrgId.Should().NotBeNull();
        odsOrganisation.Organisation.OrgId.Extension.Should().Be(odsCode);
      }

      [Fact]
      public async void OdsCode_Invalid_WasFoundFalse()
      {
        // arrange
        // act
        var odsOrganisation = await _organisation.GetOdsOrganisationAsync("XXX");

        // assert
        odsOrganisation.WasFound.Should().BeFalse();
        odsOrganisation.Organisation.Should().BeNull();
      }
    }
  }
}
