using FluentAssertions;
using System;
using System.Collections.Generic;
using WmsHub.Common.Api.Models;
using WmsHub.ReferralsService.Pdf;
using WmsHub.Tests.Helper;
using Xunit;


namespace WmsHub.ReferralsService.Tests
{

  public class ReferralProcessorInterventionTests
  {
    private SerilogLoggerMock _serilogMock;
    private ReferralAttachmentPdfProcessor _pdfProcessor;
    private ReferralAttachmentAnswerMap _map;

    public ReferralProcessorInterventionTests()
    {
      _serilogMock = new SerilogLoggerMock();
      _map = new ReferralAttachmentAnswerMap();
      _map.Load(".\\Files\\");

      _pdfProcessor = new ReferralAttachmentPdfProcessor(
        _serilogMock, _serilogMock, null, null, _map);
    }

    public class FixMultipleDiabetesHypertensionAnswersTests
      : ReferralProcessorInterventionTests
    {
      [Fact]
      public void TestTransformationWithPositiveAnswers()
      {
        //Arrange
        List<string> beforeAllYes = new List<string>
        {
          "Diabetes Type 1:",
          "Y",
          "Y",
          "Diabetes Type 2:",
          "Y",
          "Y",
          "Hypertension:",
          "Y",
          "Y"
        };
        List<string> expected = new List<string>
        {
          "Diabetes Type 1:",
          "Y",
          "Diabetes Type 2:",
          "Y",
          "Hypertension:",
          "Y"
        };
        _pdfProcessor.DocumentContent = beforeAllYes;

        //Act
        _pdfProcessor.FixMultipleDiabetesHypertensionAnswers();

        //Assert
        _pdfProcessor.DocumentContent.Should().ContainInOrder(expected);
      }

      [Fact]
      public void TestTransformationWithNegativeAnswers()
      {
        //Arrange
        List<string> beforeAllNo = new List<string>
        {
          "Diabetes Type 1:",
          "N",
          "N",
          "Diabetes Type 2:",
          "N",
          "N",
          "Hypertension:",
          "N",
          "N"
        };
        List<string> expected = new List<string>
        {
          "Diabetes Type 1:",
          "N",
          "Diabetes Type 2:",
          "N",
          "Hypertension:",
          "N"
        };
        _pdfProcessor.DocumentContent = beforeAllNo;

        //Act
        _pdfProcessor.FixMultipleDiabetesHypertensionAnswers();

        //Assert
        _pdfProcessor.DocumentContent.Should().ContainInOrder(expected);
      }

      [Fact]
      public void TestAlreadyCorrect()
      {
        //Arrange
        List<string> correctValues = new List<string>
        {
          "Diabetes Type 1:",
          "Y",
          "Diabetes Type 2:",
          "Y",
          "Hypertension:",
          "Y"
        };
        _pdfProcessor.DocumentContent.Clear();
        _pdfProcessor.DocumentContent.AddRange(correctValues);

        //Act
        _pdfProcessor.FixMultipleDiabetesHypertensionAnswers();

        //Assert
        _pdfProcessor.DocumentContent.Should().ContainInOrder(correctValues);
      }

      [Fact]
      public void TestNotAnsweredQuestions()
      {
        //Arrange
        List<string> unansweredValues = new List<string>
        {
          "Diabetes Type 1:",
          "Y","N",
          "Diabetes Type 2:",
          "Y","N",
          "Hypertension:",
          "Y","N"
        };
        _pdfProcessor.DocumentContent.Clear();
        _pdfProcessor.DocumentContent.AddRange(unansweredValues);

        //Act
        _pdfProcessor.FixMultipleDiabetesHypertensionAnswers();

        //Assert
        _pdfProcessor.DocumentContent.Should().ContainInOrder(unansweredValues);
      }

    }
  }
}
