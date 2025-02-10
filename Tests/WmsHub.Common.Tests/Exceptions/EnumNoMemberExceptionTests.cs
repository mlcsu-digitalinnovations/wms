using FluentAssertions;
using System;
using WmsHub.Business.Enums;
using WmsHub.Common.Exceptions;
using Xunit;

namespace WmsHub.Common.Tests.Exceptions;
public class EnumNoMemberExceptionTests
{
  [Fact]
  public void ConstructorWithTypeParameterCreatesCorrectMessage()
  {
    // Arrange.
    Type enumType = typeof(ReferralStatus);

    // Act.
    EnumNoMemberException exception = new(enumType);

    // Assert.
    exception.Message.Should().Be("ReferralStatus has no members.");
  }
}
