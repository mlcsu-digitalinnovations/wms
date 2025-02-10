using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;
using Xunit;

namespace WmsHub.Common.Tests.Attributes;

public class JsonStringAttributeTests
{
  [Fact]
  public void IsValidJson()
  {
    // Arrange.
    JsonStringAttribute jsonStringAttribute = new();

    // Act.
    bool result = jsonStringAttribute.IsValid(
      "[{ \"Answer\": \"answer 1\", \"Question\": \"question 1\" }]");

    // Assert.
    result.Should().BeTrue();
  }

  [Fact]
  public void IsInValidJson()
  {
    // Arrange.
    JsonStringAttribute jsonStringAttribute = new();

    // Act.
    bool result = jsonStringAttribute.IsValid(
      "[{ \"Answer\": \"answer 1\", \"Question\": \"question 1\", \"Q\" }]");

    // Assert.
    result.Should().BeFalse();
  }
}
