using AutoMapper;
using FluentAssertions.Execution;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Services.Interfaces;
using WmsHub.BusinessIntelligence.Api.Controllers;
using WmsHub.BusinessIntelligence.Api.Test;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using WmsHub.BusinessIntelligence.Api.Models;
using BI = WmsHub.Business.Models.BusinessIntelligence;
using Microsoft.AspNetCore.Http;
using WmsHub.Common.Helpers;
using WmsHub.Business.Helpers;

namespace WmsHub.BusinessIntelligence.Api.Tests;

[Collection("Service collection")]
public class QuestionnaireControllerTests : ServiceTestsBase, IDisposable
{
  private readonly Mock<IBusinessIntelligenceService> _serviceMock;
  private readonly QuestionnaireController _controller;

  public QuestionnaireControllerTests(ServiceFixture serviceFixture)
    : base(serviceFixture)
  {
    _serviceMock = new Mock<IBusinessIntelligenceService>();

    _controller = new QuestionnaireController(
      _serviceMock.Object,
      _serviceFixture.Mapper);

    CleanUp();
  }

  [Fact]
  public void WhenMapperIsNullShouldThrowArgumentNullException()
  {
    using (new AssertionScope())
    {
      // Arrange.
      // Act.
      Action action = () =>
      {
        QuestionnaireController questionnaireController = new(
          _serviceMock.Object, null);
      };

      // Assert.
      action.Should().Throw<ArgumentNullException>();
    }
  }

  public void Dispose()
  {
    CleanUp();
  }

  protected void CleanUp()
  {
  }

  public class GetQuestionnaireAsync : QuestionnaireControllerTests
  {
    public GetQuestionnaireAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task BadRequest()
    {
      DateTimeOffset? fromDate = DateTimeOffset.Now.AddDays(-20);
      DateTimeOffset? toDate = DateTimeOffset.Now.AddDays(-40);

      var expectedMessage =
        $"'from' date {fromDate.Value} cannot be later than 'to' date " +
        $"{toDate.Value}.";

      // Act.
      IActionResult result =
        await _controller.GetQuestionnaireAsync(fromDate, toDate);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedMessage);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
      }
    }

    [Fact]
    public async Task BothDatesNull_ReturnsOk()
    {
      // Arrange.
      BiQuestionnaire expectedBiQuestionnaire = new()
      {
        Answers = "Answers",
        ConsentToShare = true,
        Id = Guid.NewGuid(),
        QuestionnaireType = Business.Enums.QuestionnaireType.CompleteSelfT1,
        Ubrn = Generators.GenerateUbrn(new Random())
      };

      List<BiQuestionnaire> expectedList = new() { expectedBiQuestionnaire };

      _serviceMock
        .Setup(x => x.GetQuestionnaires(
          It.IsAny<DateTimeOffset>(),
          It.IsAny<DateTimeOffset>()))
        .ReturnsAsync(new List<BI.BiQuestionnaire>{
          new()
          {
            Answers = expectedBiQuestionnaire.Answers,
            ConsentToShare = expectedBiQuestionnaire.ConsentToShare,
            Id = expectedBiQuestionnaire.Id,
            QuestionnaireType = expectedBiQuestionnaire.QuestionnaireType,
            Ubrn = expectedBiQuestionnaire.Ubrn
          }
        });

      // Act.
      IActionResult result =
        await _controller.GetQuestionnaireAsync(null, null);

      // Assert.
      using (new AssertionScope())
      {
        OkObjectResult okResult = result.As<OkObjectResult>();
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        IEnumerable<BiQuestionnaire> resultValue =
          okResult.Value.As<IEnumerable<BiQuestionnaire>>();

        resultValue.Should().NotBeNullOrEmpty();
        resultValue.First().Should().BeEquivalentTo(expectedBiQuestionnaire);
      }
    }

    [Fact]
    public async Task NoContent()
    {
      // Arrange.
      _serviceMock
        .Setup(x => x.GetQuestionnaires(
          It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
        .ReturnsAsync(new List<BI.BiQuestionnaire>());

      // Act.
      IActionResult result =
        await _controller.GetQuestionnaireAsync(null, null);

      // Assert.
      using (new AssertionScope())
      {
        NoContentResult outputResult = result.As<NoContentResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
      }
    }

    [Fact]
    public async Task InternalServerError()
    {
      //Arrange.
      _serviceMock
        .Setup(x => x.GetQuestionnaires(
          It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
        .ThrowsAsync(new Exception());

      // Act.
      IActionResult result =
        await _controller.GetQuestionnaireAsync(null, null);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }
  }
}
