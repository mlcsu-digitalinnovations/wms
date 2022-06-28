using FluentAssertions;
using Moq;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ChatBotService;
using Xunit;

namespace WmsHub.Business.Tests.Models.ChatBotService
{
  public class BaseResponseTests
  {
    private class ResponseMock : BaseResponse
    {
    }

    private ResponseMock _classToTest;

    public class ErrorsTests : BaseResponseTests
    {
      [Fact]
      public void Valid_SetStatus()
      {
        //arrange
        _classToTest = new ResponseMock();
        //act
        _classToTest.SetStatus(StatusType.Invalid, "test");
        //assert
        _classToTest.Errors.Count.Should().Be(1);
        _classToTest.GetErrorMessage().Should().Be("test");
      }


      [Fact]
      public void Valid_SetStatus_Internal_CallIdDoesNotExist()
      {
        //arrange
        string expected = "StatusType.CallIdDoesNotExist: TODO";
        _classToTest = new ResponseMock();
        //act
        _classToTest.SetStatus(StatusType.CallIdDoesNotExist);
        //assert
        _classToTest.Errors.Count.Should().Be(1);
        _classToTest.GetErrorMessage().Should().Be(expected);
      }

      [Fact]
      public void Valid_SetStatus_Internal_OutcomeIsUnknown()
      {
        //arrange
        string expected = "StatusType.OutcomeIsUnknown: TODO";
        _classToTest = new ResponseMock();
        //act
        _classToTest.SetStatus(StatusType.OutcomeIsUnknown);
        //assert
        _classToTest.Errors.Count.Should().Be(1);
        _classToTest.GetErrorMessage().Should().Be(expected);
      }

      [Fact]
      public void Valid_SetStatus_Internal_TelephoneNumberMismatch()
      {
        //arrange
        string expected = "StatusType.TelephoneNumberMismatch: TODO";
        _classToTest = new ResponseMock();
        //act
        _classToTest.SetStatus(StatusType.TelephoneNumberMismatch);
        //assert
        _classToTest.Errors.Count.Should().Be(1);
        _classToTest.GetErrorMessage().Should().Be(expected);
      }
    }
  }
}