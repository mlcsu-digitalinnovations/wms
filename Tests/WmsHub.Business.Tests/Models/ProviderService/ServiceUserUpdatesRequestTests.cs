using FluentAssertions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Common.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Models.ProviderService;
public class ServiceUserUpdatesRequestTests
{
  public class ValidateTests : ServiceUserUpdatesRequestTests
  {

    [Fact]
    public void FutureDateReturnsValidationResult()
    {
      // Arrange.
      DateTime? date = DateTime.UtcNow.AddDays(1);
      string expectedErrorMessage = $"The Date field '{date}' is invalid. " +
        "Update cannot be dated in the future.";

      ServiceUserUpdatesRequest request = new()
      {
        Coaching = 0,
        Date = date,
        Measure = 0,
        Weight = Constants.MAX_WEIGHT_KG
      };

      // Act.
      IEnumerable<ValidationResult> validationResults = request
        .Validate(new ValidationContext(this));

      // Assert.
      validationResults.Should().HaveCount(1).And.Subject
        .Single().ErrorMessage.Should().Be(expectedErrorMessage);
    }

    [Theory]
    [MemberData(nameof(ValidDateData))]
    public void ValidDateReturnsEmptyCollection(DateTime? date)
    {
      // Arrange.
      ServiceUserUpdatesRequest request = new()
      {
        Coaching = 0,
        Date = date,
        Measure = 0,
        Weight = Constants.MAX_WEIGHT_KG
      };

      // Act.
      IEnumerable<ValidationResult> validationResults = request
        .Validate(new ValidationContext(this));

      // Assert.
      validationResults.Should().BeEmpty();
    }

    public static TheoryData<DateTime?> ValidDateData()
    {
      TheoryData<DateTime?> theoryData = [];
      theoryData.Add(null);
      theoryData.Add(DateTime.UtcNow);
      theoryData.Add(DateTime.UtcNow.AddDays(-1));
      theoryData.Add(DateTime.UtcNow.Date.AddSeconds(86399));
      return theoryData;
    }
  }
}
