using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Helpers;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class ReferralAdminServiceTests : ServiceTestsBase, IDisposable
{
  protected readonly DatabaseContext _context;
  protected readonly ReferralAdminService _service;
  protected readonly Mock<ReferralService> _mockReferralService;
  private readonly GpDocumentProxyOptions _gpDocumentProxyOptions = new();
  private readonly Mock<IOptions<GpDocumentProxyOptions>> _mockGpDocumentProxyOptions = new();
  private readonly Mock<IOptions<ReferralTimelineOptions>> _mockReferralTimelineOptions = new();
  protected readonly Random _random = new();

  public ReferralAdminServiceTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
    _context = new DatabaseContext(_serviceFixture.Options);

    _ = _mockGpDocumentProxyOptions.Setup(x => x.Value)
        .Returns(_gpDocumentProxyOptions);

    _mockReferralService = new Mock<ReferralService>(
      _context,
      _serviceFixture.Mapper,
      null,
      null,
      null,
      null,
      null,
      null,
      _mockGpDocumentProxyOptions.Object,
      _mockReferralTimelineOptions.Object,
      null,
      _log);

    _service = new ReferralAdminService(
      _context,
      _mockReferralService.Object,
      _serviceFixture.Mapper)
    {
      User = GetClaimsPrincipal()
    };

    CleanUp();
  }

  public virtual void Dispose()
  {
    // clean up
    CleanUp();
    GC.SuppressFinalize(this);
  }

  private void CleanUp()
  {
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.ReferralsAudit.RemoveRange(_context.ReferralsAudit);
    _context.MskOrganisations.RemoveRange(_context.MskOrganisations);
    _context.SaveChanges();
  }

  public class ChangeDateOfBirthAsyncTests : ReferralAdminServiceTests
  {
    public ChangeDateOfBirthAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task UbrnNullOrEmpty_Exception(string ubrn)
    {
      // Arrange.
      DateTime originalDateOfBirth = DateTimeOffset.Now.Date
        .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
      DateTime updatedDateOfBirth = originalDateOfBirth.AddYears(1);
      string expectedExceptionMessage = new
        ArgumentNullOrWhiteSpaceException(nameof(ubrn)).Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ChangeDateOfBirthAsync(
          ubrn, originalDateOfBirth, updatedDateOfBirth));

      // Assert.
      _ = ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public async Task UpdatedAndOriginalDateOfBirthIdentical_Exception()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(_random);
      DateTime originalDateOfBirth = DateTimeOffset.Now.Date
        .AddYears(Constants.MAX_GP_REFERRAL_AGE);
      DateTime updatedDateOfBirth = originalDateOfBirth;
      string expectedExceptionMessage = new ArgumentException(
        $"Value must be different from {nameof(originalDateOfBirth)}.",
        nameof(updatedDateOfBirth)).Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ChangeDateOfBirthAsync(
          ubrn, originalDateOfBirth, updatedDateOfBirth));

      // Assert.
      _ = ex.Should().BeOfType<ArgumentException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Theory]
    [InlineData(Constants.MIN_GP_REFERRAL_AGE - 1)]
    [InlineData(Constants.MAX_GP_REFERRAL_AGE + 1)]
    public async Task UpdatedDateOfBirthOutOfRange_Exception(
      int updatedDateOfBirthAge)
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(_random);
      DateTime updatedDateOfBirth = DateTimeOffset.Now.Date.AddYears(
        -updatedDateOfBirthAge);
      DateTime originalDateOfBirth = updatedDateOfBirth.AddYears(1);
      string expectedExceptionMessage = $"The {nameof(updatedDateOfBirth)} " +
        $"must result in a service user's age between " +
        $"{Constants.MIN_GP_REFERRAL_AGE} and " +
        $"{Constants.MAX_GP_REFERRAL_AGE}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ChangeDateOfBirthAsync(
          ubrn, originalDateOfBirth, updatedDateOfBirth));

      // Assert.
      _ = ex.Should().BeOfType<AgeOutOfRangeException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public async Task ReferralNotFoundByUbrn_Exception()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(_random);
      DateTime originalDateOfBirth = DateTimeOffset.Now.Date
        .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
      DateTime updatedDateOfBirth = originalDateOfBirth.AddYears(-1);
      string expectedExceptionMessage = $"Unable to find a referral with a " +
        $"UBRN of {ubrn}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ChangeDateOfBirthAsync(
          ubrn, originalDateOfBirth, updatedDateOfBirth));

      // Assert.
      _ = ex.Should().BeOfType<ReferralNotFoundException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public async Task ReferalFoundButNotWithOriginalDateOfBirth_Exception()
    {
      // Arrange.
      DateTime originalDateOfBirth = DateTimeOffset.Now.Date
        .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
      DateTime updatedDateOfBirth = originalDateOfBirth.AddYears(-1);

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfBirth: originalDateOfBirth.AddYears(-1));
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();

      string expectedExceptionMessage = $"Unable to find a referral with a " +
        $"UBRN of {referral.Ubrn} and a date of birth of " +
        $"{originalDateOfBirth:yyyy-MM-dd}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ChangeDateOfBirthAsync(
          referral.Ubrn, originalDateOfBirth, updatedDateOfBirth));

      // Assert.
      _ = ex.Should().BeOfType<ReferralNotFoundException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public async Task ReferralHasNotBeenTriaged_NotReTriaged()
    {
      // Arrange.
      DateTime originalDateOfBirth = DateTimeOffset.Now.Date
        .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
      DateTime updatedDateOfBirth = originalDateOfBirth.AddYears(-1);

      Referral createdReferral = RandomEntityCreator.CreateRandomReferral(
        dateOfBirth: originalDateOfBirth,
        offeredCompletionLevel: null,
        triagedCompletionLevel: null);
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();
      _context.ChangeTracker.Clear();

      _ = _mockReferralService.Setup(
        s => s.UpdateTriage(It.IsAny<Referral>()));

      // Act.
      string result = await _service.ChangeDateOfBirthAsync(
          createdReferral.Ubrn, originalDateOfBirth, updatedDateOfBirth);

      Referral updatedReferral = _context.Referrals
        .Single(r => r.Id == createdReferral.Id);

      // Assert.
      _mockReferralService
        .Verify(s => s.UpdateTriage(It.IsAny<Referral>()), Times.Never);

      _ = updatedReferral.Should().BeEquivalentTo(updatedReferral, opts => opts
        .Excluding(r => r.Audits)
        .Excluding(r => r.DateOfBirth)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId));

      _ = updatedReferral.DateOfBirth.Should().Be(updatedDateOfBirth);
      _ = updatedReferral.ModifiedAt.Should()
        .BeAfter(createdReferral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
    }

    [Fact]
    public async Task ReferralHasBeenTriagedProviderSelected_NotReTriaged()
    {
      // Arrange.
      DateTime originalDateOfBirth = DateTimeOffset.Now.Date
        .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
      DateTime updatedDateOfBirth = originalDateOfBirth.AddYears(-1);

      Referral createdReferral = RandomEntityCreator.CreateRandomReferral(
        dateOfBirth: originalDateOfBirth,
        offeredCompletionLevel: $"{(int)TriageLevel.Low}",
        providerId: Guid.NewGuid(),
        triagedCompletionLevel: $"{(int)TriageLevel.Low}");
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();
      _context.Entry(createdReferral).State = EntityState.Detached;

      // Act.
      string result = await _service.ChangeDateOfBirthAsync(
          createdReferral.Ubrn, originalDateOfBirth, updatedDateOfBirth);

      Referral updatedReferral = _context.Referrals
        .Single(r => r.Id == createdReferral.Id);

      // Assert.
      _mockReferralService
        .Verify(s => s.UpdateTriage(It.IsAny<Referral>()), Times.Never);

      _ = updatedReferral.Should().BeEquivalentTo(createdReferral, opts => opts
        .Excluding(r => r.Audits)
        .Excluding(r => r.DateOfBirth)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId));

      _ = updatedReferral.DateOfBirth.Should().Be(updatedDateOfBirth);
      _ = updatedReferral.ModifiedAt.Should()
        .BeAfter(createdReferral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
    }

    [Fact]
    public async Task ReferralHasBeenTriagedProviderNotSelected_ReTriaged()
    {
      // Arrange.
      DateTime originalDateOfBirth = DateTimeOffset.Now.Date
        .AddYears(-Constants.MIN_GP_REFERRAL_AGE);
      DateTime updatedDateOfBirth = originalDateOfBirth.AddYears(-1);

      Referral createdReferral = RandomEntityCreator.CreateRandomReferral(
        dateOfBirth: originalDateOfBirth,
        offeredCompletionLevel: $"{(int)TriageLevel.Low}",
        triagedCompletionLevel: $"{(int)TriageLevel.Low}");
      createdReferral.ProviderId = null;
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();
      _context.Entry(createdReferral).State = EntityState.Detached;

      _ = _mockReferralService.Setup(
        x => x.UpdateTriage(It.IsAny<Referral>()));

      // Act.
      string result = await _service.ChangeDateOfBirthAsync(
          createdReferral.Ubrn, originalDateOfBirth, updatedDateOfBirth);

      Referral updatedReferral = _context.Referrals
        .Single(r => r.Id == createdReferral.Id);

      // Assert.
      _mockReferralService
        .Verify(s => s.UpdateTriage(It.IsAny<Referral>()), Times.Once);

      _ = updatedReferral.Should().BeEquivalentTo(createdReferral, opts => opts
        .Excluding(r => r.Audits)
        .Excluding(r => r.DateOfBirth)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId));

      _ = updatedReferral.DateOfBirth.Should().Be(updatedDateOfBirth);
      _ = updatedReferral.ModifiedAt.Should()
        .BeAfter(createdReferral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
    }
  }

  public class ChangeMobileAsyncTests : ReferralAdminServiceTests
  {
    public ChangeMobileAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task UbrnNullOrEmpty_Exception(string ubrn)
    {
      // Arrange.
      string originalMobile = "+447111111111";
      string updatedMobile = "+447222222222";
      string expectedExceptionMessage = new
        ArgumentNullOrWhiteSpaceException(nameof(ubrn)).Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ChangeMobileAsync(
          ubrn, originalMobile, updatedMobile));

      // Assert.
      _ = ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public async Task UpdatedAndOriginalMobileIdentical_Exception()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(_random);
      string originalMobile = "+447111111111";
      string updatedMobile = "+447111111111";
      string expectedExceptionMessage = new ArgumentException(
        $"Value must be different from {nameof(originalMobile)}.",
        nameof(updatedMobile)).Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ChangeMobileAsync(
          ubrn, originalMobile, updatedMobile));

      // Assert.
      _ = ex.Should().BeOfType<ArgumentException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("+697715427599")]
    [InlineData("+44771542759")]
    [InlineData("+4477154275999")]
    public async Task UpdatedMobileOutOfRange_Exception(
      string updatedMobile)
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(_random);
      string originalMobile = "+447111111111";
      string expectedExceptionMessage = new ArgumentOutOfRangeException(
        "updatedMobile",
        $"Value is not a valid UK mobile number.")
        .Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ChangeMobileAsync(
          ubrn, originalMobile, updatedMobile));

      // Assert.
      _ = ex.Should().BeOfType<ArgumentOutOfRangeException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public async Task ReferralNotFoundByUbrn_Exception()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(_random);
      string originalMobile = "+447111111111";
      string updatedMobile = "+447222222222";
      string expectedExceptionMessage = $"Unable to find a referral with a " +
        $"UBRN of {ubrn}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ChangeMobileAsync(
          ubrn, originalMobile, updatedMobile));

      // Assert.
      _ = ex.Should().BeOfType<ReferralNotFoundException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public async Task ReferalFoundButNotWithOriginalMobile_Exception()
    {
      // Arrange.
      string originalMobile = "+447111111111";
      string updatedMobile = "+447222222222";

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        mobile: "+447333333333");
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();

      string expectedExceptionMessage = $"Unable to find a referral with a " +
        $"UBRN of {referral.Ubrn} and a mobile of {originalMobile}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ChangeMobileAsync(
          referral.Ubrn, originalMobile, updatedMobile));

      // Assert.
      _ = ex.Should().BeOfType<ReferralNotFoundException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public async Task ReferralFound_MobileUpdated()
    {
      // Arrange.
      string originalMobile = "+447111111111";
      string updatedMobile = "+447222222222";

      Referral createdReferral = RandomEntityCreator.CreateRandomReferral(
        isMobileValid: true,
        mobile: originalMobile);
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();
      _context.Entry(createdReferral).State = EntityState.Detached;

      // Act.
      string result = await _service.ChangeMobileAsync(
          createdReferral.Ubrn, originalMobile, updatedMobile);

      Referral updatedReferral = _context.Referrals
        .Single(r => r.Id == createdReferral.Id);

      // Assert.
      _ = updatedReferral.Should().BeEquivalentTo(createdReferral, opts => opts
        .Excluding(r => r.Audits)
        .Excluding(r => r.IsMobileValid)
        .Excluding(r => r.Mobile)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId));

      _ = updatedReferral.IsMobileValid.Should().BeNull();
      _ = updatedReferral.Mobile.Should().Be(updatedMobile);
      _ = updatedReferral.ModifiedAt.Should()
        .BeAfter(createdReferral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
    }
  }

  public class ChangeNhsNumberAsyncTests : ReferralAdminServiceTests
  {
    private delegate Task<string> MethodUnderTest(
      string ubrn,
      string originalNhsNumber,
      string updatedNhsNumber);

    private readonly MethodUnderTest methodUnderTest;

    public ChangeNhsNumberAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      methodUnderTest = _service.ChangeNhsNumberAsync;
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task UbrnNullOrEmpty_Exception(string ubrn)
    {
      // Arrange.
      string originalNhsNumber = "";
      string updatedNhsNumber = "";
      string expectedExceptionMessage =
        new ArgumentNullOrWhiteSpaceException(nameof(ubrn)).Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => methodUnderTest(ubrn, originalNhsNumber, updatedNhsNumber));

      // Assert.
      using (new AssertionScope())
      {
        _ = ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>();
        _ = ex.Message.Should().Be(expectedExceptionMessage);
      }
    }

    [Fact]
    public async Task UpdatedAndOriginalNhsNumberIdentical_Exception()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(_random);
      string originalNhsNumber = "9993879991";
      string updatedNhsNumber = originalNhsNumber;
      string expectedExceptionMessage = new ArgumentException(
        $"Value must be different from {nameof(originalNhsNumber)}.",
        nameof(updatedNhsNumber)).Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => methodUnderTest(ubrn, originalNhsNumber, updatedNhsNumber));

      // Assert.
      using (new AssertionScope())
      {
        _ = ex.Should().BeOfType<ArgumentException>();
        _ = ex.Message.Should().Be(expectedExceptionMessage);
      }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("99938799911")]
    [InlineData("999387999")]
    [InlineData("9993879999")]
    public async Task UpdatedNhsNumberInvalid_Exception(
      string updatedNhsNumber)
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(_random);
      string originalNhsNumber = "9993879991";
      string expectedExceptionMessage = new ArgumentOutOfRangeException(
        "updatedNhsNumber",
        $"Value is not a valid NHS number.")
        .Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => methodUnderTest(ubrn, originalNhsNumber, updatedNhsNumber));

      // Assert.
      using (new AssertionScope())
      {
        _ = ex.Should().BeOfType<ArgumentOutOfRangeException>();
        _ = ex.Message.Should().Be(expectedExceptionMessage);
      }
    }

    [Fact]
    public async Task ReferralNotFoundByUbrn_Exception()
    {
      // Arrange.
      string ubrn = Generators.GenerateUbrn(_random);
      string originalNhsNumber = "9993879991";
      string updatedNhsNumber = "9994929992";
      string expectedExceptionMessage = $"Unable to find a referral with a " +
        $"UBRN of {ubrn}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => methodUnderTest(ubrn, originalNhsNumber, updatedNhsNumber));

      // Assert.
      using (new AssertionScope())
      {
        _ = ex.Should().BeOfType<ReferralNotFoundException>();
        _ = ex.Message.Should().Be(expectedExceptionMessage);
      }
    }

    [Fact]
    public async Task ReferalFoundButNotWithOriginalNhsNumber_Exception()
    {
      // Arrange.
      string originalNhsNumber = "9993879991";
      string updatedNhsNumber = "9994929992";

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        nhsNumber: "9994919997");
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();

      string expectedExceptionMessage = $"Unable to find a referral with a " +
        $"UBRN of {referral.Ubrn} and a NHS number of {originalNhsNumber}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => methodUnderTest(
          referral.Ubrn,
          originalNhsNumber,
          updatedNhsNumber));

      // Assert.
      using (new AssertionScope())
      {
        _ = ex.Should().BeOfType<ReferralNotFoundException>();
        _ = ex.Message.Should().Be(expectedExceptionMessage);
      }

      _ = _context.Remove(referral);
      _ = _context.SaveChanges();
    }

    [Fact]
    public async Task NhsNumberMatchesInvalidReferral_Exception()
    {
      // Arrange.
      string originalNhsNumber = "9993879991";
      string updatedNhsNumber = "9994929992";
      string expectedExceptionMessage =
        "Referral selected provider without date of provider selection.";

      _ = _mockReferralService
        .Setup(x => x.CheckReferralCanBeCreatedWithNhsNumberAsync(
          It.IsAny<string>()))
        .ThrowsAsync(new InvalidOperationException(expectedExceptionMessage));

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => methodUnderTest(
          "UBRN",
          originalNhsNumber,
          updatedNhsNumber));

      // Assert.
      using (new AssertionScope())
      {
        _ = ex.Should().BeOfType<InvalidOperationException>();
        _ = ex.Message.Should().Be(expectedExceptionMessage);
      }
    }

    [Fact]
    public async Task NhsNumberCannotBeUsedToCreateAReferral_Exception()
    {
      // Arrange.
      string originalNhsNumber = "9993879991";
      string updatedNhsNumber = "9994929992";
      string expectedExceptionMessage = "Referral Not Found";

      _ = _mockReferralService
        .Setup(x => x.CheckReferralCanBeCreatedWithNhsNumberAsync(
          It.IsAny<string>()))
        .ThrowsAsync(new ReferralNotFoundException(expectedExceptionMessage));

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => methodUnderTest(
          "UBRN",
          originalNhsNumber,
          updatedNhsNumber));

      // Assert.
      using (new AssertionScope())
      {
        _ = ex.Should().BeOfType<ReferralNotFoundException>();
        _ = ex.Message.Should().Be(expectedExceptionMessage);
      }
    }

    [Fact]
    public async Task ReferralFound_NhsNumberUpdated()
    {
      // Arrange.
      string originalNhsNumber = "9993879991";
      string updatedNhsNumber = "9994929992";

      Referral createdReferral = RandomEntityCreator.CreateRandomReferral(
        nhsNumber: originalNhsNumber);
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();
      _context.Entry(createdReferral).State = EntityState.Detached;

      // Act.
      string result = await methodUnderTest(
          createdReferral.Ubrn, originalNhsNumber, updatedNhsNumber);

      // Assert.
      Referral updatedReferral = _context.Referrals.Find(createdReferral.Id);

      using (new AssertionScope())
      {
        _ = updatedReferral.Should().BeEquivalentTo(
          createdReferral,
          opts => opts
            .Excluding(r => r.Audits)
            .Excluding(r => r.NhsNumber)
            .Excluding(r => r.ModifiedAt)
            .Excluding(r => r.ModifiedByUserId));

        _ = updatedReferral.NhsNumber.Should().Be(updatedNhsNumber);
        _ = updatedReferral.ModifiedAt.Should()
          .BeAfter(createdReferral.ModifiedAt);
        _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }
    }
  }

  public class ChangeSexAsyncTests : ReferralAdminServiceTests
  {
    private const string MatchingIdAsString = "00000000-0000-0000-0000-000000000001";
    private const string NonMatchingIdAsString = "00000000-0000-0000-0000-000000000002";
    private const string NonMatchingOriginalSex = "Female";
    private const string NonMatchingUbrn = "987654321010";
    private const string ValidOriginalSex = "Male";
    private const string ValidUbrn = "123456789010";
    private const string ValidUpdatedSex = "Not Specified";

    public ChangeSexAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task InvalidIdThrowsArgumentOutOfRangeException()
    {
      // Arrange.
      Guid id = Guid.Empty;

      // Act.
      Func<Task<string>> result = () => _service.ChangeSexAsync(
        id,
        ValidOriginalSex,
        ValidUbrn,
        ValidUpdatedSex);

      // Assert.
      await result.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task InvalidUpdatedSexThrowsArgumentException()
    {
      // Arrange.
      Guid id = Guid.NewGuid();
      string invalidUpdatedSex = "abcxyz";

      // Act.
      Func<Task<string>> result = () => _service.ChangeSexAsync(
        id,
        ValidOriginalSex,
        ValidUbrn,
        invalidUpdatedSex);

      // Assert.
      await result.Should().ThrowAsync<ArgumentException>()
        .WithMessage($"*{invalidUpdatedSex}*");
    }

    [Fact]
    public async Task MatchingOriginalSexAndUpdatedSexThrowsArgumentException()
    {
      // Arrange.
      Guid id = Guid.NewGuid();

      // Act.
      Func<Task<string>> result = () => _service.ChangeSexAsync(
        id,
        ValidOriginalSex,
        ValidUbrn,
        ValidOriginalSex);

      // Assert.
      await result.Should().ThrowAsync<ArgumentException>()
        .WithMessage("*originalSex*updatedSex*");
    }

    [Theory]
    [InlineData(NonMatchingIdAsString, ValidOriginalSex, ValidUbrn)]
    [InlineData(MatchingIdAsString, ValidOriginalSex, NonMatchingUbrn)]
    [InlineData(MatchingIdAsString, NonMatchingOriginalSex, ValidUbrn)]
    public async Task NonMatchingParametersThrowsReferralNotFoundException(
      string idAsString,
      string originalSex,
      string ubrn)
    {
      // Arrange.
      Guid submittedId = Guid.Parse(idAsString);
      Guid referralId = Guid.Parse(MatchingIdAsString);
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: referralId,
        ubrn: ValidUbrn,
        sex: ValidOriginalSex);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      Func<Task<string>> result = () => _service.ChangeSexAsync(
        submittedId,
        originalSex,
        ubrn,
        ValidUpdatedSex);

      // Assert.
      await result.Should().ThrowAsync<ReferralNotFoundException>();
    }
    
    [Theory]
    [InlineData(" ", ValidUbrn, ValidUpdatedSex)]
    [InlineData(ValidOriginalSex, null, ValidUpdatedSex )]
    [InlineData(ValidOriginalSex, " ", ValidUpdatedSex)]
    [InlineData(ValidOriginalSex, ValidUbrn, null)]
    [InlineData(ValidOriginalSex, ValidUbrn, " ")]
    public async Task NullOrWhiteSpaceParametersThrowsArgumentException(
      string originalSex,
      string ubrn,
      string updatedSex)
    {
      // Arrange.
      Guid id = Guid.NewGuid();

      // Act.
      Func<Task<string>> result = () => _service.ChangeSexAsync(id, originalSex, ubrn, updatedSex);

      // Assert.
      await result.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(ValidOriginalSex)]
    public async Task ValidParametersUpdatesReferralAndReturnsConfirmationString(
      string originalSex)
    {
      // Arrange.
      Guid referralId = Guid.Parse(MatchingIdAsString);
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: referralId,
        ubrn: ValidUbrn);
      referral.Sex = originalSex;

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      string result = await _service.ChangeSexAsync(
        referralId,
        originalSex,
        ValidUbrn,
        ValidUpdatedSex);

      // Assert.
      Referral updatedReferral = _context.Referrals
        .AsNoTracking()
        .SingleOrDefault(r => r.Id == referralId);
      updatedReferral.Should().NotBeNull()
        .And.BeOfType<Referral>()
        .Subject.Sex.Should().Be(ValidUpdatedSex);
      result.Should().Match($"*{MatchingIdAsString}*{ValidUpdatedSex}*");
    }
  }

  public class FixNumbersAsyncTests : ReferralAdminServiceTests
  {
    public FixNumbersAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task UbrnNullOrEmpty_Exception(string ubrn)
    {
      // Arrange.
      string expectedExceptionMessage = new
        ArgumentNullOrWhiteSpaceException(nameof(ubrn)).Message;

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.FixNumbersAsync(ubrn));

      // Assert.
      _ = ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>();
      _ = ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public async Task MobileIsValidTelephoneIsValid_UpdateIsValidFields()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        mobile: MOBILE_E164,
        telephone: TELEPHONE_E164);
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;

      // Act.
      string result = await _service.FixNumbersAsync(referral.Ubrn);

      // Assert.
      _ = result.Should().Be($"Mobile for UBRN {referral.Ubrn} was not " +
        $"updated from {referral.Mobile}. Telephone for UBRN " +
        $"{referral.Ubrn} was not updated from {referral.Telephone}.");

      Referral updatedReferral = _context.Referrals
        .Single(x => x.Ubrn == referral.Ubrn);
      _ = updatedReferral.Should().BeEquivalentTo(referral, opt => opt
        .Excluding(x => x.Audits)
        .Excluding(x => x.IsMobileValid)
        .Excluding(x => x.IsTelephoneValid)
        .Excluding(x => x.ModifiedAt)
        .Excluding(x => x.ModifiedByUserId));
      _ = updatedReferral.Audits.Should().HaveCount(1);
      _ = updatedReferral.IsMobileValid.Should().BeTrue();
      _ = updatedReferral.IsTelephoneValid.Should().BeTrue();
      _ = updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
    }

    [Fact]
    public async Task MobileIsValidTelephoneIsInvalid_UpdateIsValidFields()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        mobile: MOBILE_E164,
        telephone: TELEPHONE_INVALID_SHORT);
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;

      // Act.
      string result = await _service.FixNumbersAsync(referral.Ubrn);

      // Assert.
      _ = result.Should().Be($"Mobile for UBRN {referral.Ubrn} was not " +
        $"updated from {referral.Mobile}. Telephone for UBRN " +
        $"{referral.Ubrn} was not updated from {referral.Telephone}.");

      Referral updatedReferral = _context.Referrals
        .Single(x => x.Ubrn == referral.Ubrn);
      _ = updatedReferral.Should().BeEquivalentTo(referral, opt => opt
        .Excluding(x => x.Audits)
        .Excluding(x => x.IsMobileValid)
        .Excluding(x => x.IsTelephoneValid)
        .Excluding(x => x.ModifiedAt)
        .Excluding(x => x.ModifiedByUserId));
      _ = updatedReferral.Audits.Should().HaveCount(1);
      _ = updatedReferral.IsMobileValid.Should().BeTrue();
      _ = updatedReferral.IsTelephoneValid.Should().BeFalse();
      _ = updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
    }

    [Fact]
    public async Task MobileIsInvalidTelephoneIsValid_UpdateIsValidFields()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        mobile: MOBILE_INVALID_SHORT,
        telephone: TELEPHONE_E164);
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;

      // Act.
      string result = await _service.FixNumbersAsync(referral.Ubrn);

      // Assert.
      _ = result.Should().Be($"Mobile for UBRN {referral.Ubrn} was not " +
        $"updated from {referral.Mobile}. Telephone for UBRN " +
        $"{referral.Ubrn} was not updated from {referral.Telephone}.");

      Referral updatedReferral = _context.Referrals
        .Single(x => x.Ubrn == referral.Ubrn);
      _ = updatedReferral.Should().BeEquivalentTo(referral, opt => opt
        .Excluding(x => x.Audits)
        .Excluding(x => x.IsMobileValid)
        .Excluding(x => x.IsTelephoneValid)
        .Excluding(x => x.ModifiedAt)
        .Excluding(x => x.ModifiedByUserId));
      _ = updatedReferral.Audits.Should().HaveCount(1);
      _ = updatedReferral.IsMobileValid.Should().BeFalse();
      _ = updatedReferral.IsTelephoneValid.Should().BeTrue();
      _ = updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
    }

    [Fact]
    public async Task MobileIsTelephoneIsMobile_SwitchValues()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        mobile: TELEPHONE_E164,
        telephone: MOBILE_E164);
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;

      // Act.
      string result = await _service.FixNumbersAsync(referral.Ubrn);

      // Assert.
      _ = result.Should().Be($"Mobile for UBRN {referral.Ubrn} updated " +
        $"from {referral.Mobile} to {referral.Telephone}. " +
        $"Telephone for UBRN {referral.Ubrn} updated from " +
        $"{referral.Telephone} to {referral.Mobile}.");

      Referral updatedReferral = _context.Referrals
        .Single(x => x.Ubrn == referral.Ubrn);
      _ = updatedReferral.Should().BeEquivalentTo(referral, opt => opt
        .Excluding(x => x.Audits)
        .Excluding(x => x.IsMobileValid)
        .Excluding(x => x.IsTelephoneValid)
        .Excluding(x => x.Mobile)
        .Excluding(x => x.ModifiedAt)
        .Excluding(x => x.ModifiedByUserId)
        .Excluding(x => x.Telephone));
      _ = updatedReferral.Audits.Should().HaveCount(1);
      _ = updatedReferral.IsMobileValid.Should().BeTrue();
      _ = updatedReferral.IsTelephoneValid.Should().BeTrue();
      _ = updatedReferral.Mobile.Should().Be(MOBILE_E164);
      _ = updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      _ = updatedReferral.Telephone.Should().Be(TELEPHONE_E164);
    }
  }

  public class DeleteCancelledGpReferralAsyncTests
    : ReferralAdminServiceTests
  {
    private const string VALID_UBRN = "123456789012";
    private const string UBRN_NOT_FOUND = "999999999999";
    private const string VALID_REASON = "A valid reason for deletion.";

    public DeleteCancelledGpReferralAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task ReasonNullOrWhiteSpace_ArgumentException(string reason)
    {
      // Act.
      Task result = _service
        .DeleteCancelledGpReferralAsync(VALID_UBRN, reason);

      // Assert.
      _ = await Assert.ThrowsAsync<ArgumentException>(() => result);
    }

    [Fact]
    public async Task UbrnNotExists_ReferralNotFoundException()
    {
      // Arrange.
      _ = CreateAndDetachReferral(
        UBRN_NOT_FOUND,
        ReferralStatus.CancelledByEreferrals,
        ReferralSource.GpReferral);

      // Act.
      Task result = _service
        .DeleteCancelledGpReferralAsync(VALID_UBRN, VALID_REASON);

      // Assert.
      _ = await Assert.ThrowsAsync<ReferralNotFoundException>(() => result);
    }

    [Theory]
    [MemberData(nameof(InvalidStatuses))]
    public async Task InvalidStatus_ReferralInvalidStatusException(
      ReferralStatus invalidReferralStatus)
    {
      // Arrange.
      _ = CreateAndDetachReferral(
        VALID_UBRN,
        invalidReferralStatus,
        ReferralSource.GpReferral);

      // Act.
      Task result = _service
        .DeleteCancelledGpReferralAsync(VALID_UBRN, VALID_REASON);

      // Assert.
      _ = await Assert.ThrowsAsync<ReferralInvalidStatusException>(
        () => result);
    }

    [Theory]
    [MemberData(nameof(InvalidReferralSources))]
    public async Task
      InvalidReferralSource_ReferralInvalidReferralSourceException(
        ReferralSource invalidReferralSource)
    {
      // Arrange.
      _ = CreateAndDetachReferral(
        VALID_UBRN,
        ReferralStatus.CancelledByEreferrals,
        invalidReferralSource);

      // Act.
      Task result = _service
        .DeleteCancelledGpReferralAsync(VALID_UBRN, VALID_REASON);

      // Assert.
      _ = await Assert
        .ThrowsAsync<ReferralInvalidReferralSourceException>(() => result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Existing_Cancelled_GpReferral_Updated(bool addProviderSubmissions)
    {
      // Arrange.
      Referral expectedReferral = CreateAndDetachReferral(
        VALID_UBRN,
        ReferralStatus.CancelledByEreferrals,
        ReferralSource.GpReferral,
        addProviderSubmissions);

      // Act.
      string result = await _service.DeleteCancelledGpReferralAsync(VALID_UBRN, VALID_REASON);

      Referral updatedReferral = _context.Referrals
        .Where(r => r.Ubrn == expectedReferral.Ubrn)
        .Single();

      // Assert.
      _ = result.Should().Be($"The referral with a UBRN of '{expectedReferral.Ubrn}' was deleted.");

      _ = updatedReferral.Should().BeEquivalentTo(expectedReferral, opt => opt
        .Excluding(r => r.Audits)
        .Excluding(r => r.IsActive)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.ProviderSubmissions)
        .Excluding(r => r.StatusReason));

      _ = updatedReferral.IsActive.Should().BeFalse();
      _ = updatedReferral.ModifiedAt.Should().BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      _ = updatedReferral.StatusReason.Should().Be(VALID_REASON);

      updatedReferral.ProviderSubmissions.ForEach(ps => 
      {
        _ = ps.IsActive.Should().BeFalse();
        _ = ps.ModifiedAt.Should().BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
        _ = ps.ModifiedByUserId.Should().Be(TEST_USER_ID);
      });
    }

    public static TheoryData<ReferralSource> InvalidReferralSources
    {
      get
      {
        TheoryData<ReferralSource> invalidReferralSources = new();

        foreach (object value in Enum.GetValues(typeof(ReferralSource)))
        {
          if ((ReferralSource)value != ReferralSource.GpReferral)
          {
            invalidReferralSources.Add((ReferralSource)value);
          }
        }

        return invalidReferralSources;
      }
    }

    public static TheoryData<ReferralStatus> InvalidStatuses
    {
      get
      {
        TheoryData<ReferralStatus> invalidStatuses = new();

        foreach (object value in Enum.GetValues(typeof(ReferralStatus)))
        {
          if ((ReferralStatus)value != ReferralStatus.CancelledByEreferrals)
          {
            invalidStatuses.Add((ReferralStatus)value);
          }
        }

        return invalidStatuses;
      }
    }

    private Referral CreateAndDetachReferral(
      string ubrn,
      ReferralStatus referralStatus,
      ReferralSource referralSource,
      bool addProviderSubmissions = false)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        modifiedAt: new DateTimeOffset(new DateTime(1900, 1, 1)),
        referralSource: referralSource,
        status: referralStatus,
        ubrn: ubrn);

      if (addProviderSubmissions)
      {
        referral.ProviderSubmissions = [
          RandomEntityCreator.CreateProviderSubmission(),
          RandomEntityCreator.CreateProviderSubmission()
        ];
      }

      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;

      return referral;
    }
  }

  public class DeleteReferralAsyncTests : ReferralAdminServiceTests
  {
    private const string VALID_UBRN = "123456789012";
    private const string UBRN_NOT_FOUND = "999999999999";
    private const string VALID_REASON = "A valid reason for deletion.";

    public DeleteReferralAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReferralNull_ArgumentNullException()
    {
      // Act.
      Exception ex = await Record
        .ExceptionAsync(() => _service.DeleteReferralAsync(null));

      // Assert.
      _ = ex.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task UbrnDoesNotMatch_ReferralNotFoundException()
    {
      // Arrange.
      Referral createdReferral = RandomEntityCreator.CreateRandomReferral();
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();

      Business.Models.Referral referralToDelete = new()
      {
        Id = createdReferral.Id,
        Status = createdReferral.Status,
        Ubrn = "DoesNotMatch"
      };

      await ReferralNotFoundExceptionActAndAssert(referralToDelete);
    }

    [Fact]
    public async Task IdDoesNotMatch_ReferralNotFoundException()
    {
      // Arrange.
      Referral createdReferral = RandomEntityCreator.CreateRandomReferral();
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();

      Business.Models.Referral referralToDelete = new()
      {
        Id = Guid.NewGuid(),
        Status = createdReferral.Status,
        Ubrn = createdReferral.Ubrn
      };

      await ReferralNotFoundExceptionActAndAssert(referralToDelete);
    }

    [Fact]
    public async Task StatusDoesNotMatch_ReferralNotFoundException()
    {
      // Arrange.
      Referral createdReferral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New);
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();

      Business.Models.Referral referralToDelete = new()
      {
        Id = createdReferral.Id,
        Status = ReferralStatus.RmcCall.ToString(),
        Ubrn = createdReferral.Ubrn
      };

      await ReferralNotFoundExceptionActAndAssert(referralToDelete);
    }

    private async Task ReferralNotFoundExceptionActAndAssert(
      Business.Models.Referral referralToDelete)
    {
      string expectedMessage = "Unable to find an active referral with " +
        $"an id of {referralToDelete.Id}, " +
        $"status of {referralToDelete.Status} and ubrn of " +
        $"{referralToDelete.Ubrn}.";

      // Act.
      Exception ex = await Record
        .ExceptionAsync(() => _service.DeleteReferralAsync(referralToDelete));

      // Assert.
      _ = ex.Should().BeOfType<ReferralNotFoundException>();
      _ = ex.Message.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Success(bool addProviderSubmissions)
    {
      // Arrange.
      Referral createdReferral = RandomEntityCreator.CreateRandomReferral();
      if (addProviderSubmissions)
      {
        createdReferral.ProviderSubmissions = [
          RandomEntityCreator.CreateProviderSubmission(),
          RandomEntityCreator.CreateProviderSubmission()
        ];
      }

      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();
      _context.ChangeTracker.Clear();

      Business.Models.Referral referralToDelete = new()
      {
        Id = createdReferral.Id,
        Status = createdReferral.Status,
        Ubrn = createdReferral.Ubrn
      };

      string expectedResult =
        $"The referral with an id of {referralToDelete.Id} was deleted.";

      // Act.
      string result = await _service.DeleteReferralAsync(referralToDelete);
      Referral updatedReferral = _context.Referrals
        .Single(r => r.Id == referralToDelete.Id);

      // Assert.
      _ = result.Should().Be(expectedResult);

      _ = updatedReferral.Should().BeEquivalentTo(createdReferral, opts => opts
        .Excluding(r => r.Audits)
        .Excluding(r => r.IsActive)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.ProviderSubmissions));

      _ = updatedReferral.IsActive.Should().BeFalse();
      _ = updatedReferral.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);

      updatedReferral.ProviderSubmissions.ForEach(ps =>
      {
        _ = ps.IsActive.Should().BeFalse();
        _ = ps.ModifiedAt.Should().BeAfter(createdReferral.ModifiedAt);
        _ = ps.ModifiedByUserId.Should().Be(TEST_USER_ID);
      });
    }
  }

  public class FixNonGpReferralsWithStatusProviderCompletedAsyncTests
    : ReferralAdminServiceTests
  {
    public FixNonGpReferralsWithStatusProviderCompletedAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
    { }

    /// <summary>
    /// Creates a referral with a status of ProviderCompleted for each 
    /// ReferralSource except Undefined and GpReferral and tests that 
    /// that their status is updated to Complete
    /// </summary>
    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), new ReferralSource[] {
        ReferralSource.GpReferral})]
    public async Task NonGpReferralSource_ProviderCompleted_UpdatedToComplete(
      ReferralSource referralSource)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource,
        status: ReferralStatus.ProviderCompleted);

      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();

      _context.Entry(referral).State = EntityState.Detached;

      // Act.
      string result = await _service
        .FixNonGpReferralsWithStatusProviderCompletedAsync();

      // Assert.
      _ = result.Should().Be($"1 non-GP referral(s) had " +
      $"their status updated from {ReferralStatus.ProviderCompleted} to " +
      $"{ReferralStatus.Complete}.");

      Referral updatedReferral = _context.Referrals
        .Single(r => r.Id == referral.Id);

      _ = updatedReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.Status));

      _ = updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      _ = updatedReferral.ModifiedByUserId.Should()
        .NotBe(referral.ModifiedByUserId);
      _ = updatedReferral.Status.Should()
        .Be(ReferralStatus.Complete.ToString());
      _ = updatedReferral.Status.Should().NotBe(referral.Status);
    }

    /// <summary>
    /// Creates a referral for each ReferralSource except Undefined and 
    /// GpReferral for all ReferralStatuses except ProviderCompleted and
    /// tests that that they are not updated
    /// </summary>
    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), new ReferralSource[] {
        ReferralSource.GpReferral})]
    public async Task NonGpReferralSource_NotProviderCompleted_NotUpdated(
        ReferralSource referralSource)
    {
      List<Referral> referrals = new();

      List<ReferralStatus> referralStatuses = Enum
        .GetValues(typeof(ReferralStatus))
        .Cast<ReferralStatus>()
        .Where(s => s != ReferralStatus.ProviderCompleted)
        .ToList();

      // Arrange.
      foreach (ReferralStatus status in referralStatuses)
      {
        referrals.Add(RandomEntityCreator.CreateRandomReferral(
          referralSource: referralSource,
          status: status));
      }

      _context.Referrals.AddRange(referrals);
      _ = _context.SaveChanges();

      referrals.ForEach(r => _context.Entry(r).State = EntityState.Detached);

      // Act.
      string result = await _service
        .FixNonGpReferralsWithStatusProviderCompletedAsync();

      // Assert.
      _ = result.Should().Be($"0 non-GP referral(s) had " +
      $"their status updated from {ReferralStatus.ProviderCompleted} to " +
      $"{ReferralStatus.Complete}.");

      List<Referral> updatedReferrals = _context.Referrals
        .Where(r => referrals.Select(x => x.Id).Contains(r.Id))
        .ToList();

      foreach (Referral updatedReferral in updatedReferrals)
      {
        Referral referral = referrals.Single(r => r.Id == updatedReferral.Id);
        _ = updatedReferral.Should().BeEquivalentTo(referral, opt => opt
          .Excluding(r => r.Audits));
      }
    }

    /// <summary>
    /// Creates a referral for each ReferralStatus with a ReferralSource of 
    /// GpReferral and tests that none are updated.
    /// </summary>
    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData))]
    public async Task GpReferralSource_NotUpdated(
      ReferralStatus referralStatus)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.GpReferral,
        status: referralStatus);

      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();

      _context.Entry(referral).State = EntityState.Detached;

      // Act.
      string result = await _service
        .FixNonGpReferralsWithStatusProviderCompletedAsync();

      // Assert.
      _ = result.Should().Be($"0 non-GP referral(s) had " +
      $"their status updated from {ReferralStatus.ProviderCompleted} to " +
      $"{ReferralStatus.Complete}.");

      Referral updatedReferral = _context.Referrals
        .Single(r => r.Id == referral.Id);

      _ = updatedReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits));
    }
  }

  public class FixReferralProviderUbrnAsyncTests : ReferralAdminServiceTests
  {
    public FixReferralProviderUbrnAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task UpdatedIgnoredAndExcluded()
    {
      // Arrange.
      Referral referralToUpdate = RandomEntityCreator.CreateRandomReferral(
        providerUbrn: "000000000001",
        ubrn: "GP0000000001");
      _ = _context.Referrals.Add(referralToUpdate);

      Referral referralToExclude = RandomEntityCreator.CreateRandomReferral(
        ubrn: "MSK000000001");
      referralToExclude.ProviderUbrn = null;
      _ = _context.Referrals.Add(referralToExclude);

      Referral referralToIgnore = RandomEntityCreator.CreateRandomReferral(
        providerUbrn: "GP0000000002",
        ubrn: "000000000002");
      _ = _context.Referrals.Add(referralToIgnore);

      _ = await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      // Act.
      object result = await _service.FixReferralProviderUbrnAsync();

      // Assert.
      _ = result.ToString().Should().Be("Updated 1 referrals. Ignored 1");

      Referral referralUpdated = _context.Referrals
        .Single(r => r.Id == referralToUpdate.Id);
      _ = referralUpdated.Should().BeEquivalentTo(referralToUpdate, ops => ops
        .Excluding(r => r.Audits)
        .Excluding(r => r.ProviderUbrn)
        .Excluding(r => r.Ubrn));
      _ = referralUpdated.Audits.Should().HaveCount(1);
      _ = referralUpdated.ProviderUbrn.Should().Be(referralToUpdate.Ubrn);
      _ = referralUpdated.Ubrn.Should().Be(referralToUpdate.ProviderUbrn);

      Referral referralIgnored = _context.Referrals
        .Single(r => r.Id == referralToIgnore.Id);
      _ = referralIgnored.Should().BeEquivalentTo(referralToIgnore, ops => ops
        .Excluding(r => r.Audits));
      _ = referralUpdated.Audits.Should().HaveCount(1);

      Referral referralExcluded = _context.Referrals
        .Single(r => r.Id == referralToExclude.Id);
      _ = referralExcluded.Should().BeEquivalentTo(
        referralToExclude,
        ops => ops
          .Excluding(r => r.Audits));
      _ = referralUpdated.Audits.Should().HaveCount(1);
    }
  }

  public class FixReferralsWithMissingDateStartedProgramme :
    ReferralAdminServiceTests
  {
    public FixReferralsWithMissingDateStartedProgramme(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    public override void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);
      base.Dispose();
    }

    [Fact]
    public async Task DateStartedProgrammeNotNull()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      Guid providerId = Guid.NewGuid();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: referralId,
        providerId: providerId,
        dateStartedProgramme: DateTimeOffset.Now);

      _ = _context.Referrals.Add(referral);
      _ = _context.ProviderSubmissions.Add(
        RandomEntityCreator.CreateProviderSubmission(
          providerId: providerId,
          referralId: referralId));
      _ = await _context.SaveChangesAsync();

      // Act.
      List<string> outcomes =
        await _service.FixReferralsWithMissingDateStartedProgramme();

      // Assert.
      using (new AssertionScope())
      {
        _ = outcomes.Single().Should().Be("No referrals required updates.");
        _ = referral.Should().BeEquivalentTo(
          _context.Referrals.Single(r => r.Id == referralId));
      }
    }

    [Fact]
    public async Task NoProviderSubmissions()
    {
      // Arrange.
      _ = _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral());
      _ = await _context.SaveChangesAsync();

      // Act.
      List<string> outcomes =
        await _service.FixReferralsWithMissingDateStartedProgramme();

      // Assert.
      _ = outcomes.Single().Should().Be("No referrals required updates.");
    }

    [Fact]
    public async Task ProviderSubmissionsWithDateStartedProgrammeNull()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      Guid providerId = Guid.NewGuid();
      DateTimeOffset firstSubmissionDate =
        new(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
      DateTimeOffset secondSubmissionDate =
        new(2023, 1, 2, 0, 0, 0, TimeSpan.Zero);

      _ = _context.Referrals.Add(
        RandomEntityCreator.CreateRandomReferral(
          id: referralId,
          providerId: providerId));
      _ = _context.ProviderSubmissions.Add(
        RandomEntityCreator.CreateProviderSubmission(
          providerId: providerId,
          referralId: referralId,
          date: firstSubmissionDate));
      _ = _context.ProviderSubmissions.Add(
        RandomEntityCreator.CreateProviderSubmission(
          providerId: providerId,
          referralId: referralId,
          date: secondSubmissionDate));
      _ = await _context.SaveChangesAsync();

      // Act.
      List<string> outcomes =
        await _service.FixReferralsWithMissingDateStartedProgramme();

      // Assert.
      using (new AssertionScope())
      {
        _ = outcomes.Single().Should().Be("DateStartedProgramme for Referral" +
          $" {referralId} updated to {firstSubmissionDate}.");
        _ = _context.Referrals.Single(r => r.Id == referralId)
          .DateStartedProgramme.Should().Be(firstSubmissionDate);
      }
    }

    [Fact]
    public async Task MismatchedProviderIds()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      Guid providerId1 = Guid.NewGuid();
      Guid providerId2 = Guid.NewGuid();
      Guid providerSubmissionId = Guid.NewGuid();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: referralId,
        providerId: providerId1);
      ProviderSubmission providerSubmission =
        RandomEntityCreator.CreateProviderSubmission(
          providerId: providerId2,
          referralId: referralId);

      _ = _context.Referrals.Add(referral);
      _ = _context.ProviderSubmissions.Add(providerSubmission);
      _ = await _context.SaveChangesAsync();

      // Act.
      List<string> outcomes =
        await _service.FixReferralsWithMissingDateStartedProgramme();

      // Assert.
      using (new AssertionScope())
      {
        _ = outcomes.Single().Should().Be("Mismatch between ProviderId for " +
          $"Referral {referralId} and ProviderSubmission " +
          $"{providerSubmission.Id}. No update made.");
        _ = referral.Should().BeEquivalentTo(
          _context.Referrals.Single(r => r.Id == referralId));
      }
    }
  }

  public class FixReferralsWithNullProviderUbrnTests :
    ReferralAdminServiceTests
  {
    public FixReferralsWithNullProviderUbrnTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    public override void Dispose()
    {
      _context.GeneralReferrals.RemoveRange(_context.GeneralReferrals);
      _context.GpReferrals.RemoveRange(_context.GpReferrals);
      _context.MskReferrals.RemoveRange(_context.MskReferrals);
      _context.PharmacyReferrals.RemoveRange(_context.PharmacyReferrals);
      _context.MskReferrals.RemoveRange(_context.MskReferrals);
      base.Dispose();
    }

    [Fact]
    public async Task NothingToDo()
    {
      // Arrange.
      _ = _context.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
        providerUbrn: Generators.GenerateUbrnGp(_rnd)));
      _ = _context.SaveChanges();

      // Act.
      List<string> outcomes = await _service
        .FixReferralsWithNullProviderUbrn();

      // Assert.
      _ = outcomes.Single().Should().Be("Nothing to do.");
    }

    [Fact]
    public async Task UnknownReferralSource()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral();
      referral.ProviderUbrn = null;
      referral.ReferralSource = "Unknown";
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();

      // Act.
      List<string> outcomes = await _service
        .FixReferralsWithNullProviderUbrn();

      // Assert.
      _ = outcomes.Single().Should().Contain(
        $"[ERR] Found unknown referral source {referral.ReferralSource}");
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task ReferralInvalidCreationException(
      ReferralSource referralSource)
    {
      // Arrange.
      const string Message = "Exception Message";
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource);
      referral.ProviderUbrn = null;
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      List<Guid> args = new();

      switch (referralSource)
      {
        case ReferralSource.GeneralReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateGeneralReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralInvalidCreationException(Message));
          break;
        case ReferralSource.GpReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateGpReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralInvalidCreationException(Message));
          break;
        case ReferralSource.Msk:
          _ = _mockReferralService
            .Setup(x => x.UpdateMskReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralInvalidCreationException(Message));
          break;
        case ReferralSource.Pharmacy:
          _ = _mockReferralService
            .Setup(x => x.UpdatePharmacyReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralInvalidCreationException(Message));
          break;
        case ReferralSource.SelfReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateSelfReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralInvalidCreationException(Message));
          break;
        case ReferralSource.ElectiveCare:
          // TODO: Add test for ReferralSource.ElectiveCare
          return;
        default:
          throw new XunitException(
            $"Untested referral source type {referralSource}");
      }

      // Act.
      List<string> outcomes = await _service
        .FixReferralsWithNullProviderUbrn();

      // Assert.
      _ = args.First().Should().Be(referral.Id);
      _ = outcomes.Single().Should().Contain($"[ERR] {Message}");

      switch (referralSource)
      {
        case ReferralSource.GeneralReferral:
          _mockReferralService
            .Verify(x => x.UpdateGeneralReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.GpReferral:
          _mockReferralService
            .Verify(x => x.UpdateGpReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.Msk:
          _mockReferralService
            .Verify(x => x.UpdateMskReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.Pharmacy:
          _mockReferralService
            .Verify(x => x.UpdatePharmacyReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.SelfReferral:
          _mockReferralService
            .Verify(x => x.UpdateSelfReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
      }
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task ReferralNotFoundException(ReferralSource referralSource)
    {
      // Arrange.
      const string Message = "Exception Message";
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource);
      referral.ProviderUbrn = null;
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      List<Guid> args = new();

      switch (referralSource)
      {
        case ReferralSource.GeneralReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateGeneralReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralNotFoundException(Message));
          break;
        case ReferralSource.GpReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateGpReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralNotFoundException(Message));
          break;
        case ReferralSource.Msk:
          _ = _mockReferralService
            .Setup(x => x.UpdateMskReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralNotFoundException(Message));
          break;
        case ReferralSource.Pharmacy:
          _ = _mockReferralService
            .Setup(x => x.UpdatePharmacyReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralNotFoundException(Message));
          break;
        case ReferralSource.SelfReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateSelfReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralNotFoundException(Message));
          break;
        case ReferralSource.ElectiveCare:
          // TODO: Add test for ReferralSource.ElectiveCare
          return;
        default:
          throw new XunitException(
            $"Untested referral source type {referralSource}");
      }

      // Act.
      List<string> outcomes = await _service
        .FixReferralsWithNullProviderUbrn();

      // Assert.
      _ = args.First().Should().Be(referral.Id);
      _ = outcomes.Single().Should().Contain($"[ERR] {Message}");

      switch (referralSource)
      {
        case ReferralSource.GeneralReferral:
          _mockReferralService
            .Verify(x => x.UpdateGeneralReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.GpReferral:
          _mockReferralService
            .Verify(x => x.UpdateGpReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.Msk:
          _mockReferralService
            .Verify(x => x.UpdateMskReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.Pharmacy:
          _mockReferralService
            .Verify(x => x.UpdatePharmacyReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.SelfReferral:
          _mockReferralService
            .Verify(x => x.UpdateSelfReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
      }
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task ReferralUpdateException(ReferralSource referralSource)
    {
      // Arrange.
      const string Message = "Exception Message";
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource);
      referral.ProviderUbrn = null;
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      List<Guid> args = new();

      switch (referralSource)
      {
        case ReferralSource.GeneralReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateGeneralReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralUpdateException(Message));
          break;
        case ReferralSource.GpReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateGpReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralUpdateException(Message));
          break;
        case ReferralSource.Msk:
          _ = _mockReferralService
            .Setup(x => x.UpdateMskReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralUpdateException(Message));
          break;
        case ReferralSource.Pharmacy:
          _ = _mockReferralService
            .Setup(x => x.UpdatePharmacyReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralUpdateException(Message));
          break;
        case ReferralSource.SelfReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateSelfReferralUbrnAsync(Capture.In(args)))
            .ThrowsAsync(new ReferralUpdateException(Message));
          break;
        case ReferralSource.ElectiveCare:
          // TODO: Add test for ReferralSource.ElectiveCare
          return;
        default:
          throw new XunitException(
            $"Untested referral source type {referralSource}");
      }

      // Act.
      List<string> outcomes = await _service
        .FixReferralsWithNullProviderUbrn();

      // Assert.
      _ = args.First().Should().Be(referral.Id);
      _ = outcomes.Single().Should().Contain($"[ERR] {Message}");

      switch (referralSource)
      {
        case ReferralSource.GeneralReferral:
          _mockReferralService
            .Verify(x => x.UpdateGeneralReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.GpReferral:
          _mockReferralService
            .Verify(x => x.UpdateGpReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.Msk:
          _mockReferralService
            .Verify(x => x.UpdateMskReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.Pharmacy:
          _mockReferralService
            .Verify(x => x.UpdatePharmacyReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.SelfReferral:
          _mockReferralService
            .Verify(x => x.UpdateSelfReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
      }
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task UpdatedReferral(ReferralSource referralSource)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource);
      referral.ProviderUbrn = null;
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      List<Guid> args = new();

      switch (referralSource)
      {
        case ReferralSource.GeneralReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateGeneralReferralUbrnAsync(Capture.In(args)));
          break;
        case ReferralSource.GpReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateGpReferralUbrnAsync(Capture.In(args)));
          break;
        case ReferralSource.Msk:
          _ = _mockReferralService
            .Setup(x => x.UpdateMskReferralUbrnAsync(Capture.In(args)));
          break;
        case ReferralSource.Pharmacy:
          _ = _mockReferralService
            .Setup(x => x.UpdatePharmacyReferralUbrnAsync(Capture.In(args)));
          break;
        case ReferralSource.SelfReferral:
          _ = _mockReferralService
            .Setup(x => x.UpdateSelfReferralUbrnAsync(Capture.In(args)));
          break;
        case ReferralSource.ElectiveCare:
          // TODO: Add test for ReferralSource.ElectiveCare
          return;
        default:
          throw new XunitException(
            $"Untested referral source type {referralSource}");
      }

      // Act.
      List<string> outcomes = await _service
        .FixReferralsWithNullProviderUbrn();

      // Assert.
      _ = args.First().Should().Be(referral.Id);
      _ = outcomes.Single().Should()
        .Be($"[INF] Updated referral {referral.Id}");

      switch (referralSource)
      {
        case ReferralSource.GeneralReferral:
          _mockReferralService
            .Verify(x => x.UpdateGeneralReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.GpReferral:
          _mockReferralService
            .Verify(x => x.UpdateGpReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.Msk:
          _mockReferralService
            .Verify(x => x.UpdateMskReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.Pharmacy:
          _mockReferralService
            .Verify(x => x.UpdatePharmacyReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
        case ReferralSource.SelfReferral:
          _mockReferralService
            .Verify(x => x.UpdateSelfReferralUbrnAsync(referral.Id),
              Times.Once);
          break;
      }
    }
  }

  public class FixProviderAwaitingTraceAsyncTests : ReferralAdminServiceTests
  {
    public FixProviderAwaitingTraceAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task NullNhsNumber_NotTraced_ProviderAwaitingTrace()
    {
      // Arrange. 
      Referral referral = 
        CreateAndDetachReferral(nhsNumber: null, traceCount: 0);

      // Act.
      _ = await _service.FixProviderAwaitingTraceAsync();

      // Assert.
      Referral updatedReferral = _context.Referrals.Find(referral.Id);
      _ = updatedReferral.Should().BeEquivalentTo(referral, opts => opts
        .Excluding(r => r.Audits));
    }

    [Fact]
    public async Task NullNhsNumber_Traced_ProviderAwaitingStart()
    {
      // Arrange. 
      Referral referral =
        CreateAndDetachReferral(nhsNumber: null, traceCount: 1);

      // Act.
      _ = await _service.FixProviderAwaitingTraceAsync();

      Assert(
        createdReferral: referral,
        expectedStatus: ReferralStatus.ProviderAwaitingStart,
        expectedStatusReason: "Status not updated to ProviderAwaitingStart " +
          "after first trace failure.");
    }

    [Fact]
    public async Task NhsNumber_NoDupe_Traced_ProviderAwaitingStart()
    {
      // Arrange.
      Referral referral = CreateAndDetachReferral(
        nhsNumber: VALID_NHS_NUMBER, traceCount: 1);

      // Act.
      _ = await _service.FixProviderAwaitingTraceAsync();

      Assert(
        createdReferral: referral,
        expectedStatus: ReferralStatus.ProviderAwaitingStart,
        expectedStatusReason: "Status incorrectly set to " +
          "ProviderAwaitingTrace after provider selection.");
    }

    [Fact]
    public async Task NhsNumber_Dupe_Traced_CancelledDuplicateTextMessage()
    {
      // Arrange. 
      Referral referral = CreateAndDetachReferral(
        nhsNumber: VALID_NHS_NUMBER, traceCount: 1);

      Referral duplicate = CreateAndDetachReferral(
        nhsNumber: referral.NhsNumber, traceCount: 0);

      // Act.
      _ = await _service.FixProviderAwaitingTraceAsync();

      Assert(
        createdReferral: referral,
        expectedStatus: ReferralStatus.CancelledDuplicateTextMessage,
        expectedStatusReason: "Traced NHS number is a duplicate of " +
          $"existing referral id(s) {string.Join(", ", duplicate.Id)}");
    }

    private Referral CreateAndDetachReferral(string nhsNumber, int traceCount)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        modifiedAt: new DateTimeOffset(new DateTime(1900, 1, 1)),
        status: ReferralStatus.ProviderAwaitingTrace,
        traceCount: traceCount);
      referral.NhsNumber = nhsNumber;

      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;

      return referral;
    }

    private void Assert(
      Referral createdReferral,
      ReferralStatus expectedStatus,
      string expectedStatusReason)
    {
      Referral updatedReferral = _context.Referrals.Find(createdReferral.Id);

      createdReferral.ModifiedByUserId = TestUserId;
      createdReferral.Status = expectedStatus.ToString();
      createdReferral.StatusReason = expectedStatusReason;

      _ = updatedReferral.Should().BeEquivalentTo(createdReferral, opt => opt
        .Excluding(r => r.Audits)
        .Excluding(r => r.ModifiedAt));

      _ = updatedReferral.ModifiedAt.Should()
        .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
    }
  }

  public class GPReferralsLetterOrLetterSent : ReferralAdminServiceTests
  {
    public GPReferralsLetterOrLetterSent(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task IsActiveFalse_NotUpdated()
    {
      // Arrange.
      Referral referral = CreateAndDetachReferral(isActive: false);

      // Act.
      _ = await _service.FixGPReferralsWithStatusLetterOrLetterSent();

      // Assert.
      AssertNotUpdated(referral);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), new ReferralSource[] {
        ReferralSource.GpReferral})]
    public async Task ReferralSourceNotGPReferral_NotUpdated(
      ReferralSource referralSource)
    {
      // Arrange.
      Referral referral =
        CreateAndDetachReferral(referralSource: referralSource);

      // Act.
      _ = await _service.FixGPReferralsWithStatusLetterOrLetterSent();

      // Assert.
      AssertNotUpdated(referral);
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
      { ReferralStatus.Letter, ReferralStatus.LetterSent })]
    public async Task ReferralStatusNotLetterOrLetterSent_NotUpdated(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral =
        CreateAndDetachReferral(referralStatus: referralStatus);

      // Act.
      _ = await _service.FixGPReferralsWithStatusLetterOrLetterSent();

      // Assert.
      AssertNotUpdated(referral);
    }

    [Theory]
    [InlineData(ReferralStatus.Letter)]
    [InlineData(ReferralStatus.LetterSent)]
    public async Task ReferralStatusLetterOrLetterSent_Updated(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral = CreateAndDetachReferral(referralStatus);

      // Act.
      _ = await _service.FixGPReferralsWithStatusLetterOrLetterSent();

      // Assert.
      Referral updatedReferral = _context.Referrals.Find(referral.Id);

      referral.ModifiedByUserId = TestUserId;
      referral.Status = ReferralStatus.RejectedToEreferrals.ToString();
      referral.ProgrammeOutcome = ProgrammeOutcome.DidNotCommence.ToString();
      _ = updatedReferral.Should().BeEquivalentTo(referral, opt => opt
        .Excluding(r => r.Audits)
        .Excluding(r => r.ModifiedAt));
      _ = updatedReferral.ModifiedAt.Should()
        .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
    }

    private Referral CreateAndDetachReferral(
      ReferralStatus referralStatus = ReferralStatus.New,
      ReferralSource referralSource = ReferralSource.GpReferral,
      bool isActive = true)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        isActive: isActive,
        modifiedAt: new DateTimeOffset(new DateTime(1900, 1, 1)),
        referralSource: referralSource,
        status: referralStatus);

      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;

      return referral;
    }
    private void AssertNotUpdated(Referral createdReferral)
    {
      Referral updatedReferral = _context.Referrals.Find(createdReferral.Id);

      _ = updatedReferral.Should().BeEquivalentTo(createdReferral, opt => opt
        .Excluding(r => r.Audits));
    }
  }

  public class FixDeclinedOrRejectedReferralsWithMissingProgrammeOutcomeTests
    : ReferralAdminServiceTests, IDisposable
  {
    Mapper _mapper;
    public FixDeclinedOrRejectedReferralsWithMissingProgrammeOutcomeTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      MapperConfiguration config =
        new(c => c.CreateMap<Referral, ReferralAudit>());
      _mapper = new Mapper(config);
    }

    [Theory]
    [InlineData(ReferralStatus.ProviderDeclinedByServiceUser)]
    [InlineData(ReferralStatus.ProviderDeclinedTextMessage)]
    [InlineData(ReferralStatus.ProviderRejected)]
    [InlineData(ReferralStatus.ProviderRejectedTextMessage)]
    public async Task DeclinedOrRejected_MissingProgrammeOutcome_Updated(
      ReferralStatus status)
    {
      // Arrange.
      Guid Id = Guid.NewGuid();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Id,
        programmeOutcome: null,
        status: status);
      ReferralAudit referralAudit = _mapper.Map<ReferralAudit>(referral);
      referral.Status = ReferralStatus.Complete.ToString();
      _context.Referrals.Add(referral);
      _context.ReferralsAudit.Add(referralAudit);
      await _context.SaveChangesAsync();

      // Act.
      string response = await 
        _service.FixDeclinedOrRejectedReferralsWithMissingProgrammeOutcome();

      // Assert.
      using (new AssertionScope())
      {
        response.Should().Be($"1 referral(s) had their " +
          $"ProgrammeOutcome updated to {ProgrammeOutcome.DidNotCommence}");
        _context.Referrals.Single(r => r.Id == referral.Id).ProgrammeOutcome
          .Should().Be(ProgrammeOutcome.DidNotCommence.ToString());
      }
    }

    [Theory]
    [InlineData(ReferralStatus.ProviderDeclinedByServiceUser)]
    [InlineData(ReferralStatus.ProviderDeclinedTextMessage)]
    [InlineData(ReferralStatus.ProviderRejected)]
    [InlineData(ReferralStatus.ProviderRejectedTextMessage)]
    public async Task DeclinedOrRejected_ExistingProgrammeOutcome_NotUpdated(
      ReferralStatus status)
    {
      // Arrange.
      Guid Id = Guid.NewGuid();
      const string EXISTING_PROGRAMME_OUTCOME = "ExistingProgrammeOutcome";
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        programmeOutcome: EXISTING_PROGRAMME_OUTCOME,
        status: status);
      ReferralAudit referralAudit = _mapper.Map<ReferralAudit>(referral);
      _context.Referrals.Add(referral);
      _context.ReferralsAudit.Add(referralAudit);
      await _context.SaveChangesAsync();

      // Act.
      string response = await
        _service.FixDeclinedOrRejectedReferralsWithMissingProgrammeOutcome();

      // Assert.
      using (new AssertionScope())
      {
        response.Should().Be($"0 referral(s) had their " +
          $"ProgrammeOutcome updated to {ProgrammeOutcome.DidNotCommence}");
        _context.Referrals.Single(r => r.Id == referral.Id).ProgrammeOutcome
          .Should().Be(EXISTING_PROGRAMME_OUTCOME);
      }
    }

    [Fact]
    public async Task NotDeclinedOrRejected_NullProgrammeOutcome_NotUpdated()
    {
      // Arrange.
      Guid Id = Guid.NewGuid();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        programmeOutcome: null,
        status: ReferralStatus.ProviderAccepted);
      ReferralAudit referralAudit = _mapper.Map<ReferralAudit>(referral);
      _context.Referrals.Add(referral);
      _context.ReferralsAudit.Add(referralAudit);
      await _context.SaveChangesAsync();

      // Act.
      string response = await
        _service.FixDeclinedOrRejectedReferralsWithMissingProgrammeOutcome();

      // Assert.
      using (new AssertionScope())
      {
        response.Should().Be($"0 referral(s) had their " +
          $"ProgrammeOutcome updated to {ProgrammeOutcome.DidNotCommence}");
        _context.Referrals.Single(r => r.Id == referral.Id).ProgrammeOutcome
          .Should().BeNull();
      }
    }
  }

  public class FixMSKReferralsWithStatusRejectedToEreferralsTests
      : ReferralAdminServiceTests
  {
    public FixMSKReferralsWithStatusRejectedToEreferralsTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task IsActiveFalse_NotUpdated()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        isActive: false);
      AddAndDetatchReferral(referral);

      // Act.
      string result = await _service
        .FixMSKReferralsWithStatusRejectedToEreferrals();

      // Assert.
      AssertNotUpdated(referral, result);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), new ReferralSource[] {
        ReferralSource.Msk})]
    public async Task ReferralSourceNonMSK_NotUpdated(
      ReferralSource referralSource)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource);
      AddAndDetatchReferral(referral);

      // Act.
      string result = await _service
        .FixMSKReferralsWithStatusRejectedToEreferrals();

      // Assert.
      AssertNotUpdated(referral, result);
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[] {
        ReferralStatus.RejectedToEreferrals})]
    public async Task ReferralStatusNonRejectedToEreferrals_NotUpdated(
      ReferralStatus status)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: status);
      AddAndDetatchReferral(referral);

      // Act.
      string result = await _service
        .FixMSKReferralsWithStatusRejectedToEreferrals();

      // Assert.
      AssertNotUpdated(referral, result);
    }

    [Fact]
    public async Task IsActiveMSKRejectedToEreferrals_Updated()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.Msk,
        status: ReferralStatus.RejectedToEreferrals);
      AddAndDetatchReferral(referral);

      // Act.
      string result = await _service
        .FixMSKReferralsWithStatusRejectedToEreferrals();

      // Assert.
      _ = result.Should().Be($"1 MSK referral(s) had " +
        $"their status updated from {ReferralStatus.RejectedToEreferrals} " +
        $"to {ReferralStatus.CancelledByEreferrals}.");

      Referral updatedReferral = _context.Referrals
        .Single(r => r.Id == referral.Id);
      _ = updatedReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits)
        .Excluding(r => r.Status)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId));
      _ = updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
      _ = updatedReferral.ModifiedByUserId.Should()
        .NotBe(referral.ModifiedByUserId);
      _ = updatedReferral.Status.Should()
        .Be(ReferralStatus.CancelledByEreferrals.ToString());
      _ = updatedReferral.Status.Should().NotBe(referral.Status);
    }

    private void AddAndDetatchReferral(Referral referral)
    {
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;
    }

    private void AssertNotUpdated(Referral referral, string result)
    {
      _ = result.Should().Be($"0 MSK referral(s) had " +
        $"their status updated from {ReferralStatus.RejectedToEreferrals} " +
        $"to {ReferralStatus.CancelledByEreferrals}.");

      Referral updatedReferral = _context.Referrals
        .Single(r => r.Id == referral.Id);
      _ = updatedReferral.Should().BeEquivalentTo(referral, options => options
        .Excluding(r => r.Audits));
    }
  }

  public class FixPharmacyReferralsWithInvalidStatus
    : ReferralAdminServiceTests
  {
    public FixPharmacyReferralsWithInvalidStatus(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task IsActiveFalse_NotUpdated()
    {
      // Arrange.
      Referral referral = CreateAndDetachReferral(isActive: false);

      // Act.
      _ = await _service.FixPharmacyReferralsWithInvalidStatus();

      // Assert.
      AssertNotUpdated(referral);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), new ReferralSource[] {
        ReferralSource.Pharmacy})]
    public async Task ReferralSourceNotPharmacy_NotUpdated(
        ReferralSource referralSource)
    {
      // Arrange.
      Referral referral =
        CreateAndDetachReferral(referralSource: referralSource);

      // Act.
      _ = await _service.FixPharmacyReferralsWithInvalidStatus();

      // Assert.
      AssertNotUpdated(referral);
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
      {
          ReferralStatus.Letter,
          ReferralStatus.LetterSent,
          ReferralStatus.RejectedToEreferrals
        })]
    public async Task ReferralStatusInvalid_NotUpdated(
        ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral =
        CreateAndDetachReferral(referralStatus: referralStatus);

      // Act.
      _ = await _service.FixPharmacyReferralsWithInvalidStatus();

      // Assert.
      AssertNotUpdated(referral);
    }

    [Theory]
    [InlineData(ReferralStatus.Letter)]
    [InlineData(ReferralStatus.LetterSent)]
    [InlineData(ReferralStatus.RejectedToEreferrals)]
    public async Task ReferralStatusValid_Updated(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral = CreateAndDetachReferral(referralStatus);

      // Act.
      _ = await _service.FixPharmacyReferralsWithInvalidStatus();

      // Assert.
      Referral updatedReferral = _context.Referrals.Find(referral.Id);

      _ = updatedReferral.Should().BeEquivalentTo(referral, opt => opt
        .Excluding(r => r.Audits)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.ProgrammeOutcome)
        .Excluding(r => r.Status));
      _ = updatedReferral.ModifiedAt.Should()
        .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
      _ = updatedReferral.ModifiedByUserId.Should().Be(TestUserId);
      _ = updatedReferral.ProgrammeOutcome.Should()
        .Be(ProgrammeOutcome.DidNotCommence.ToString());
      _ = updatedReferral.Status.Should()
        .Be(ReferralStatus.CancelledByEreferrals.ToString());
    }

    private Referral CreateAndDetachReferral(
      ReferralStatus referralStatus = ReferralStatus.New,
      ReferralSource referralSource = ReferralSource.Pharmacy,
      bool isActive = true)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        isActive: isActive,
        modifiedAt: new DateTimeOffset(new DateTime(1900, 1, 1)),
        referralSource: referralSource,
        status: referralStatus);

      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;

      return referral;
    }

    private void AssertNotUpdated(Referral createdReferral)
    {
      Referral updatedReferral = _context.Referrals.Find(createdReferral.Id);
      _ = updatedReferral.Should().BeEquivalentTo(createdReferral, opt => opt
        .Excluding(r => r.Audits));
    }
  }

  public class FixSelfReferralsWithInvalidStatus : ReferralAdminServiceTests
  {
    public FixSelfReferralsWithInvalidStatus(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task IsActiveFalse_NotUpdated()
    {
      // Arrange.
      Referral referral = CreateAndDetachReferral(isActive: false);

      // Act.
      _ = await _service.FixSelfReferralsWithInvalidStatus();

      // Assert.
      AssertNotUpdated(referral);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), new ReferralSource[] {
        ReferralSource.SelfReferral})]
    public async Task ReferralSourceNotSelfReferral_NotUpdated(
        ReferralSource referralSource)
    {
      // Arrange.
      Referral referral =
        CreateAndDetachReferral(referralSource: referralSource);

      // Act.
      _ = await _service.FixSelfReferralsWithInvalidStatus();

      // Assert.
      AssertNotUpdated(referral);
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
      {
          ReferralStatus.Letter,
          ReferralStatus.LetterSent,
          ReferralStatus.RejectedToEreferrals,
          ReferralStatus.FailedToContact
        })]
    public async Task ReferralStatusInvalid_NotUpdated(
        ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral =
        CreateAndDetachReferral(referralStatus: referralStatus);

      // Act.
      _ = await _service.FixSelfReferralsWithInvalidStatus();

      // Assert.
      AssertNotUpdated(referral);
    }

    [Theory]
    [InlineData(ReferralStatus.Letter)]
    [InlineData(ReferralStatus.LetterSent)]
    public async Task LetterOrLetterSent_Updated(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral = CreateAndDetachReferral(referralStatus);

      // Act.
      _ = await _service.FixSelfReferralsWithInvalidStatus();

      // Assert.
      Referral updatedReferral = _context.Referrals.Find(referral.Id);
      _ = updatedReferral.ProgrammeOutcome.Should()
        .Be(ProgrammeOutcome.DidNotCommence.ToString());
      _ = updatedReferral.Status.Should()
        .Be(ReferralStatus.CancelledByEreferrals.ToString());
      AssertUpdated(updatedReferral, referral);
    }

    [Fact]
    public async Task RejectedToEreferrals_Updated()
    {
      // Arrange.
      Referral referral =
        CreateAndDetachReferral(ReferralStatus.RejectedToEreferrals);

      // Act.
      _ = await _service.FixSelfReferralsWithInvalidStatus();

      // Assert.
      Referral updatedReferral = _context.Referrals.Find(referral.Id);
      _ = updatedReferral.Status.Should()
        .Be(ReferralStatus.CancelledByEreferrals.ToString());
      AssertUpdated(updatedReferral, referral);
    }

    [Fact]
    public async Task FailedToContact_Updated()
    {
      // Arrange.
      Referral referral =
        CreateAndDetachReferral(ReferralStatus.FailedToContact);

      // Act.
      _ = await _service.FixSelfReferralsWithInvalidStatus();

      // Assert.
      Referral updatedReferral = _context.Referrals.Find(referral.Id);
      _ = updatedReferral.Status.Should()
        .Be(ReferralStatus.CancelledDueToNonContact.ToString());
      AssertUpdated(updatedReferral, referral);
    }

    private Referral CreateAndDetachReferral(
      ReferralStatus referralStatus = ReferralStatus.New,
      ReferralSource referralSource = ReferralSource.SelfReferral,
      bool isActive = true)
    {
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        isActive: isActive,
        modifiedAt: new DateTimeOffset(new DateTime(1900, 1, 1)),
        referralSource: referralSource,
        status: referralStatus);

      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;

      return referral;
    }

    private void AssertNotUpdated(Referral createdReferral)
    {
      Referral updatedReferral = _context.Referrals.Find(createdReferral.Id);
      _ = updatedReferral.Should().BeEquivalentTo(createdReferral, opt => opt
        .Excluding(r => r.Audits));
    }

    private void AssertUpdated(
      Referral updatedReferral,
      Referral createdReferral)
    {
      _ = updatedReferral.Should().BeEquivalentTo(createdReferral, opt => opt
        .Excluding(r => r.Audits)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.ProgrammeOutcome)
        .Excluding(r => r.Status));
      _ = updatedReferral.ModifiedAt.Should()
        .BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
      _ = updatedReferral.ModifiedByUserId.Should().Be(TestUserId);

    }
  }

  public class ResetReferralAsyncTests : ReferralAdminServiceTests
  {
    public ResetReferralAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReferralNull_ArgumentNullException()
    {
      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ResetReferralAsync(null, ReferralStatus.New));

      // Assert.
      _ = ex.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task UbrnDoesNotMatch_ReferralNotFoundException()
    {
      // Arrange.
      Referral createdReferral = RandomEntityCreator.CreateRandomReferral();
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();

      Business.Models.Referral referralToReset = new()
      {
        Id = createdReferral.Id,
        Status = createdReferral.Status,
        Ubrn = "DoesNotMatch"
      };

      await ReferralNotFoundExceptionActAndAssert(referralToReset);
    }

    [Fact]
    public async Task IdDoesNotMatch_ReferralNotFoundException()
    {
      // Arrange.
      Referral createdReferral = RandomEntityCreator.CreateRandomReferral();
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();

      Business.Models.Referral referralToReset = new()
      {
        Id = Guid.NewGuid(),
        Status = createdReferral.Status,
        Ubrn = createdReferral.Ubrn
      };

      await ReferralNotFoundExceptionActAndAssert(referralToReset);
    }

    [Fact]
    public async Task StatusDoesNotMatch_ReferralNotFoundException()
    {
      // Arrange.
      Referral createdReferral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New);
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();

      Business.Models.Referral referralToReset = new()
      {
        Id = createdReferral.Id,
        Status = ReferralStatus.RmcCall.ToString(),
        Ubrn = createdReferral.Ubrn
      };

      await ReferralNotFoundExceptionActAndAssert(referralToReset);
    }

    private async Task ReferralNotFoundExceptionActAndAssert(
      Business.Models.Referral referralToReset)
    {
      string expectedMessage = "Unable to find an active referral with an " +
        $"id of {referralToReset.Id}, " +
        $"status of {referralToReset.Status} and ubrn of " +
        $"{referralToReset.Ubrn}.";

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ResetReferralAsync(
          referralToReset,
          ReferralStatus.New));

      // Assert.
      _ = ex.Should().BeOfType<ReferralNotFoundException>();
      _ = ex.Message.Should().Be(expectedMessage);
    }

    [Theory]
    [MemberData(nameof(ReferralStatusesTheoryData), new ReferralStatus[]
      { ReferralStatus.New, ReferralStatus.RmcCall })]
    public async Task UpdateStatusNotSupported_Exception(
      ReferralStatus status)
    {
      // Arrange. 
      Referral createdReferral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New);
      _ = _context.Referrals.Add(createdReferral);
      _ = _context.SaveChanges();

      Business.Models.Referral referralToReset = new()
      {
        Id = createdReferral.Id,
        Status = ReferralStatus.New.ToString(),
        Ubrn = createdReferral.Ubrn
      };

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _service.ResetReferralAsync(referralToReset, status));

      // Assert.
      _ = ex.Should().BeOfType<NotSupportedException>();
      _ = ex.Message.Should().Be("Currently this process will only reset a " +
        $"referral's status to {ReferralStatus.New} or " +
        $"{ReferralStatus.RmcCall}");
    }

    [Theory]
    [InlineData(false, ReferralStatus.New)]
    [InlineData(true, ReferralStatus.RmcCall)]
    public async Task ResetToNew_StatusUpdateAndResetProviderFields(
      bool isVulnerable,
      ReferralStatus expectedStatus)
    {
      Referral existingReferral = CreateReferralWithProviderSubmission(
        isVulnerable: isVulnerable);

      string expectedStatusReason = "Approved reset by NHSE";
      Business.Models.Referral referralToReset = new()
      {
        Id = existingReferral.Id,
        Status = existingReferral.Status,
        StatusReason = expectedStatusReason,
        Ubrn = existingReferral.Ubrn
      };

      // Act.
      Business.Models.Referral result = await _service.ResetReferralAsync(
        referralToReset,
        expectedStatus);

      Referral updatedReferral = _context.Referrals
        .Include(r => r.ProviderSubmissions)
        .Single(r => r.Id == referralToReset.Id);

      StatusUpdateGenericAsserts(
        existingReferral,
        expectedStatusReason,
        expectedStatus,
        result,
        updatedReferral);
    }

    [Fact]
    public async Task ResetToRmcCall_StatusUpdateAndResetProviderFields()
    {
      Referral existingReferral = CreateReferralWithProviderSubmission(
        isVulnerable: true);

      ReferralStatus expectedStatus = ReferralStatus.RmcCall;
      string expectedStatusReason = "Approved reset by NHSE";
      Business.Models.Referral referralToReset = new()
      {
        Id = existingReferral.Id,
        Status = existingReferral.Status,
        StatusReason = expectedStatusReason,
        Ubrn = existingReferral.Ubrn
      };

      // Act.
      Business.Models.Referral result = await _service.ResetReferralAsync(
        referralToReset,
        expectedStatus);

      Referral updatedReferral = _context.Referrals
        .Include(r => r.ProviderSubmissions)
        .Single(r => r.Id == referralToReset.Id);

      StatusUpdateGenericAsserts(
        existingReferral,
        expectedStatusReason,
        expectedStatus,
        result,
        updatedReferral);
    }

    private static void StatusUpdateGenericAsserts(
      Referral existingReferral,
      string expectedStatusReason,
      ReferralStatus expectedStatus,
      Business.Models.Referral result,
      Referral updatedReferral)
    {
      // Assert.
      _ = result.Should().BeOfType<Business.Models.Referral>();
      _ = updatedReferral.Should().BeEquivalentTo(existingReferral, opt => opt
        .Excluding(r => r.Audits)
        .Excluding(r => r.DateCompletedProgramme)
        .Excluding(r => r.DateLetterSent)
        .Excluding(r => r.DateOfProviderContactedServiceUser)
        .Excluding(r => r.DateOfProviderSelection)
        .Excluding(r => r.DateStartedProgramme)
        .Excluding(r => r.DateToDelayUntil)
        .Excluding(r => r.DelayReason)
        .Excluding(r => r.FirstRecordedWeight)
        .Excluding(r => r.FirstRecordedWeightDate)
        .Excluding(r => r.LastRecordedWeight)
        .Excluding(r => r.LastRecordedWeightDate)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.ProgrammeOutcome)
        .Excluding(r => r.ProviderId)
        .Excluding(r => r.ProviderSubmissions)
        .Excluding(r => r.Status)
        .Excluding(r => r.StatusReason));

      _ = result.DateCompletedProgramme.Should().BeNull();
      _ = result.DateLetterSent.Should().BeNull();
      _ = result.DateOfProviderContactedServiceUser.Should().BeNull();
      _ = result.DateOfProviderSelection.Should().BeNull();
      _ = result.DateStartedProgramme.Should().BeNull();
      _ = result.DelayReason.Should().BeNull();
      _ = result.FirstRecordedWeight.Should().BeNull();
      _ = result.FirstRecordedWeightDate.Should().BeNull();
      _ = result.LastRecordedWeight.Should().BeNull();
      _ = result.LastRecordedWeightDate.Should().BeNull();
      _ = result.ModifiedAt.Should().BeAfter(existingReferral.ModifiedAt);
      _ = result.ModifiedByUserId.Should().Be(TEST_USER_ID);
      _ = result.ProgrammeOutcome.Should().BeNull();
      _ = result.ProviderId.Should().BeNull();
      _ = result.ProviderSubmissions.Any().Should().BeFalse();
      _ = result.Status.Should().Be(expectedStatus.ToString());
      _ = result.StatusReason.Should().Be(expectedStatusReason);
    }

    private Referral CreateReferralWithProviderSubmission(
      bool isVulnerable)
    {
      // Arrange. 
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateCompletedProgramme: DateTimeOffset.Now,
        dateLetterSent: DateTimeOffset.Now,
        dateOfProviderContactedServiceUser: DateTimeOffset.Now,
        dateOfProviderSelection: DateTimeOffset.Now,
        dateStartedProgramme: DateTimeOffset.Now,
        dateToDelayUntil: DateTimeOffset.Now,
        delayReason: "DelayReason",
        firstRecordedWeight: 100,
        firstRecordedWeightDate: DateTimeOffset.Now,
        isVulnerable: isVulnerable,
        lastRecordedWeight: 95,
        lastRecordedWeightDate: DateTimeOffset.Now,
        programmeOutcome: ReferralStatus.Complete.ToString(),
        providerId: Guid.NewGuid(),
        status: ReferralStatus.Complete);

      referral.ProviderSubmissions = new()
        {
          RandomEntityCreator.CreateProviderSubmission()
        };

      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.ChangeTracker.Clear();
      return referral;
    }
  }

  public class FixReferralsWithInvalidStatusesTests : ReferralAdminServiceTests
  {
    public FixReferralsWithInvalidStatusesTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ExceptionGpReferralDoNotContactByEmail_NoUpdate()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        email: Constants.DO_NOT_CONTACT_EMAIL,
        referralSource: ReferralSource.GpReferral,
        status: ReferralStatus.Exception);
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      List<string> results = await _service.FixReferralsWithInvalidStatuses();

      // Assert.
      using (new AssertionScope())
      {
        _ = results.Should().HaveCount(0);

        Referral updatedReferral = _context.Referrals
          .Single(x => x.Id == referral.Id);

        _ = updatedReferral.Should().BeEquivalentTo(referral, opt => opt
          .Excluding(x => x.Audits));
      }
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData),
      new ReferralSource[] { ReferralSource.GpReferral })]
    public async Task ExceptionNonGpReferralDoNotContactByEmail_Update(
      ReferralSource referralSource)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        email: Constants.DO_NOT_CONTACT_EMAIL,
        referralSource: referralSource,
        status: ReferralStatus.Exception);
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string expectedStatusReason = $"Fixed invalid status {referral.Status}";
      string expectedResult = $"{referral.Id} {expectedStatusReason}";

      // Act.
      List<string> results = await _service.FixReferralsWithInvalidStatuses();

      // Assert.
      using (new AssertionScope())
      {
        _ = results.Should().HaveCount(1);
        _ = results.Single().Should().Be(expectedResult);

        Referral updatedReferral = _context.Referrals
          .Single(x => x.Id == referral.Id);

        _ = updatedReferral.Should().BeEquivalentTo(referral, ops => ops
          .Excluding(x => x.Audits)
          .Excluding(x => x.ModifiedAt)
          .Excluding(x => x.ModifiedByUserId)
          .Excluding(x => x.Status)
          .Excluding(x => x.StatusReason));

        _ = updatedReferral.Audits.Should().HaveCount(1);
        _ = updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        _ = updatedReferral.Status.Should()
          .Be(ReferralStatus.Complete.ToString());
        _ = updatedReferral.StatusReason.Should().Be(expectedStatusReason);
      }
    }

    [Theory]
    [MemberData(nameof(InvalidReferralStatusAndSource))]
    public async Task InvalidStatus_Update(
      ReferralSource referralSource,
      ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource,
        status: referralStatus);
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.ChangeTracker.Clear();

      string expectedStatusReason = $"Fixed invalid status {referral.Status}";
      string expectedResult = $"{referral.Id} {expectedStatusReason}";

      // Act.
      List<string> results = await _service.FixReferralsWithInvalidStatuses();

      // Assert.
      using (new AssertionScope())
      {
        _ = results.Should().HaveCount(1);
        _ = results.Single().Should().Be(expectedResult);

        Referral updatedReferral = _context.Referrals
          .Single(x => x.Id == referral.Id);

        _ = updatedReferral.Should().BeEquivalentTo(referral, ops => ops
          .Excluding(x => x.Audits)
          .Excluding(x => x.ModifiedAt)
          .Excluding(x => x.ModifiedByUserId)
          .Excluding(x => x.Status)
          .Excluding(x => x.StatusReason));

        _ = updatedReferral.Audits.Should().HaveCount(1);
        _ = updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        _ = updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        _ = updatedReferral.Status.Should()
          .Be(ReferralStatus.Complete.ToString());
        _ = updatedReferral.StatusReason.Should().Be(expectedStatusReason);
      }
    }

    [Theory]
    [MemberData(nameof(ValidReferralStatusAndSource))]
    public async Task ValidStatus_NoUpdate(
      ReferralSource referralSource,
      ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: referralSource,
        status: referralStatus);
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.ChangeTracker.Clear();

      // Act.
      List<string> results = await _service.FixReferralsWithInvalidStatuses();

      // Assert.
      using (new AssertionScope())
      {
        _ = results.Should().HaveCount(0);

        Referral updatedReferral = _context.Referrals
          .Single(x => x.Id == referral.Id);

        _ = updatedReferral.Should().BeEquivalentTo(referral, opt => opt
          .Excluding(x => x.Audits));
      }
    }

    public static TheoryData<ReferralSource, ReferralStatus>
      InvalidReferralStatusAndSource()
    {
      TheoryData<ReferralSource, ReferralStatus> data = new();

      // Add invalid non GP referral sources and statuses.
      ReferralSource[] nonGpReferralSources = Enum
        .GetValues(typeof(ReferralSource))
        .Cast<ReferralSource>()
        .Where(x => x != ReferralSource.GpReferral)
        .ToArray();

      ReferralStatus[] invalidNonGpReferralStatuses =
      {
        ReferralStatus.CancelledByEreferrals,
        ReferralStatus.CancelledDueToNonContact,
        ReferralStatus.CancelledDuplicate,
        ReferralStatus.FailedToContact,
        ReferralStatus.Letter,
        ReferralStatus.LetterSent,
        ReferralStatus.RejectedToEreferrals
      };

      for (int i = 0; i < nonGpReferralSources.Length; i++)
      {
        for (int j = 0; j < invalidNonGpReferralStatuses.Length; j++)
        {
          data.Add(nonGpReferralSources[i], invalidNonGpReferralStatuses[j]);
        }
      }

      // Add invalid GP referral statuses.
      data.Add(ReferralSource.GpReferral,
        ReferralStatus.CancelledDueToNonContact);
      data.Add(ReferralSource.GpReferral, ReferralStatus.CancelledDuplicate);
      data.Add(ReferralSource.GpReferral, ReferralStatus.DischargeOnHold);
      data.Add(ReferralSource.GpReferral, ReferralStatus.Letter);
      data.Add(ReferralSource.GpReferral, ReferralStatus.LetterSent);

      return data;
    }

    public static TheoryData<ReferralSource, ReferralStatus>
      ValidReferralStatusAndSource()
    {
      TheoryData<ReferralSource, ReferralStatus> data = new();

      // Add valid non GP referral sources and statuses.
      ReferralSource[] nonGpReferralSources = Enum
        .GetValues(typeof(ReferralSource))
        .Cast<ReferralSource>()
        .Where(x => x != ReferralSource.GpReferral)
        .ToArray();

      ReferralStatus[] validNonGpReferralStatuses = Enum
        .GetValues(typeof(ReferralStatus))
        .Cast<ReferralStatus>()
        .Where(x => x != ReferralStatus.CancelledByEreferrals)
        .Where(x => x != ReferralStatus.CancelledDueToNonContact)
        .Where(x => x != ReferralStatus.CancelledDuplicate)
        .Where(x => x != ReferralStatus.FailedToContact)
        .Where(x => x != ReferralStatus.Letter)
        .Where(x => x != ReferralStatus.LetterSent)
        .Where(x => x != ReferralStatus.RejectedToEreferrals)
        .ToArray();

      for (int i = 0; i < nonGpReferralSources.Length; i++)
      {
        for (int j = 0; j < validNonGpReferralStatuses.Length; j++)
        {
          data.Add(nonGpReferralSources[i], validNonGpReferralStatuses[j]);
        }
      }

      // Add valid GP referral statuses.
      ReferralStatus[] validGpReferralSources = Enum
        .GetValues(typeof(ReferralStatus))
        .Cast<ReferralStatus>()
        .Where(x => x != ReferralStatus.CancelledDueToNonContact)
        .Where(x => x != ReferralStatus.CancelledDuplicate)
        .Where(x => x != ReferralStatus.DischargeOnHold)
        .Where(x => x != ReferralStatus.Letter)
        .Where(x => x != ReferralStatus.LetterSent)
        .ToArray();

      for (int i = 0; i < validGpReferralSources.Length; i++)
      {
        data.Add(ReferralSource.GpReferral, validGpReferralSources[i]);
      }

      return data;
    }
  }

  public class SendDischargeLettersForCompleteMskReferralsTests :
    ReferralAdminServiceTests
  {
    private Mock<ReferralService> _referralServiceMock;
    private const string ODS_CODE = "ABCDE";

    public SendDischargeLettersForCompleteMskReferralsTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _referralServiceMock = new();
    }

    [Fact]
    public async Task FutureDate_ThrowsDateRangeNotValidException()
    {
      // Arrange.
      DateTimeOffset fromDate = DateTimeOffset.Now.AddDays(1);
      DateTimeOffset toDate = DateTimeOffset.Now.AddDays(1);

      try
      {
        // Act.
        await _service.SendDischargeLettersForCompleteMskReferrals(
          fromDate,
          toDate);
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.Should().BeOfType(typeof(DateRangeNotValidException));
          ex.Message.Should().Be("Dates cannot be in the future.");
        }
      }
    }

    [Fact]
    public async Task FromDateAfterToDate_ThrowsDateRangeNotValidException()
    {
      // Arrange.
      DateTimeOffset fromDate = DateTimeOffset.Now.AddDays(-1);
      DateTimeOffset toDate = DateTimeOffset.Now.AddDays(-2);

      try
      {
        // Act.
        await _service.SendDischargeLettersForCompleteMskReferrals(
          fromDate,
          toDate);
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.Should().BeOfType(typeof(DateRangeNotValidException));
          ex.Message.Should().Be("FromDate must be earlier than ToDate.");
        }
      }
    }

    [Fact]
    public async Task ReferralWithDateOfReferralOutsideRange_NoLetterSent()
    {
      // Arrange.
      DateTimeOffset fromDate = DateTimeOffset.UtcNow.AddDays(-10);
      DateTimeOffset toDate = DateTimeOffset.UtcNow;

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        referralSource: ReferralSource.Msk,
        isActive: true,
        status: ReferralStatus.Complete,
        dateOfReferral: DateTimeOffset.UtcNow.AddDays(-11),
        consentForReferrerUpdatedWithOutcome: true);
      referral.ReferringOrganisationOdsCode = ODS_CODE;
      _context.Referrals.Add(referral);

      MskOrganisation organisation = new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = ODS_CODE,
        SendDischargeLetters = true,
        SiteName = "SiteName"
      };
      _context.MskOrganisations.Add(organisation);

      await _context.SaveChangesAsync();

      // Act.
      List<Guid> result = await _service
        .SendDischargeLettersForCompleteMskReferrals(fromDate, toDate);

      // Assert.
      result.Should().BeEmpty();
    }

    [Fact]
    public async Task ReferralWithNonParticipatingOrganisation_NoLetterSent()
    {
      // Arrange.
      DateTimeOffset fromDate = DateTimeOffset.UtcNow.AddDays(-10);
      DateTimeOffset toDate = DateTimeOffset.UtcNow;

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        referralSource: ReferralSource.Msk,
        isActive: true,
        status: ReferralStatus.Complete,
        dateOfReferral: DateTimeOffset.UtcNow.AddDays(-9),
        consentForReferrerUpdatedWithOutcome: true);
      referral.ReferringOrganisationOdsCode = ODS_CODE;
      _context.Referrals.Add(referral);

      MskOrganisation organisation = new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = ODS_CODE,
        SendDischargeLetters = false,
        SiteName = "SiteName"
      };
      _context.MskOrganisations.Add(organisation);

      await _context.SaveChangesAsync();

      // Act.
      List<Guid> result = await _service
        .SendDischargeLettersForCompleteMskReferrals(fromDate, toDate);

      // Assert.
      result.Should().BeEmpty();
    }

    [Fact]
    public async Task ReferralWithParticipatingOrganisation_SendsLetter()
    {
      // Arrange.
      DateTimeOffset fromDate = DateTimeOffset.UtcNow.AddDays(-10);
      DateTimeOffset toDate = DateTimeOffset.UtcNow;

      Provider provider = RandomEntityCreator.CreateRandomProvider(
        id: Guid.NewGuid());
      _context.Providers.Add(provider);

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        referralSource: ReferralSource.Msk,
        isActive: true,
        status: ReferralStatus.Complete,
        dateOfReferral: DateTimeOffset.UtcNow.AddDays(-9),
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        providerId: provider.Id,
        consentForReferrerUpdatedWithOutcome: true);
      referral.Provider = provider;
      referral.ReferringOrganisationOdsCode = ODS_CODE;
      _context.Referrals.Add(referral);

      MskOrganisation organisation = new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = ODS_CODE,
        SendDischargeLetters = true,
        SiteName = "SiteName"
      };
      _context.MskOrganisations.Add(organisation);

      await _context.SaveChangesAsync();

      _mockReferralService.Setup(r =>
        r.PostDischarges(It.IsAny<List<GpDocumentProxyReferralDischarge>>()))
        .ReturnsAsync(new List<Guid>() { referral.Id});

      // Act.
      List<Guid> result = await _service
        .SendDischargeLettersForCompleteMskReferrals(fromDate, toDate);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().HaveCount(1);
        result.Single().Should().Be(referral.Id);
      }
    }

    [Theory]
    [InlineData(ReferralStatus.SentForDischarge)]
    [InlineData(ReferralStatus.UnableToDischarge)]
    public async Task ReferralWithPreviouslySentDischarge_NoLetterSent(
      ReferralStatus auditStatus)
    {
      // Arrange.
      DateTimeOffset fromDate = DateTimeOffset.UtcNow.AddDays(-10);
      DateTimeOffset toDate = DateTimeOffset.UtcNow;

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        referralSource: ReferralSource.Msk,
        isActive: true,
        status: ReferralStatus.Complete,
        dateOfReferral: DateTimeOffset.UtcNow.AddDays(-9),
        consentForReferrerUpdatedWithOutcome: true);
      referral.ReferringOrganisationOdsCode = ODS_CODE;
      _context.Referrals.Add(referral);

      MskOrganisation organisation = new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = ODS_CODE,
        SendDischargeLetters = false,
        SiteName = "SiteName"
      };

      _context.MskOrganisations.Add(organisation);

      ReferralAudit audit = new()
      {
        Id = referral.Id,
        Status = auditStatus.ToString(),
        IsActive = true
      };

      _context.ReferralsAudit.Add(audit);

      await _context.SaveChangesAsync();

      // Act.
      List<Guid> result = await _service
        .SendDischargeLettersForCompleteMskReferrals(fromDate, toDate);

      // Assert.
      result.Should().BeEmpty();
    }
  }

  public class SetIsErsClosedToFalseTests : ReferralAdminServiceTests
  {
    private const string MatchingId = "B773FF08-214A-474D-B9DA-2EADC273D406";
    private const string MatchingUbrn = "012345678910";
    private const string NonMatchingId = "A37B04C5-0397-4E63-847E-7CB95FC9CB76";
    private const string NonMatchingUbrn = "019876543210";


    public SetIsErsClosedToFalseTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper) { }

    [Theory]
    [MemberData(
      nameof(ReferralStatusesTheoryData), 
      new ReferralStatus[] { ReferralStatus.RejectedToEreferrals })]
    public async Task InvalidReferralStatusThrowsException(ReferralStatus status)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.GpReferral,
        status: status,
        isErsClosed: true);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      Func<Task<Business.Models.Referral>> result =
        () => _service.SetIsErsClosedToFalse(referral.Id, referral.Ubrn);

      // Assert.
      await result.Should().ThrowAsync<ReferralNotFoundException>()
        .WithMessage($"*Id*{referral.Id}*Ubrn*{referral.Ubrn}*Status*" +
        $"{nameof(ReferralStatus.RejectedToEreferrals)}*IsErsClosed = true*");
    }

    [Theory]
    [InlineData(NonMatchingId, MatchingUbrn)]
    [InlineData(MatchingId, NonMatchingUbrn)]
    public async Task NonMatchingParamsThrowsException(string id, string ubrn)
    {
      // Arrange.
      Guid idGuid = Guid.Parse(id);
      Guid matchingIdGuid = Guid.Parse(MatchingId);

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: matchingIdGuid,
        referralSource: ReferralSource.GpReferral,
        status: ReferralStatus.RejectedToEreferrals,
        isErsClosed: true,
        ubrn: MatchingUbrn);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      Func<Task<Business.Models.Referral>> result =
        () => _service.SetIsErsClosedToFalse(idGuid, ubrn);

      // Assert.
      await result.Should().ThrowAsync<ReferralNotFoundException>()
        .WithMessage($"*Id*{id}*Ubrn*{ubrn}*Status*{nameof(ReferralStatus.RejectedToEreferrals)}" +
        "*IsErsClosed = true*");
    }

    [Fact]
    public async Task ValidReferralInvalidIdThrowsException()
    {
      // Arrange.
      Guid id = Guid.Empty;

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.GpReferral,
        status: ReferralStatus.RejectedToEreferrals,
        isErsClosed: true);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      Func<Task<Business.Models.Referral>> result =
        () => _service.SetIsErsClosedToFalse(id, referral.Ubrn);

      // Assert.
      await result.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ValidReferralInvalidUbrnThrowsException()
    {
      // Arrange.
      string ubrn = " ";

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.GpReferral,
        status: ReferralStatus.RejectedToEreferrals,
        isErsClosed: true);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      Func<Task<Business.Models.Referral>> result =
        () => _service.SetIsErsClosedToFalse(referral.Id, ubrn);

      // Assert.
      await result.Should().ThrowAsync<ArgumentNullOrWhiteSpaceException>();
    }

    [Fact]
    public async Task ValidReferralValidParamsResetsIsErsClosedToFalse()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        referralSource: ReferralSource.GpReferral,
        status: ReferralStatus.RejectedToEreferrals,
        isErsClosed: true);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      Business.Models.Referral result =
        await _service.SetIsErsClosedToFalse(referral.Id, referral.Ubrn);

      // Assert.
      Referral storedReferral = _context.Referrals.Where(r => r.Id == referral.Id).SingleOrDefault();
      storedReferral.IsErsClosed.Should().BeFalse();
    }
  }

  public class SetMismatchedEthnicityToNullTests : ReferralAdminServiceTests
  {
    public SetMismatchedEthnicityToNullTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      if (!_context.Ethnicities.Any())
      {
        AServiceFixtureBase.PopulateEthnicities(_context);
      }
    }

    public override void Dispose()
    {
      base.Dispose();
      _context.Ethnicities.RemoveRange(_context.Ethnicities);
      GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task EmptyArrayThrowsArgumentOutOfRangeException()
    {
      // Arrange.
      Guid[] ids = [];

      // Act.
      Func<Task<List<Guid>>> result = () => _service.SetMismatchedEthnicityToNull(ids);

      // Assert.
      await result.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task MatchingEthnicityIsNotUpdated()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      Guid[] ids = [referralId];

      Entities.Ethnicity ethnicity = await _context.Ethnicities.FirstAsync();

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: referralId,
        ethnicity: ethnicity.TriageName,
        serviceUserEthnicity: ethnicity.DisplayName,
        serviceUserEthnicityGroup: ethnicity.GroupName);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      // Act.
      List<Guid> result = await _service.SetMismatchedEthnicityToNull(ids);

      // Assert.
      result.Should().BeEmpty();
      Referral storedReferral = await _context.Referrals
        .Where(r => r.Id == referralId)
        .SingleAsync();
      storedReferral.Ethnicity.Should().Be(ethnicity.TriageName);
    }

    [Fact]
    public async Task MismatchedEthnicityIsSetToNull()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      Guid[] ids = [referralId];

      Entities.Ethnicity ethnicity = await _context.Ethnicities.FirstAsync();

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: referralId,
        ethnicity: "Mismatch",
        serviceUserEthnicity: ethnicity.DisplayName,
        serviceUserEthnicityGroup: ethnicity.GroupName);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      // Act.
      List<Guid> result = await _service.SetMismatchedEthnicityToNull(ids);

      // Assert.
      result.Should().OnlyContain(x => x.Equals(referralId));
      Referral storedReferral = await _context.Referrals
        .Where(r => r.Id == referralId)
        .SingleAsync();
      storedReferral.Ethnicity.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task NullArrayThrowsArgumentNullException()
    {
      // Arrange.

      // Act.
      Func<Task<List<Guid>>> result = () => _service.SetMismatchedEthnicityToNull(null);

      // Assert.
      await result.Should().ThrowAsync<ArgumentNullException>();
    }
  }

  public class UpdateMskReferringOrganisationOdsCodeTests :
    ReferralAdminServiceTests
  {
    private const string CURRENT_ODS_CODE = "ABCDE";
    private const string NEW_ODS_CODE = "FGHIJ";

    public UpdateMskReferringOrganisationOdsCodeTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.MskOrganisations.Add(new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = CURRENT_ODS_CODE,
        SendDischargeLetters = true,
        SiteName = "Current Site"
      });

      _context.MskOrganisations.Add(new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = NEW_ODS_CODE,
        SendDischargeLetters = true,
        SiteName = "New Site"
      });

      _context.SaveChanges();
    }

    [Theory]
    [InlineData(CURRENT_ODS_CODE, "")]
    [InlineData("", NEW_ODS_CODE)]
    public async Task MissingOdsCodesThrowsArgumentException(
      string currentOdsCode,
      string newOdsCode)
    {
      // Arrange.

      // Act.
      try
      {
        await _service.UpdateMskReferringOrganisationOdsCode(
          currentOdsCode,
          newOdsCode);
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.Should().BeOfType(typeof(ArgumentException));
          ex.Message.Should().Be("Both ODS codes must be provided.");
        }
      }
    }

    [Theory]
    [InlineData(CURRENT_ODS_CODE, "VWXYZ")]
    [InlineData("VWXYZ", NEW_ODS_CODE)]
    public async Task OdsCodeNotPresentInMskOrganisations_ThrowsException(
      string currentOdsCode,
      string newOdsCode)
    {
      // Arrange.
      string nonPresentOdsCode = "VWXYZ";

      // Act.
      try
      {
        await _service.UpdateMskReferringOrganisationOdsCode(
          currentOdsCode,
          newOdsCode);
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.Should().BeOfType(typeof(MskOrganisationNotFoundException));
          ex.Message.Should().Be("No MskOrganisation found" +
            $" with OdsCode {nonPresentOdsCode}. No referrals were updated.");
        }
      }
    }

    [Fact]
    public async Task ValidOdsCodes_ReferralUpdated()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral();
      referral.ReferringOrganisationOdsCode = CURRENT_ODS_CODE;

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      List<Guid> referralIds = await _service
        .UpdateMskReferringOrganisationOdsCode(CURRENT_ODS_CODE, NEW_ODS_CODE);

      // Assert.
      using (new AssertionScope())
      {
        referralIds.Should().HaveCount(1);
        referralIds.Single().Should().Be(referral.Id);
        _context.Referrals
          .Single(r => r.Id == referral.Id)
          .ReferringOrganisationOdsCode
          .Should().Be(NEW_ODS_CODE);
      }
        
    }
    
  }
}
