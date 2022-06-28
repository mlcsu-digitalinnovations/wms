using FluentAssertions;
using WmsHub.Business.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Common
{
  public class VulnerableDescriptionHelperTests
  {
    [Theory]
    [InlineData("This is a valid date of 21/09/2012.", 
      "This is a valid date of XX/XX/XXXX.")]
    [InlineData("This is a valid date of 1960-01-01.",
      "This is a valid date of XXXX-XX-XX.")]
    [InlineData("This is a valid date of 1960-01-01 12:41.",
      "This is a valid date of XXXX-XX-XX xx:xx.")]
    [InlineData("This is a valid UBRN of 123456789123.",
      "This is a valid UBRN of xxxxxxxxxxxx.")]
    [InlineData("This is a valid UBRN of SR3456789123.",
      "This is a valid UBRN of xxxxxxxxxxxx.")]
    [InlineData("This is a valid NHS Number of 1234567890.",
      "This is a valid NHS Number of xxxxxxxxxx.")]
    [InlineData("This is a valid NHS Number of 123 456 7890.",
      "This is a valid NHS Number of xxx-xxx-xxxx.")]
    [InlineData("This is a valid NHS Number of 123-456-7890.",
      "This is a valid NHS Number of xxx-xxx-xxxx.")]
    [InlineData("This is a valid NHS Number of 123/456/7890.",
      "This is a valid NHS Number of xxx-xxx-xxxx.")]
    [InlineData(
      "This is a complex sentence with date 1 as 21/07/1972 and from the DB " +
      "using GetDate() 2021-08-26 12:23:02.350.  A UBRN od PH0000000001 and " +
      "two NHS numbers the same 999 828 2999 and 9998282999.",
      "This is a complex sentence with date 1 as XX/XX/XXXX and from the DB " +
      "using GetDate() XXXX-XX-XX xx:xx:xx.xxx.  A UBRN od xxxxxxxxxxxx and " +
      "two NHS numbers the same xxx-xxx-xxxx and xxxxxxxxxx.")]
    [InlineData("",null)]
    [InlineData(null, null)]
    public void Valid(string input, string output)
    {
      //arrange

      //act
      string result = input.TryParseToAnonymous();
      //assert
      result.Should().Be(output);
    }
  }
}
