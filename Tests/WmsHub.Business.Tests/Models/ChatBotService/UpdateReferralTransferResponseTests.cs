using FluentAssertions;
using Moq;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ChatBotService;
using Xunit;

namespace WmsHub.Business.Tests.Models.ChatBotService
{
  public class UpdateReferralTransferResponseTests
  {
    private class Response : UpdateReferralTransferResponse
    {
      public Response(UpdateReferralTransferRequest request) : base(request)
      {
      }
    }

    private Response _classToTest;

    private Mock<UpdateReferralTransferRequest> _mockRequest = new();

    public class ErrorsTests : UpdateReferralTransferResponseTests
    {
      [Fact]
      public void Valid_SetStatus()
      {
        //arrange
        _classToTest = new Response(_mockRequest.Object);
        //act
        _classToTest.SetStatus(StatusType.Invalid, "test");
        //assert
        _classToTest.Errors.Count.Should().Be(1);
        _classToTest.GetErrorMessage().Should().Be("test");
      }


      [Fact]
      public void Valid_SetStatus_Internal()
      {
        //arrange
        string expected = "An Outcome of Testing is unknown.";
        _mockRequest.Object.Outcome = "Testing";
        _classToTest = new Response(_mockRequest.Object);
        //act
        _classToTest.SetStatus(StatusType.OutcomeIsUnknown);
        //assert
        _classToTest.Errors.Count.Should().Be(1);
        _classToTest.GetErrorMessage().Should().Be(expected);
      }
    }
  }
}