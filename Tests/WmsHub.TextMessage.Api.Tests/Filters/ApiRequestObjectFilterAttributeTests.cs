using Xunit;
using WmsHub.TextMessage.Api.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.Notify;
using WmsHub.TextMessage.Api.Models.Notify;

namespace WmsHub.TextMessage.Api.Filters.Tests
{
  public class ApiRequestObjectFilterAttributeTests: ApiRequestObjectFilterAttribute
  {
    private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();

    private Mock<CallbackRequest> _mockRequest = new Mock<CallbackRequest>();

    private Mock<CallbackPostRequest> _mockPostRequest =
      new Mock<CallbackPostRequest>();

    private BadRequestObjectScaffold scaffold = new BadRequestObjectScaffold();

    [Fact()]
    public void Valid_Request()
    {
      //Arrange
      _mockRequest.Object.Status = "delivered";
      _mockRequest.Object.Reference = Guid.NewGuid().ToString();
      _mockRequest.Object.To = "+4477712345645";
      _mockRequest.Object.NotificationType = "sms";

      Dictionary<string, object> actionArgument =
        new Dictionary<string, object>() { { "request", _mockPostRequest.Object } };

      _mockMapper
       .Setup(t =>
          t.Map<CallbackPostRequest, CallbackRequest>(
            It.IsAny<CallbackPostRequest>())).Returns(_mockRequest.Object);

      Mapper = _mockMapper.Object;

      //Act
      bool result = ValidateRequest(actionArgument.First(), scaffold);
      //Assert
      result.Should().BeTrue();
    }

    [Fact()]
    public void InValid_Missing_To()
    {
      //Arrange
      string expected = "Message 'TO' must be supplied";
      _mockRequest.Object.Status = "delivered";
      _mockRequest.Object.Reference = Guid.NewGuid().ToString();
     // _mockRequest.Object.To = "+4477712345645";
      _mockRequest.Object.NotificationType = "sms";

      Dictionary<string, object> actionArgument =
        new Dictionary<string, object>() { { "request", _mockPostRequest.Object } };

      _mockMapper
       .Setup(t =>
          t.Map<CallbackPostRequest, CallbackRequest>(
            It.IsAny<CallbackPostRequest>())).Returns(_mockRequest.Object);

      Mapper = _mockMapper.Object;

      //Act
      bool result = ValidateRequest(actionArgument.First(), scaffold);
      //Assert
      result.Should().BeFalse();
      scaffold.Errors.ForEach(t=>t.Should().Be(expected));
    }

    [Fact()]
    public void InValid_Missing_Invalid_To()
    {
      //Arrange
      string expected = "To: 017712345645 must be a mobile number";
      _mockRequest.Object.Status = "delivered";
      _mockRequest.Object.Reference = Guid.NewGuid().ToString();
       _mockRequest.Object.To = "017712345645";
      _mockRequest.Object.NotificationType = "sms";

      Dictionary<string, object> actionArgument =
        new Dictionary<string, object>() { { "request", _mockPostRequest.Object } };

      _mockMapper
       .Setup(t =>
          t.Map<CallbackPostRequest, CallbackRequest>(
            It.IsAny<CallbackPostRequest>())).Returns(_mockRequest.Object);

      Mapper = _mockMapper.Object;

      //Act
      bool result = ValidateRequest(actionArgument.First(), scaffold);
      //Assert
      result.Should().BeFalse();
      scaffold.Errors.ForEach(t => t.Should().Be(expected));
    }

    [Fact()]
    public void InValid_Missing_Reference()
    {
      //Arrange
      string expected = "Reference must be supplied";
      _mockRequest.Object.Status = "delivered";
      //_mockRequest.Object.Reference = Guid.NewGuid().ToString();
       _mockRequest.Object.To = "+4477712345645";
      _mockRequest.Object.NotificationType = "Sms";

      Dictionary<string, object> actionArgument =
        new Dictionary<string, object>() { { "request", _mockPostRequest.Object } };

      _mockMapper
       .Setup(t =>
          t.Map<CallbackPostRequest, CallbackRequest>(
            It.IsAny<CallbackPostRequest>())).Returns(_mockRequest.Object);

      Mapper = _mockMapper.Object;

      //Act
      bool result = ValidateRequest(actionArgument.First(), scaffold);
      //Assert
      result.Should().BeFalse();
      scaffold.Errors.ForEach(t => t.Should().Be(expected));
    }

    [Fact()]
    public void InValid_Callback_Is_Email()
    {
      //Arrange
      string expected = "Only SMS Notification Type Callback allowed";
      _mockRequest.Object.Status = "delivered";
      _mockRequest.Object.Reference = Guid.NewGuid().ToString();
      _mockRequest.Object.To = "+4477712345645";
      _mockRequest.Object.NotificationType = "Email";

      Dictionary<string, object> actionArgument =
        new Dictionary<string, object>() { { "request", _mockPostRequest.Object } };

      _mockMapper
       .Setup(t =>
          t.Map<CallbackPostRequest, CallbackRequest>(
            It.IsAny<CallbackPostRequest>())).Returns(_mockRequest.Object);

      Mapper = _mockMapper.Object;

      //Act
      bool result = ValidateRequest(actionArgument.First(), scaffold);
      //Assert
      result.Should().BeFalse();
      scaffold.Errors.ForEach(t => t.Should().Be(expected));
    }
  }
}