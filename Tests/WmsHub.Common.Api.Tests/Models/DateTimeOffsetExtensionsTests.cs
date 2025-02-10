using FluentAssertions;
using FluentAssertions.Execution;
using System;
using WmsHub.Common.Extensions;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Common.Tests;

public class DateTimeOffsetExtensionsTests : ATheoryData
{
  [Theory]
  [MemberData(nameof(DatesOfBirth))]
  public void GetAgeReturnValid(string date, int expectedAge)
  {
    // Arrange.
    DateTimeOffset dateToTest = DateTimeOffset.Parse(date);
    // Act.
    int age = dateToTest.GetAge();
    // Assert.
    age.Should().Be(expectedAge);
  }

  public class GetNullableAgeTests : DateTimeOffsetExtensionsTests
  {
    [Theory]
    [MemberData(nameof(DatesBool))]
    public void GetNullableAgeReturnValid(string date, bool isNull)
    {
      // Arrange.
      DateTimeOffset.TryParse(date, out DateTimeOffset result);

      DateTimeOffset? dateToTest = isNull ? null : result;

      int? expectedAge = null;
      if (!isNull)
      {
        expectedAge =
          new DateTime(DateTime
            .Now
            .Subtract(dateToTest.Value.Date)
            .Ticks)
          .Year - 1;
      }

      // Act.
      int? age = dateToTest.GetAge();

      // Assert.
      using (new AssertionScope())
      {
        if (isNull)
        {
          age.Should().BeNull();
        }
        else
        {
          age.Should().NotBeNull();
        }

        age.Should().Be(expectedAge);
      }
    }
  }
}