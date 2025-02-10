using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Services;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class LinkIdServiceTests : ServiceTestsBase, IDisposable
{
  protected const string BatchSizeConfigurationId = "WmsHub_LinkIdService_BatchSizeToGenerate";
  protected const int DefaultBatchSize = 10;
  protected const int DefaultIdLength = 12;
  protected const string IdLengthConfigurationId = "WmsHub_LinkIdService_IdLength";

  private protected readonly DatabaseContext _context;
  private protected readonly LinkIdService _linkIdService;

  public LinkIdServiceTests(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper) 
    : base(serviceFixture, testOutputHelper)
  {
    _context = new(serviceFixture.Options);
    _linkIdService = new(_context);

    AddConfigurationValues();
  }

  public void Dispose()
  {
    _context.LinkIds.RemoveRange(_context.LinkIds);
    _context.ConfigurationValues.RemoveRange(_context.ConfigurationValues);
    _context.SaveChanges();
    GC.SuppressFinalize(this);
  }

  private void AddConfigurationValues()
  {
    ConfigurationValue batchSizeConfigurationValue = new()
    {
      Id = BatchSizeConfigurationId,
      Value = DefaultBatchSize.ToString(CultureInfo.InvariantCulture)
    };

    ConfigurationValue idLengthConfigurationValue = new()
    {
      Id = IdLengthConfigurationId,
      Value = DefaultIdLength.ToString(CultureInfo.InvariantCulture)
    };

    _context.ConfigurationValues.Add(batchSizeConfigurationValue);
    _context.ConfigurationValues.Add(idLengthConfigurationValue);
    _context.SaveChanges();
  }

  public class GenerateDummyIdTests : LinkIdServiceTests
  {
    public GenerateDummyIdTests(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
    }

    [InlineData(8)]
    [InlineData(16)]
    [Theory]
    public void LengthInRangeGeneratesId(int length)
    {
      // Arrange.

      // Act.
      string id = LinkIdService.GenerateDummyId(length);

      // Assert.
      id.Should().NotBeNullOrWhiteSpace().And.HaveLength(length);
    }

    [InlineData(0)]
    [InlineData(201)]
    [Theory]
    public void LengthOutOfRangeThrowsException(int length)
    {
      // Arrange.

      // Act.
      Func<string> result = () => LinkIdService.GenerateDummyId(length);

      // Assert.
      result.Should().Throw<ArgumentOutOfRangeException>();
    }
  }

  public class GenerateNewIdsAsyncTests : LinkIdServiceTests
  {
    public GenerateNewIdsAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task ValidConfigurationGeneratesNewUnusedIds()
    {
      // Arrange.

      // Act.
      await _linkIdService.GenerateNewIdsAsync(DefaultBatchSize);

      // Assert.
      _context.LinkIds.Distinct().Should().HaveCount(DefaultBatchSize);
      _context.LinkIds.Where(x => x.IsUsed).Any().Should().BeFalse();
      _context.LinkIds.First().Id.Should().HaveLength(DefaultIdLength);
    }

    [Fact]
    public async Task MissingIdLengthConfigThrowsException()
    {
      // Arrange.
      _context.ConfigurationValues.RemoveRange(_context.ConfigurationValues);
      await _context.SaveChangesAsync();

      // Act.
      Func<Task> result = () => _linkIdService.GenerateNewIdsAsync(10);

      // Assert.
      await result.Should().ThrowAsync<InvalidOptionsException>();
    }
  }

  public class GetUnusedLinkIdAsyncTests : LinkIdServiceTests
  {
    public GetUnusedLinkIdAsyncTests(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task ReturnsSingleId()
    {
      // Arrange.
      string id = "a2b3c5d6e7f8";
      _context.LinkIds.Add(new() { Id = id, IsUsed = false });
      await _context.SaveChangesAsync();

      // Act.
      string returnedId = await _linkIdService.GetUnusedLinkIdAsync();
      
      // Assert.
      returnedId.Should().Be(id);
      _context.LinkIds.Where(x => x.Id == id).Single().IsUsed.Should().BeTrue();
    }
  }

  public class GetUnusedLinkIdBatchAsyncTests : LinkIdServiceTests
  {
    public GetUnusedLinkIdBatchAsyncTests(
      ServiceFixture serviceFixture, 
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task NoGenerationRequiredReturnsRequestedIds()
    {
      // Arrange.
      int numberToReturn = 2;

      string id1 = "a2b3c5d6e7f8";
      string id2 = "g9h2i3j4k5m6";
      string id3 = "n7p8q9r2s3t4";

      _context.LinkIds.Add(new() { Id = id1, IsUsed = true });
      _context.LinkIds.Add(new() { Id = id2, IsUsed = false });
      _context.LinkIds.Add(new() { Id = id3, IsUsed = false });
      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<string> ids = await _linkIdService.GetUnusedLinkIdBatchAsync(numberToReturn);

      // Assert.
      ids.Should().HaveCount(numberToReturn).And.Contain(id2).And.Contain(id3);
      _context.LinkIds.Where(x => x.Id == id2).Single().IsUsed.Should().BeTrue();
      _context.LinkIds.Where(x => x.Id == id3).Single().IsUsed.Should().BeTrue();
    }

    [InlineData(DefaultBatchSize + 2, DefaultBatchSize + 1)]
    [InlineData(DefaultBatchSize - 5, DefaultBatchSize)]
    [Theory]
    public async Task GenerationRequiredGeneratesIdsAndReturnsRequiredUnused(
      int requested,
      int generated)
    {
      // Arrange.
      string id1 = "a2b3c5d6e7f8";

      _context.LinkIds.Add(new() { Id = id1, IsUsed = false });
      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<string> ids = await _linkIdService.GetUnusedLinkIdBatchAsync(requested);

      // Assert.
      ids.Distinct().Should().HaveCount(requested);
      _context.LinkIds.Should().HaveCount(generated + 1)
        .And.Subject.Where(x => ids.Contains(x.Id)).Where(x => !x.IsUsed).Any().Should().BeFalse();
    }

    [Fact]
    public async Task IsRunningThrowsProcessAlreadyRunningException()
    {
      // Arrange.
      int retries = 3;
      Stopwatch stopwatch = new();
      ConfigurationValue isRunningConfigurationValue = new()
      {
        Id = "WmsHub_LinkIdService_IsRunning:GetUnusedLinkIdBatchAsync",
        Value = true.ToString()
      };

      _context.ConfigurationValues.Add(isRunningConfigurationValue);
      await _context.SaveChangesAsync();

      // Act.
      
      Func<Task<IEnumerable<string>>> result = 
        () => _linkIdService.GetUnusedLinkIdBatchAsync(10, retries);
      
      // Assert.
      stopwatch.Start();
      await result.Should().ThrowAsync<ProcessAlreadyRunningException>();
      stopwatch.Stop();
      stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(1000 * retries);
    }

    [Fact]
    public async Task MissingBatchSizeToGenerateConfigurationValueThrowsException()
    {
      // Arrange.
      ConfigurationValue batchSizeConfigurationValue =
        _context.ConfigurationValues.Where(x => x.Id == BatchSizeConfigurationId).Single();
      _context.ConfigurationValues.Remove(batchSizeConfigurationValue);
      await _context.SaveChangesAsync();

      // Act.
      Func<Task<IEnumerable<string>>> result = () => _linkIdService.GetUnusedLinkIdBatchAsync(1);

      // Await.
      await result.Should().ThrowAsync<InvalidOptionsException>();
    }
  }
}
