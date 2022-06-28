using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services
{
  public class PostcodeServiceTests : ServiceTestsBase
  {
    private readonly IPostcodeService _service;
    private readonly IOptions<PostcodeOptions> _options =
      TestConfiguration.CreatePostcodeOptions();

    public PostcodeServiceTests(ITestOutputHelper testOutputHelper)
      : base(null, testOutputHelper)
    {
      _service = new PostcodeService(_options, _log);
    }

    public class GetLsoa : PostcodeServiceTests
    {

      public GetLsoa(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
      { }

      [Theory]
      [InlineData("SY3 9NS", "E01028962")]
      [InlineData("ST4 4LZ", "E01014277")]
      public async void ValidPostcodes(
        string validPostcode, string expectedLsoa)
      {
        // ACT
        string retrievedLsoa = await _service.GetLsoa(validPostcode);

        // assert
        retrievedLsoa.Should().Be(expectedLsoa);
      }

      [Theory]
      [InlineData("FOO BAR")]
      [InlineData("XX11111XX")]
      public async void InvalidPostcodes(string invalidPostcode)
      {
        // ACT
        Func<Task> act = async () => await _service.GetLsoa(invalidPostcode);

        // ASSERT
        await act.Should().ThrowAsync<PostcodeNotFoundException>()
          .WithMessage($"Postcode {invalidPostcode} not found.");
      }

      [Theory]
      [InlineData(null)]
      [InlineData("")]
      public async void ThrowArgumentNullOrWhiteSpaceException(
        string nullPostcode)
      {
        // ARRANGE
        string expectedMessage = 
          new PostcodeNotFoundException("Postcode is null or white space.")
            .Message;

        // ACT
        Func<Task> act = async () => await _service.GetLsoa(nullPostcode);

        // ASSERT
        await act.Should().ThrowAsync<PostcodeNotFoundException>()
          .WithMessage(expectedMessage);
      }
    }
  }
}
