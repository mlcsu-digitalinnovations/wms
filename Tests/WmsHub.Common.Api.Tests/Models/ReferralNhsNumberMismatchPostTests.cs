using FluentAssertions;
using System.Linq;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Validation;
using Xunit;

namespace WmsHub.Common.Api.Tests.Models
{
  public class ReferralNhsNumberMismatchPostTests : ReferralPostBaseTests
  {

    public const string VALID_NHSNUMBERATTACHMENT = "9996699994";
    public const string VALID_NHSNUMBERWORKLIST = "9990759995";

    protected override ReferralPostBase CreateBaseModel(
      string ubrn = VALID_UBRN, string serviceId = VALID_SERVICEID)
    {
      return new ReferralNhsNumberMismatchPost
      {
        Ubrn = ubrn,
        ServiceId = serviceId,
        NhsNumberAttachment = VALID_NHSNUMBERATTACHMENT,
        NhsNumberWorkList = VALID_NHSNUMBERWORKLIST
      };
    }

    protected ReferralNhsNumberMismatchPost CreateModel(
      string nhsNumberAttachment = VALID_NHSNUMBERATTACHMENT,
      string nhsNumberWorkList = VALID_NHSNUMBERWORKLIST,
      string ubrn = VALID_UBRN)
    {
      return new ReferralNhsNumberMismatchPost
      {
        Ubrn = ubrn,
        NhsNumberAttachment = nhsNumberAttachment,
        NhsNumberWorkList = nhsNumberWorkList
      };
    }

    [Theory]
    [InlineData(
      "123456789",
      "The NhsNumber must be 10 numbers only, remove any spaces or dashes.", 
      "NhsNumberAttachment too short")]
    [InlineData(
      "12345678901",
      "The NhsNumber must be 10 numbers only, remove any spaces or dashes.",
      "NhsNumberAttachment too long")]
    public void Invalid_NhsNumberAttachment(
      string nhsNumberAttachment, string expected, string because)
    {
      // arrange
      ReferralNhsNumberMismatchPost modelToTest = 
        CreateModel(nhsNumberAttachment: nhsNumberAttachment);
      // act
      ValidateModelResult result = ValidateModel(modelToTest);
      // assert
      result.IsValid.Should().BeFalse(because);
      result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
    }
  }
}

