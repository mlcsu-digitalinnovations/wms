using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services
{
  [Collection("Service collection")]
  public class DeprivationServiceTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly IDeprivationService _service;
    private readonly DeprivationOptions _options = new();

    public DeprivationServiceTests(ServiceFixture serviceFixture) : base(serviceFixture)
    {
      IConfiguration config = new ConfigurationBuilder()
        .AddInMemoryCollection(TestConfig())
        .Build();

      config.GetSection(DeprivationOptions.SectionKey).Bind(_options);

      _context = new DatabaseContext(_serviceFixture.Options);

      _service = new DeprivationService(
        Options.Create(_options),
        _context,
        _serviceFixture.Mapper,
        _log)
      {
        User = GetClaimsPrincipal()
      };


      //add depravations
      Entities.Deprivation dep1 = new()
      {
        ImdDecile = 4,
        Lsoa = "Z0101244"
      };
      Entities.Deprivation dep2 = new()
      {
        ImdDecile = 5,
        Lsoa = "Z01030030"
      };
      Entities.Deprivation dep3 = new()
      {
        ImdDecile = 1,
        Lsoa = "Z01004754"
      };

      _context.Deprivations.Add(dep1);
      _context.Deprivations.Add(dep2);
      _context.Deprivations.Add(dep3);
      _context.SaveChangesAsync();
    }

    public static Dictionary<string, string> TestConfig()
    {
      string key = DeprivationOptions.SectionKey;
      return new Dictionary<string, string>{

        { $"{key}:{nameof(DeprivationOptions.ImdResourceUrl)}",
          "https://assets.publishing.service.gov.uk" +
          "/government/uploads/system" +
          "/uploads/attachment_data/file/833970/" + 
          "File_1_-_IMD2019_Index_of_Multiple_Deprivation.xlsx" },

        { $"{key}:{nameof(DeprivationOptions.Col1)}", "LSOA code (2011)" },

        { $"{key}:{nameof(DeprivationOptions.Col2)}", 
          "Index of Multiple Deprivation (IMD) Decile" }
      };
    }

    [Fact]
    public async Task EtlImdFile()
    {
      // ARRANGE
      _context.Deprivations.RemoveRange(_context.Deprivations);
      string expectedLsoa = "E01000001";
      DateTimeOffset methodCallTime = DateTimeOffset.Now;

      // ACT
      await _service.EtlImdFile();

      // assert
      var savedDeprivation = await _context.Deprivations
        .SingleOrDefaultAsync(d => d.Lsoa == expectedLsoa);

      savedDeprivation.ImdDecile.Should().BeInRange(1, 10);
      savedDeprivation.IsActive.Should().BeTrue();
      savedDeprivation.ModifiedAt.Should().BeAfter(methodCallTime);
      savedDeprivation.ModifiedByUserId.Should().Be(_service.User.GetUserId());
    }

    [Fact]
    public async Task GetByLsoa_ArgumentNullException()
    {
      await Assert.ThrowsAsync<ArgumentNullException>(
        () => _service.GetByLsoa(null));
    }

    [Fact]
    public async Task GetByLsoa()
    {
      //ARRANGE
      _context.Deprivations.RemoveRange(_context.Deprivations);

      Entities.Deprivation expectedDeprivation = new()
      {
        IsActive = true,
        ModifiedAt = System.DateTime.Now,
        ModifiedByUserId = System.Guid.NewGuid(),
        ImdDecile = 3,
        Lsoa = "E01012446"
      };
      _context.Deprivations.Add(expectedDeprivation);
      await _context.SaveChangesAsync();

      // act
      var result = await _service.GetByLsoa(expectedDeprivation.Lsoa);

      // ASSERT
      result.Should().NotBeNull();
      result.ImdDecile.Should().Be(expectedDeprivation.ImdDecile);
      result.Lsoa.Should().Be(expectedDeprivation.Lsoa);
    }

    [Fact]
    public async Task GetByLsoa_DeprivationNotLoaded()
    {
      // ARRANGE
      string lsoa = "Z0101999";
      _context.Deprivations.RemoveRange(_context.Deprivations);
      await _context.SaveChangesAsync();
      string expectedMessage = "Deprivations have not been loaded.";

      // ACT / ASSERT
      var ex = await Assert.ThrowsAsync<DeprivationNotFoundException>(
        () => _service.GetByLsoa(lsoa));

      ex.Message.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData("Z0101244")]
    [InlineData("Z01030030")]
    [InlineData("Z01004754")]
    public async Task GetByLsoa_NotFound(string lsoa)
    {
      // ARRANGE
      string expectedMessage = "Deprivation with a lsoa code "
        + $"of {lsoa} not found.";

      // ACT / ASSERT
      var ex = await Assert.ThrowsAsync<DeprivationNotFoundException>(
        () => _service.GetByLsoa(lsoa));

      ex.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task RefreshDeprivations_ArgumentNullException()
    {
      await Assert.ThrowsAsync<ArgumentNullException>(
        () => _service.RefreshDeprivations(null));
    }

    [Fact]
    public async Task RefreshDeprivations()
    {
      // ARRANGE
      var methodCallTime = DateTimeOffset.Now;
      List<Deprivation> expectedDeprivations = new()
      {
        new Deprivation { Lsoa = "TESTLSOA1", ImdDecile = 10 },
        new Deprivation { Lsoa = "TESTLSOA2", ImdDecile = 5 },
      };

      // ACT
      await _service.RefreshDeprivations(expectedDeprivations);

      // ASSERT
      _context.Deprivations.Count().Should().Be(expectedDeprivations.Count);

      expectedDeprivations.ForEach(expectedDeprivation =>
      {
        var savedDeprivation =
          _context.Deprivations.Single(d => d.Lsoa == expectedDeprivation.Lsoa);

        savedDeprivation.ImdDecile.Should().Be(expectedDeprivation.ImdDecile);
        savedDeprivation.IsActive.Should().BeTrue();
        savedDeprivation.ModifiedAt.Should().BeAfter(methodCallTime);
        savedDeprivation.ModifiedByUserId.Should()
          .Be(_service.User.GetUserId());
      });
    }

  }
}
