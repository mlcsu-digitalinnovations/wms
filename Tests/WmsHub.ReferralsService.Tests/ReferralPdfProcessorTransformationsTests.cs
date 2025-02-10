using FluentAssertions;
using System;
using System.Collections.Generic;
using WmsHub.Common.Api.Models;
using WmsHub.ReferralsService.Pdf;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.ReferralsService.Tests
{
  public class ReferralPdfProcessorTransformationsTests
  {
    public class ConvertToDateTimeOffset
      : ReferralPdfProcessorTransformationsTests
    {

      public static IEnumerable<object[]> GetValidConversions()
      {
        return new List<object[]>
        {
          new object[] {"1st jan 2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"2nd jan 2021",
            new DateTimeOffset(2021,1,2,0,0,0, new TimeSpan()) },
          new object[] {"3rd jan 2021",
            new DateTimeOffset(2021,1,3,0,0,0, new TimeSpan()) },
          new object[] {"4th jan 2021",
            new DateTimeOffset(2021,1,4,0,0,0, new TimeSpan()) },
          new object[] {"1 jan 2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1 january 2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1 1 2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"01 01 2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1-1-2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"01-01-2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1/1/2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"01/01/2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"2021-1-1",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"2021-01-01",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1st jan 21",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1stjan2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1st jan2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1stjan 2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1st jan21",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1stjan 21",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1stjan 21 with other text after",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"text preceeding 1stjan 21",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"text surrounding 1stjan 21 random text",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1st jan 2021 other random text",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1st jan 2021 2/2/2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] {"1st jan 76",
            new DateTimeOffset(1976,1,1,0,0,0, new TimeSpan()) },
          new object[] {"Digital Weight Management 23-Sep-2021",
            new DateTimeOffset(2021,9,23,0,0,0, new TimeSpan()) },
          new object[] {"Digital Weight Management 23-Sep-21",
            new DateTimeOffset(2021,9,23,0,0,0, new TimeSpan()) },
          new object[] {"23-Sept-21",
            new DateTimeOffset(2021,9,23,0,0,0, new TimeSpan()) },
          new object[] {"23-Sept-2021",
            new DateTimeOffset(2021,9,23,0,0,0, new TimeSpan()) },
          new object[] {"23-January-21",
            new DateTimeOffset(2021,1,23,0,0,0, new TimeSpan()) },
          new object[] {"23-February-21",
            new DateTimeOffset(2021,2,23,0,0,0, new TimeSpan()) },
          new object[] {"23-March-21",
            new DateTimeOffset(2021,3,23,0,0,0, new TimeSpan()) },
          new object[] {"23-April-21",
            new DateTimeOffset(2021,4,23,0,0,0, new TimeSpan()) },
          new object[] {"23-May-21",
            new DateTimeOffset(2021,5,23,0,0,0, new TimeSpan()) },
          new object[] {"23-June-21",
            new DateTimeOffset(2021,6,23,0,0,0, new TimeSpan()) },
          new object[] {"23-July-21",
            new DateTimeOffset(2021,7,23,0,0,0, new TimeSpan()) },
          new object[] {"23-August-21",
            new DateTimeOffset(2021,8,23,0,0,0, new TimeSpan()) },
          new object[] {"23-September-21",
            new DateTimeOffset(2021,9,23,0,0,0, new TimeSpan()) },
          new object[] {"23-October-21",
            new DateTimeOffset(2021,10,23,0,0,0, new TimeSpan()) },
          new object[] {"23-November-21",
            new DateTimeOffset(2021,11,23,0,0,0, new TimeSpan()) },
          new object[] {"23-December-21",
            new DateTimeOffset(2021,12,23,0,0,0, new TimeSpan()) },
          new object[] {"23-January-2021",
            new DateTimeOffset(2021,1,23,0,0,0, new TimeSpan()) },
          new object[] {"23-February-2021",
            new DateTimeOffset(2021,2,23,0,0,0, new TimeSpan()) },
          new object[] {"23-March-2021",
            new DateTimeOffset(2021,3,23,0,0,0, new TimeSpan()) },
          new object[] {"23-April-2021",
            new DateTimeOffset(2021,4,23,0,0,0, new TimeSpan()) },
          new object[] {"23-May-2021",
            new DateTimeOffset(2021,5,23,0,0,0, new TimeSpan()) },
          new object[] {"23-June-2021",
            new DateTimeOffset(2021,6,23,0,0,0, new TimeSpan()) },
          new object[] {"23-July-2021",
            new DateTimeOffset(2021,7,23,0,0,0, new TimeSpan()) },
          new object[] {"23-August-2021",
            new DateTimeOffset(2021,8,23,0,0,0, new TimeSpan()) },
          new object[] {"23-September-2021",
            new DateTimeOffset(2021,9,23,0,0,0, new TimeSpan()) },
          new object[] {"23-October-2021",
            new DateTimeOffset(2021,10,23,0,0,0, new TimeSpan()) },
          new object[] {"23-November-2021",
            new DateTimeOffset(2021,11,23,0,0,0, new TimeSpan()) },
          new object[] {"23-December-2021",
            new DateTimeOffset(2021,12,23,0,0,0, new TimeSpan()) },
          new object[] {"20211023",
            new DateTimeOffset(2021,10,23,0,0,0, new TimeSpan()) },
          new object[] {"23102021",
            new DateTimeOffset(2021,10,23,0,0,0, new TimeSpan()) },
          new object[] {"23.10.2021",
            new DateTimeOffset(2021,10,23,0,0,0, new TimeSpan()) },
          new object[] {"some 20210224 text",
            new DateTimeOffset(2021,02,24,0,0,0, new TimeSpan()) },
          new object[] {"some 24022021 text",
            new DateTimeOffset(2021,02,24,0,0,0, new TimeSpan()) },
          new object[] {"other random text 1st jan 2021",
            new DateTimeOffset(2021,1,1,0,0,0, new TimeSpan()) },
          new object[] { "14 Sep 2021, 876Kg",
            new DateTimeOffset(2021,9,14,0,0,0, new TimeSpan()) },
          
          //Unicode
          new object[] {"25‐Jun‐1987", //U+2010
            new DateTimeOffset(1987,6,25,0,0,0, new TimeSpan()) },
          new object[] {"25‐Jun‐1987", //U+2011
            new DateTimeOffset(1987,6,25,0,0,0, new TimeSpan()) },
          new object[] {"25‒Jun‒1987", //U+2012
            new DateTimeOffset(1987,6,25,0,0,0, new TimeSpan()) },
          new object[] {"25–Jun–1987", //U+2013
            new DateTimeOffset(1987,6,25,0,0,0, new TimeSpan()) },
          new object[] {"25—Jun—1987", //U+2014
            new DateTimeOffset(1987,6,25,0,0,0, new TimeSpan()) },
          new object[] {"25⁄Jun⁄1987", //U+2044
            new DateTimeOffset(1987,6,25,0,0,0, new TimeSpan()) },
          };
      }

      [Theory]
      [MemberData(nameof(GetValidConversions))]
      public void Valid(string answer, DateTimeOffset expectedResult)
      {
        // arrange
        string question = "test question";
        var serilogMock = new SerilogLoggerMock();
        var processor = new ReferralAttachmentPdfProcessor(serilogMock, 
          serilogMock, null, null, null);

        // act
        DateTimeOffset? result = processor
          .ConvertToDateTimeOffset(question, answer);

        // assert
        result.Should().NotBeNull();
        result.Value.Should().Be(expectedResult);
      }

    }

    public class ConvertToSystemVersion
      : ReferralPdfProcessorTransformationsTests
    {
      [Theory]
      [InlineData("System:Emis V1.23", "1.23")]
      [InlineData("","0")]
      [InlineData("System: Emis V1.24", "1.24")]
      [InlineData("System:Emis", "0")]
      [InlineData("System:EmisV1.25", "1.25")]
      public void SystemVersion(string answerToCheck, string expectedAnswer)
      {
        var serilogMock = new SerilogLoggerMock();
        var processor = new ReferralAttachmentPdfProcessor(serilogMock, 
          serilogMock, null, null, null);

        // act
        decimal result = processor
          .ConvertToDecimal(answerToCheck);

        // assert
        $"{result}".Should().Be(expectedAnswer);
      }
    }

    public class ConvertToWeightInKg
      : ReferralPdfProcessorTransformationsTests
    {
      [Theory]
      [InlineData("30-JUL-21: 40.12 kg", "40.12")]
      [InlineData("Equal: 40.12 kg", "40.12")]
      [InlineData("Weight : 31.99kg", "31.99")]
      [InlineData("Some text: 31.99kg 42 extra text", "31.99")]
      [InlineData("some text : 31.99kg extra text", "31.99")]
      [InlineData("some text 31.99kg extra text", "31.99")]
      [InlineData("some text 31.99: extra text", "31.99")]
      [InlineData("text 31.99 text", "31.99")]
      [InlineData("80kg", "80")]
      [InlineData("80 kg", "80")]
      [InlineData("20.77 80 kg", "80")]
      [InlineData("20.77 80", "20.77")]
      [InlineData("80", "80")]
      [InlineData("27.5", "27.5")]
      [InlineData("=74.4", "74.4")]
      [InlineData("≈74.4", "74.4")]
      [InlineData("", "")]
      public void WeightInKg(string answerToCheck, string expectedAnswer)
      {
        // act
        ReferralAttachmentPdfProcessor processor =
          new ReferralAttachmentPdfProcessor(null, null, null, null, null);
        decimal? answer =
          processor.ConvertToWeightinKg("WeightInKg:", answerToCheck);
        // assert
        $"{answer}".Should().Be(expectedAnswer);
      }

    }

    public class ConvertToBMI
      : ReferralPdfProcessorTransformationsTests
    {
      [Theory]
      [InlineData("30-JUL-21: 40.12 kg/m2", "40.12")]
      [InlineData("Body mass index: 40.12 kg/m2", "40.12")]
      [InlineData("Body mass index : 31.99kg/m2", "31.99")]
      [InlineData("Body mass index : 31.99kg/m2 42 extra text", "31.99")]
      [InlineData("Body mass index : 31.99kg/m2 extra text", "31.99")]
      [InlineData("80kg/m2", "80")]
      [InlineData("80 kg/m2", "80")]
      [InlineData("20.77 80 kg/m2", "80")]
      [InlineData("20.77 80", "20.77")]
      [InlineData("80", "80")]
      [InlineData("27.5", "27.5")]
      [InlineData("=23", "23")]
      [InlineData("≈23", "23")]
      [InlineData("", "")]
      public void Bmi(string answerToCheck, string expectedAnswer)
      {
        // act
        decimal? answer = new ReferralAttachmentPdfProcessor(null, null, null, 
          null, null).
          ConvertToBMI("BMI:", answerToCheck);
        // assert
        $"{answer}".Should().Be(expectedAnswer);
      }

    }

    public class CenvertToHeightInCm
    {
      [Theory]
      [InlineData("1.4", "140.0")]
      [InlineData("160", "160.0")]
      [InlineData("=1.5", "150.0")]
      [InlineData("=123", "123.0")]
      [InlineData("160cm", "160.0")]
      [InlineData("160 cm", "160.0")]
      [InlineData("1.58m", "158.0")]
      [InlineData("1.58 m", "158.0")]
      [InlineData("=158cm", "158.0")]
      [InlineData("≈158cm", "158.0")]
      [InlineData("=158 cm", "158.0")]
      [InlineData("≈158 cm", "158.0")]
      [InlineData("12/4/22 70cm 8/6/21 40cm 16/1/21 35cm","")]
      [InlineData("", "")]
      public void ConvertToHeightInCm(
        string answerToCheck, string expectedAnswer)
      {
        // act
        decimal? answer = new ReferralAttachmentPdfProcessor(null, null, null, 
          null, null).
          ConvertToHeightinCm("Height:", answerToCheck);
        // assert
        $"{answer:0.0}".Should().Be(expectedAnswer);
      }

    }

    public class SplitAndExtractCorrectedAnswer
      : ReferralPdfProcessorTransformationsTests
    {

      [Theory]
      [InlineData("12-JUL-2021 Standing height: 180 cm", "180")]
      [InlineData("180cm", "180")]
      [InlineData("180cms", "180")]
      [InlineData("180 cms", "180")]
      [InlineData("180", "180")]
      [InlineData("99", "")]
      [InlineData("100", "100")]
      [InlineData("250", "250")]
      [InlineData("251", "")]
      [InlineData("0.9m", "")]
      [InlineData("1.0", "100.0")]
      [InlineData("2.5", "250.0")]
      [InlineData("2.6 m", "")]
      public void Height(string answerToCheck, string expectedAnswer)
      {
        // act
        var answer = ReferralAttachmentPdfProcessor
          .SplitAndExtractCorrectedAnswer(answerToCheck, 100, 250, true, 100);

        // assert
        answer.Should().Be(expectedAnswer);
      }
    }

    public class LateReferralObjectTransformations :
      ReferralPdfProcessorTransformationsTests
    {
      [Fact]
      public void MissingAddressFixTestPost()
      {
        ReferralAttachmentPdfProcessor pdf =
          new ReferralAttachmentPdfProcessor(null, null, null, null, null);

        ReferralPost referralPost = new ReferralPost()
        {
          Address1 = "Valid 1",
          Address2 = "Valid 2",
          Address3 = "Valid 3"
        };
        pdf.FixAddressFieldsIfAddress1Missing(referralPost);
        referralPost.Address1.Should().Be("Valid 1");
        referralPost.Address2.Should().Be("Valid 2");
        referralPost.Address3.Should().Be("Valid 3");

        referralPost = new ReferralPost()
        {
          Address1 = null,
          Address2 = "Valid 2",
          Address3 = "Valid 3"
        };
        pdf.FixAddressFieldsIfAddress1Missing(referralPost);
        referralPost.Address1.Should().Be("Valid 2");
        referralPost.Address2.Should().Be("Valid 3");
        referralPost.Address3.Should().BeEmpty();

        referralPost = new ReferralPost()
        {
          Address1 = null,
          Address2 = null,
          Address3 = "Valid 3"
        };
        pdf.FixAddressFieldsIfAddress1Missing(referralPost);
        referralPost.Address1.Should().Be("Valid 3");
        referralPost.Address2.Should().BeEmpty();
        referralPost.Address3.Should().BeEmpty();

        referralPost = new ReferralPost()
        {
          Address1 = null,
          Address2 = "Valid 2",
          Address3 = null
        };
        pdf.FixAddressFieldsIfAddress1Missing(referralPost);
        referralPost.Address1.Should().Be("Valid 2");
        referralPost.Address2.Should().BeNull();
        referralPost.Address3.Should().BeEmpty();

        referralPost = new ReferralPost()
        {
          Address1 = "Valid 1",
          Address2 = null,
          Address3 = null
        };
        pdf.FixAddressFieldsIfAddress1Missing(referralPost);
        referralPost.Address1.Should().Be("Valid 1");
        referralPost.Address2.Should().BeNull();
        referralPost.Address3.Should().BeNull();

      }

      [Fact]
      public void MissingAddressFixTestPut()
      {
        ReferralAttachmentPdfProcessor pdf =
          new ReferralAttachmentPdfProcessor(null, null, null, null, null);

        ReferralPut referralPut = new ReferralPut()
        {
          Address1 = "Valid 1",
          Address2 = "Valid 2",
          Address3 = "Valid 3"
        };
        pdf.FixAddressFieldsIfAddress1Missing(referralPut);
        referralPut.Address1.Should().Be("Valid 1");
        referralPut.Address2.Should().Be("Valid 2");
        referralPut.Address3.Should().Be("Valid 3");

        referralPut = new ReferralPut()
        {
          Address1 = null,
          Address2 = "Valid 2",
          Address3 = "Valid 3"
        };
        pdf.FixAddressFieldsIfAddress1Missing(referralPut);
        referralPut.Address1.Should().Be("Valid 2");
        referralPut.Address2.Should().Be("Valid 3");
        referralPut.Address3.Should().BeEmpty();

        referralPut = new ReferralPut()
        {
          Address1 = null,
          Address2 = null,
          Address3 = "Valid 3"
        };
        pdf.FixAddressFieldsIfAddress1Missing(referralPut);
        referralPut.Address1.Should().Be("Valid 3");
        referralPut.Address2.Should().BeEmpty();
        referralPut.Address3.Should().BeEmpty();

        referralPut = new ReferralPut()
        {
          Address1 = null,
          Address2 = "Valid 2",
          Address3 = null
        };
        pdf.FixAddressFieldsIfAddress1Missing(referralPut);
        referralPut.Address1.Should().Be("Valid 2");
        referralPut.Address2.Should().BeNull();
        referralPut.Address3.Should().BeEmpty();

        referralPut = new ReferralPut()
        {
          Address1 = "Valid 1",
          Address2 = null,
          Address3 = null
        };
        pdf.FixAddressFieldsIfAddress1Missing(referralPut);
        referralPut.Address1.Should().Be("Valid 1");
        referralPut.Address2.Should().BeNull();
        referralPut.Address3.Should().BeNull();

      }

      [Fact]
      public void DiabetesHypertensionMissingAnswerTestPut()
      {
        ReferralAttachmentPdfProcessor pdf =
          new ReferralAttachmentPdfProcessor(null, null, null, null, null);

        ReferralPut testPut = new ReferralPut()
        {
          HasDiabetesType1 = true,
          HasDiabetesType2 = null,
          HasHypertension = null
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPut);
        testPut.HasDiabetesType1.Should().BeTrue();
        testPut.HasDiabetesType2.Should().BeFalse();
        testPut.HasHypertension.Should().BeFalse();

        testPut = new ReferralPut()
        {
          HasDiabetesType1 = false,
          HasDiabetesType2 = null,
          HasHypertension = null
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPut);

        testPut.HasDiabetesType1.Should().BeFalse();
        testPut.HasDiabetesType2.Should().BeNull();
        testPut.HasHypertension.Should().BeNull();

        testPut = new ReferralPut()
        {
          HasDiabetesType1 = null,
          HasDiabetesType2 = true,
          HasHypertension = null
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPut);
        testPut.HasDiabetesType1.Should().BeFalse();
        testPut.HasDiabetesType2.Should().BeTrue();
        testPut.HasHypertension.Should().BeFalse();

        testPut = new ReferralPut()
        {
          HasDiabetesType1 = null,
          HasDiabetesType2 = null,
          HasHypertension = true
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPut);
        testPut.HasDiabetesType1.Should().BeFalse();
        testPut.HasDiabetesType2.Should().BeFalse();
        testPut.HasHypertension.Should().BeTrue();

        testPut = new ReferralPut()
        {
          HasDiabetesType1 = null,
          HasDiabetesType2 = null,
          HasHypertension = null
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPut);
        testPut.HasDiabetesType1.Should().BeNull();
        testPut.HasDiabetesType2.Should().BeNull();
        testPut.HasHypertension.Should().BeNull();

        testPut = new ReferralPut()
        {
          HasDiabetesType1 = false,
          HasDiabetesType2 = true,
          HasHypertension = false
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPut);
        testPut.HasDiabetesType1.Should().BeFalse();
        testPut.HasDiabetesType2.Should().BeTrue();
        testPut.HasHypertension.Should().BeFalse();

      }

      [Fact]
      public void DiabetesHypertensionMissingAnswerTestPost()
      {
        ReferralAttachmentPdfProcessor pdf =
          new ReferralAttachmentPdfProcessor(null, null, null, null, null);

        ReferralPost testPost = new ReferralPost()
        {
          HasDiabetesType1 = true,
          HasDiabetesType2 = null,
          HasHypertension = null
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPost);
        testPost.HasDiabetesType1.Should().BeTrue();
        testPost.HasDiabetesType2.Should().BeFalse();
        testPost.HasHypertension.Should().BeFalse();

        testPost = new ReferralPost()
        {
          HasDiabetesType1 = false,
          HasDiabetesType2 = null,
          HasHypertension = null
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPost);

        testPost.HasDiabetesType1.Should().BeFalse();
        testPost.HasDiabetesType2.Should().BeNull();
        testPost.HasHypertension.Should().BeNull();

        testPost = new ReferralPost()
        {
          HasDiabetesType1 = null,
          HasDiabetesType2 = true,
          HasHypertension = null
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPost);
        testPost.HasDiabetesType1.Should().BeFalse();
        testPost.HasDiabetesType2.Should().BeTrue();
        testPost.HasHypertension.Should().BeFalse();

        testPost = new ReferralPost()
        {
          HasDiabetesType1 = null,
          HasDiabetesType2 = null,
          HasHypertension = true
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPost);
        testPost.HasDiabetesType1.Should().BeFalse();
        testPost.HasDiabetesType2.Should().BeFalse();
        testPost.HasHypertension.Should().BeTrue();

        testPost = new ReferralPost()
        {
          HasDiabetesType1 = null,
          HasDiabetesType2 = null,
          HasHypertension = null
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPost);
        testPost.HasDiabetesType1.Should().BeNull();
        testPost.HasDiabetesType2.Should().BeNull();
        testPost.HasHypertension.Should().BeNull();

        testPost = new ReferralPost()
        {
          HasDiabetesType1 = false,
          HasDiabetesType2 = true,
          HasHypertension = false
        };
        pdf.RemoveNullAnswersIfAnyDiabetesHypertensionsAnswersAreTrue(testPost);
        testPost.HasDiabetesType1.Should().BeFalse();
        testPost.HasDiabetesType2.Should().BeTrue();
        testPost.HasHypertension.Should().BeFalse();

      }

    }

    public class ConvertToTelephoneNumber 
      : ReferralPdfProcessorTransformationsTests
    {
      [Theory]
      [InlineData("01234 123456", "+441234123456")]
      [InlineData("Tel: 01234 123456", "+441234123456")] //Vision 1.0 Style
      [InlineData("Mob: 01234 123456", "+441234123456")] //Vision 1.0 Style
      [InlineData("Tel. (01234) 123456", "+441234123456")]
      [InlineData("01234123456", "+441234123456")]
      [InlineData("+441234 123456", "+441234123456")]
      [InlineData("+441234123456", "+441234123456")]
      [InlineData("+44 1234 123456", "+441234123456")]
      [InlineData("(01234) 123456", "+441234123456")]
      [InlineData("(01234)123456", "+441234123456")]
      [InlineData("", "")]
      public void TelephoneNumber(string answerToCheck, string expectedAnswer)
      {
        // act
        ReferralAttachmentPdfProcessor processor =
          new ReferralAttachmentPdfProcessor(null, null, null, null, null);
        string answer =
          processor.ConvertToTelephoneNumber(answerToCheck);
        // assert
        $"{answer}".Should().Be(expectedAnswer);
      }

    }


  }
}