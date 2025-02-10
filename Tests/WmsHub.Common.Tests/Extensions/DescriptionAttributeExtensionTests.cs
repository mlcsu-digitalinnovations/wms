using FluentAssertions;
using System;
using System.ComponentModel;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using Xunit;

namespace WmsHub.Common.Tests.Extensions;
public class DescriptionAttributeExtensionTests
{
  protected const string Description = "Enum description";

  public class GetDescriptionAttributeValueTests : DescriptionAttributeExtensionTests
  {
    [Fact]
    public void NoDescriptionReturnsNull()
    {
      // Arrange.
      EnumWithoutDescriptions enumWithoutDescriptions = 
        EnumWithoutDescriptions.FirstWithoutDescription;

      // Act.
      string description = enumWithoutDescriptions.GetDescriptionAttributeValue();

      // Assert.
      description.Should().BeNull();
    }

    [Fact]
    public void ValidDescriptionAttributeReturnsDescription()
    {
      // Arrange.
      EnumWithDescriptions enumWithDescriptions = EnumWithDescriptions.FirstWithDescription;

      // Act.
      string description = enumWithDescriptions.GetDescriptionAttributeValue();

      // Assert.
      description.Should().Be(Description);
    }
  }

  public class TryParseEnumFromDescriptionTests : DescriptionAttributeExtensionTests
  { 

    [Fact]
    public void EnumWithNoMembersThrowsException()
    {
      // Arrange.

      // Act.
      Func<bool> result = () => Description.TryParseEnumFromDescription(
        out EnumWithNoMembers outputEnum);

      // Assert.
      result.Should().Throw<EnumNoMemberException>();
    }

    [Fact]
    public void MatchingStringReturnsTrueAndOutputsEnumMember()
    {
      // Arrange.

      // Act.
      bool parsedSuccessfully = Description.TryParseEnumFromDescription(
        out EnumWithDescriptions outputEnum);

      // Assert.
      parsedSuccessfully.Should().BeTrue();
      outputEnum.Should().Be(EnumWithDescriptions.FirstWithDescription);
    }

    [Fact]
    public void NonMatchingStringReturnsFalseAndOutputsDefault()
    {
      // Arrange.
      string nonMatchingDescription = "Non-matching";

      // Assert.
      bool parsedSuccessfully = nonMatchingDescription.TryParseEnumFromDescription(
        out EnumWithDescriptions outputEnum);

      // Act.
      parsedSuccessfully.Should().BeFalse();
      outputEnum.Should().Be(default);
    }
  }

  protected enum EnumWithDescriptions
  {
    [Description(Description)]
    FirstWithDescription
  }

  protected enum EnumWithoutDescriptions
  {
    FirstWithoutDescription
  }

  protected enum EnumWithNoMembers { }
}
