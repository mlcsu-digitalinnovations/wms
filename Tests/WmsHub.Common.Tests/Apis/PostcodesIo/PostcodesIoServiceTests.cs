using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using WmsHub.Common.Apis.Ods.PostcodesIo;
using Xunit;

namespace WmsHub.Common.Tests.PostcodesIo;

public class PostcodesIoServiceTests
{
  private readonly IPostcodesIoService _postcodeIoService;

  public PostcodesIoServiceTests()
  {
    ServiceCollection services = new();
    ConfigurationBuilder configurationBuilder = new();

    IConfiguration configuration = configurationBuilder.Build();

    services.AddPostcodesIoService(configuration);

    ServiceProvider serviceProvider = services.BuildServiceProvider();

    _postcodeIoService = serviceProvider
      .GetRequiredService<IPostcodesIoService>();
  }

  public class GetLsoaAsync : PostcodesIoServiceTests
  {
    [Theory]
    [InlineData("SY3 9NS", "E01028962")]
    [InlineData("st44lz", "E01014277")]
    [InlineData("LL45 2PL", "W01000079")]
    [InlineData("ph494jg", "S01010526")]
    public async Task ValidPostcode_ExpectedLsoa(
      string validPostcode, 
      string expectedLsoa)
    {
      // Act.
      string lsoa = await _postcodeIoService.GetLsoaAsync(validPostcode);

      // Assert.
      lsoa.Should().Be(expectedLsoa);
    }

    [Theory]
    [InlineData("FOO BAR")]
    [InlineData("XX11111XX")]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidPostcode_NullLsoa(string invalidPostcode)
    {
      // Act.
      string lsoa = await _postcodeIoService.GetLsoaAsync(invalidPostcode);

      // Assert.
      lsoa.Should().BeNull();
    }
  }

  public class IsEnglishPostcodeAsync : PostcodesIoServiceTests
  {
    [Theory]
    [InlineData("SY3 9NS")]
    [InlineData("st44lz")]
    public async Task ValidPostcode_English_True(string postcode)
    {
      // Act.
      bool IsEnglish = await _postcodeIoService
        .IsEnglishPostcodeAsync(postcode);

      // Assert.
      IsEnglish.Should().BeTrue();
    }

    [Theory]
    [InlineData("LL45 2PL")]
    [InlineData("ph494jg")]
    public async Task ValidPostcode_NonEnglish_False(string postcode)
    {
      // Act.
      bool IsEnglish = await _postcodeIoService
        .IsEnglishPostcodeAsync(postcode);

      // Assert.
      IsEnglish.Should().BeFalse();
    }


    [Theory]
    [InlineData("FOO BAR")]
    [InlineData("XX11111XX")]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidPostcode_False(string invalidPostcode)
    {
      // Act.
      bool IsEnglish = await _postcodeIoService
        .IsEnglishPostcodeAsync(invalidPostcode);

      // Assert.
      IsEnglish.Should().BeFalse();
    }
  }

  public class IsUkOutwardCodeAsync : PostcodesIoServiceTests
  {
    [Theory]
    [InlineData("SY3")]
    [InlineData("ST4")]
    [InlineData("LL45")]
    [InlineData("ph49")]

    public async Task ValidOutwardCode_Uk_True(string outwardCode)
    {
      // Act.
      bool IsUk = await _postcodeIoService.IsUkOutwardCodeAsync(outwardCode);

      // Assert.
      IsUk.Should().BeTrue();
    }

    [Theory]
    [InlineData("FOO BAR")]
    [InlineData("XX11111XX")]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidOutwardCode_False(string outwardCode)
    {
      // Act.
      bool IsUk = await _postcodeIoService.IsUkOutwardCodeAsync(outwardCode);

      // Assert.
      IsUk.Should().BeFalse();
    }
  }

  public class IsUkPostcodeAsync : PostcodesIoServiceTests
  {
    [Theory]
    [InlineData("SY3 9NS")]
    [InlineData("ST44LZ")]
    [InlineData("LL45 2PL")]
    [InlineData("ph494jg")]

    public async Task ValidPostcode_Uk_True(string postcode)
    {
      // Act.
      bool IsUk = await _postcodeIoService.IsUkPostcodeAsync(postcode);

      // Assert.
      IsUk.Should().BeTrue();
    }

    [Theory]
    [InlineData("FOO BAR")]
    [InlineData("XX11111XX")]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidPostcode_False(string invalidPostcode)
    {
      // Act.
      bool IsUk = await _postcodeIoService.IsUkPostcodeAsync(invalidPostcode);

      // Assert.
      IsUk.Should().BeFalse();
    }
  }
}
