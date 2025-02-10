using FluentAssertions;
using FluentAssertions.Execution;
using System.Linq;
using WmsHub.Business.Models;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models;

public class CompleteQuestionnaireTests : AModelsBaseTests
{
  const bool TEST_ANSWERS_EXPECTED_CONSENT = true;
  const string TEST_ANSWERS_EXPECTED_EMAIL = "test.questionnaire@nhs.net";
  const string TEST_ANSWERS_EXPECTED_MOBILE = "+447715427599";
  const string TEST_ANSWERS_EXPECTED_FAMILY_NAME = "TestFamily";
  const string TEST_ANSWERS_EXPECTED_GIVEN_NAME = "TestGiven";

  const string VALID_ANSWERS_START =
    "[{\"QuestionId\":1," +
    "\"a\":\"StronglyAgree\"," +
    "\"b\":\"Agree\"," +
    "\"c\":\"NeitherAgreeOrDisagree\"}," +
    "{\"QuestionId\":2," +
    "\"a\":\"StronglyAgree\"," +
    "\"b\":\"Agree\"," +
    "\"c\":\"NeitherAgreeOrDisagree\"" +
    ",\"d\":\"Disagree\"" +
    ",\"e\":\"StronglyDisagree\"," +
    "\"f\":\"StronglyAgree\"," +
    "\"g\":\"Agree\"," +
    "\"h\":\"NeitherAgreeOrDisagree\"}," +
    "{\"QuestionId\":3," +
    "\"a\":\"StronglyAgree\"," +
    "\"b\":\"Agree\"," +
    "\"c\":\"NeitherAgreeOrDisagree\"," +
    "\"d\":\"Disagree\"}," +
    "{\"QuestionId\":4," +
    "\"a\":\"VeryGood\"}," +
    "{\"QuestionId\":5," +
    "\"a\":\"some random\"}," +
    "{\"QuestionId\":6," +
    "\"a\":\"\"},";

  const string VALID_ANSWERS = VALID_ANSWERS_START +
    "{\"QuestionId\":7," +
    "\"a\":\"true\"," +
    "\"b\":\"test.questionnaire@nhs.net\"," +
    "\"c\":\"+447715427599\"," +
    "\"d\":\"TestGiven\"," +
    "\"e\":\"TestFamily\"" +
    "}]";

  [Fact]
  public void MissingProperties_Invalid()
  {
    // Arrange.
    CompleteQuestionnaire model = new();


    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(2);
      result.Results
        .Any(x => x.ErrorMessage.Contains("Answers"))
        .Should().BeTrue();
      result.Results
        .Any(x => x.ErrorMessage.Contains("NotificationKey"))
        .Should().BeTrue();
    }
  }

  [Fact]
  public void MissingQuestionnaireType_Invalid()
  {
    // Arrange.
    CompleteQuestionnaire model = new()
    {
      Answers = VALID_ANSWERS,
      NotificationKey = "14uijna46i7cz"
    };
    string expectedErrorMessage = "QuestionnaireType is required";
    string[] expectedMemberNames = { "QuestionnaireType" };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.Results[0].ErrorMessage.Should().Contain(expectedErrorMessage);
      result.Results[0].MemberNames.Should().HaveCount(1);
      result.Results[0].MemberNames.Should()
        .BeEquivalentTo(expectedMemberNames);
    }
  }

  [Fact]
  public void ConsentAnswerInvalidJson_Invalid()
  {
    string invalidConsentAnswer = VALID_ANSWERS_START + "{QuestionId:7}]";

    // Arrange.
    CompleteQuestionnaire model = new()
    {
      Answers = invalidConsentAnswer,
      NotificationKey = "14uijna46i7cz",
      QuestionnaireType = Enums.QuestionnaireType.CompleteSelfT1
    };
    string expectedErrorMessage = "must be valid json";
    string[] expectedMemberNames = { "Answers" };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.Results[0].ErrorMessage.Should().Contain(expectedErrorMessage);
      result.Results[0].MemberNames.Should().HaveCount(1);
      result.Results[0].MemberNames.Should()
        .BeEquivalentTo(expectedMemberNames);
    }
  }

  [Fact]
  public void ConsentAnswerCannotBeDeserialised_Invalid()
  {
    string invalidConsentAnswer = "{\"QuestionId\":1}";

    // Arrange.
    CompleteQuestionnaire model = new()
    {
      Answers = invalidConsentAnswer,
      NotificationKey = "14uijna46i7cz",
      QuestionnaireType = Enums.QuestionnaireType.CompleteSelfT1
    };
    string expectedErrorMessage = "Cannot deserialize the current JSON object";
    string[] expectedMemberNames = { "Answers" };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.Results[0].ErrorMessage.Should().Contain(expectedErrorMessage);
      result.Results[0].MemberNames.Should().HaveCount(1);
      result.Results[0].MemberNames.Should()
        .BeEquivalentTo(expectedMemberNames);
    }
  }

  [Theory]
  [InlineData(6)]
  [InlineData(8)]
  public void QuestionIdDoesNotMatchNumberOfAnswers_Invalid(int questionId)
  {
    string invalidConsentAnswer = VALID_ANSWERS_START +
      $"{{\"QuestionId\":{questionId}," +
      "\"a\":\"false\"}]";

    // Arrange.
    CompleteQuestionnaire model = new()
    {
      Answers = invalidConsentAnswer,
      NotificationKey = "14uijna46i7cz",
      QuestionnaireType = Enums.QuestionnaireType.CompleteSelfT1
    };
    string expectedErrorMessage = "Found Answers property QuestionId";
    string[] expectedMemberNames = { "Answers" };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.Results[0].ErrorMessage.Should().Contain(expectedErrorMessage);
      result.Results[0].MemberNames.Should().HaveCount(1);
      result.Results[0].MemberNames.Should()
        .BeEquivalentTo(expectedMemberNames);
    }
  }

  [Fact]
  public void InvalidConsentAnswer_Invalid()
  {
    string invalidConsentAnswer = VALID_ANSWERS_START + "{\"QuestionId\":7}]";

    // Arrange.
    CompleteQuestionnaire model = new()
    {
      Answers = invalidConsentAnswer,
      NotificationKey = "14uijna46i7cz",
      QuestionnaireType = Enums.QuestionnaireType.CompleteSelfT1
    };
    string expectedErrorMessage = "expected consent answer";
    string[] expectedMemberNames = { "Answers" };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.Results[0].ErrorMessage.Should().Contain(expectedErrorMessage);
      result.Results[0].MemberNames.Should().HaveCount(1);
      result.Results[0].MemberNames.Should()
        .BeEquivalentTo(expectedMemberNames);
    }
  }

  [Fact]
  public void InvalidEmailWithConsentTrue_Invalid()
  {
    // Arrange.
    string invalidEmail = VALID_ANSWERS_START + 
      "{\"QuestionId\":7," +
      "\"a\":\"true\"," +
      "\"b\":\"invalid.email\"," +
      "\"c\":\"+447000000000\"}]";

    CompleteQuestionnaire model = new()
    {
      Answers = invalidEmail,
      NotificationKey = "14uijna46i7cz",
      QuestionnaireType = Enums.QuestionnaireType.CompleteSelfT1
    };
    string expectedErrorMessage = "not a valid e-mail address";
    string[] expectedMemberNames = { "Email" };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.Results[0].ErrorMessage.Should().Contain(expectedErrorMessage);
      result.Results[0].MemberNames.Should().HaveCount(1);
      result.Results[0].MemberNames.Should().Contain(expectedMemberNames);
    }
  }

  [Fact]
  public void InvalidMobileWithConsentTrue_Invalid()
  {
    // Arrange.
    string invalidMobile = VALID_ANSWERS_START + 
      "{\"QuestionId\":7," +
      "\"a\":\"true\"," +
      "\"b\":\"valid.email@test.com\"," +
      "\"c\":\"7000000000\"}]";

    CompleteQuestionnaire model = new()
    {
      Answers = invalidMobile,
      NotificationKey = "14uijna46i7cz",
      QuestionnaireType = Enums.QuestionnaireType.CompleteSelfT1
    };
    string expectedErrorMessage = "valid UK mobile number";
    string[] expectedMemberNames = { "Mobile" };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.Results[0].ErrorMessage.Should().Contain(expectedErrorMessage);
      result.Results[0].MemberNames.Should().HaveCount(1);
      result.Results[0].MemberNames.Should().Contain(expectedMemberNames);
    }
  }

  [Fact]
  public void InvalidEmailAndMobileMissingNamesWithConsentFalse_Valid()
  {
    // Arrange.
    string expectedAnswers = VALID_ANSWERS_START + 
      "{\"QuestionId\":7," +
      "\"a\":\"false\"," +
      "\"b\":\"invalid.email@test.com\"," +
      "\"c\":\"7000000000\"}]";

    CompleteQuestionnaire model = new()
    {
      Answers = expectedAnswers,
      NotificationKey = "14uijna46i7cz",
      QuestionnaireType = Enums.QuestionnaireType.CompleteSelfT1
    };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();

      model.Answers.Should().Be(expectedAnswers);
      model.ConsentToShare.Should().BeFalse();
      model.Email.Should().BeNull();
      model.FamilyName.Should().BeNull();
      model.GivenName.Should().BeNull();
      model.Mobile.Should().BeNull();
    }
  }

  [Fact]
  public void NullEmailAndMobileWithConsentTrue_Invalid()
  {
    // Arrange.
    string nullEmailAndMobile = VALID_ANSWERS_START + 
      "{\"QuestionId\":7," +
      "\"a\":\"true\"," +
      "\"b\":null," +
      "\"c\":null}]";

    CompleteQuestionnaire model = new()
    {
      Answers = nullEmailAndMobile,
      NotificationKey = "14uijna46i7cz",
      QuestionnaireType = Enums.QuestionnaireType.CompleteSelfT1
    };
    string expectedErrorMessage = "ConsentToShare = true";
    string[] expectedMemberNames = { "Email", "Mobile" };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.Results[0].ErrorMessage.Should().Contain(expectedErrorMessage);
      result.Results[0].MemberNames.Should().HaveCount(2);
      result.Results[0].MemberNames.Should().Contain(expectedMemberNames);
    }
  }

  [Fact]
  public void MissingPropertiesWithConsentFalse_Valid()
  {
    // Arrange.
    string expectedAnswers = VALID_ANSWERS_START + 
      "{\"QuestionId\":7," +
      "\"a\":\"false\"}]";

    CompleteQuestionnaire model = new()
    {
      Answers = expectedAnswers,
      NotificationKey = "14uijna46i7cz",
      QuestionnaireType = Enums.QuestionnaireType.CompleteSelfT1
    };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();

      model.Answers.Should().Be(expectedAnswers);
      model.ConsentToShare.Should().BeFalse();
      model.Email.Should().BeNull();
      model.FamilyName.Should().BeNull();
      model.GivenName.Should().BeNull();
      model.Mobile.Should().BeNull();
    }
  }

  [Fact]
  public void Valid()
  {
    // Arrange.
    CompleteQuestionnaire model = new()
    {
      Answers = VALID_ANSWERS,
      NotificationKey = "14uijna46i7cz",
      QuestionnaireType = Enums.QuestionnaireType.CompleteSelfT1
    };

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    using (new AssertionScope())
    {
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();

      model.Answers.Should().Be(VALID_ANSWERS);
      model.ConsentToShare.Should().Be(TEST_ANSWERS_EXPECTED_CONSENT);
      model.Email.Should().Be(TEST_ANSWERS_EXPECTED_EMAIL);
      model.FamilyName.Should().Be(TEST_ANSWERS_EXPECTED_FAMILY_NAME);
      model.GivenName.Should().Be(TEST_ANSWERS_EXPECTED_GIVEN_NAME);
      model.Mobile.Should().Be(TEST_ANSWERS_EXPECTED_MOBILE);
    }
  }
}
