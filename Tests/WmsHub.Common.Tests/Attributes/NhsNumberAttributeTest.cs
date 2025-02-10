using System;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;
using Xunit;

namespace WmsHub.Common.Tests.Attributes
{
  public class NhsNumberAttributeTest
  {
    [Theory]
    [InlineData("1234567890", false)]
    [InlineData("484-694-9656", false)]
    [InlineData("484 694 9656", false)]
    [InlineData("1234", false)]
    [InlineData("9998069998", true)]
    [InlineData("9995209993", true)]
    [InlineData("9993239992", true)]
    [InlineData("9998729998", true)]
    [InlineData("9992109998", true)]
    [InlineData("9999659993", true)]
    [InlineData("9994659995", true)]
    [InlineData("9993989991", true)]
    [InlineData("9992469994", true)]
    public void TestIsNhsNumber(string nhsNumber, bool expected)
    {
      //arrange
      NhsNumberAttribute testClass = new NhsNumberAttribute();
      //act
      bool result = testClass.IsValid(nhsNumber);
      //assert
      Assert.Equal(expected, result);
    }

    [Fact]
    public void NhsGeneratorTest()
    {
      //arrange
      Random random = new Random();
      NhsNumberAttribute testClass = new NhsNumberAttribute();
      //act
      string nhsNumber = Generators.GenerateNhsNumber(random);
      bool result = testClass.IsValid(nhsNumber);
      //assert
      Assert.True(result);
    }
  }
}
