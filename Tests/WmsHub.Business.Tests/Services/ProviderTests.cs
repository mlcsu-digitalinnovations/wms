using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralStatusReason;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Services;
using WmsHub.Common.Extensions;
using WmsHub.Common.Validation;
using Xunit;
using Xunit.Abstractions;
using Referral = WmsHub.Business.Entities.Referral;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class ProviderServiceTests : ServiceTestsBase
{
  private readonly DatabaseContext _context;
  private ProviderService _service;
  private readonly Entities.Provider _provider;
  private readonly Guid _referralId;
  private const string _ubrn = "120000000001";

  private readonly ProviderOptions _options = new ProviderOptions
  { CompletionDays = 84, NumDaysPastCompletedDate = 10 };

  private readonly Mock<IOptions<ProviderOptions>> _mockOptions =
    new Mock<IOptions<ProviderOptions>>();


  public ProviderServiceTests(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper) 
    : base(serviceFixture, testOutputHelper)
  {
    string completionDays = Environment.GetEnvironmentVariable(
      "WmsHub_Provider_Api_ProviderOptions:CompletionDays");

    if (int.TryParse(completionDays, out int cd))
      _options.CompletionDays = cd;

    string numDaysPastCompletedDate = Environment.GetEnvironmentVariable(
      "WmsHub_Provider_Api_ProviderOptions:NumDaysPastCompletedDate");

    if (int.TryParse(numDaysPastCompletedDate, out int ndpc))
      _options.NumDaysPastCompletedDate = ndpc;

    _mockOptions.Setup(x => x.Value).Returns(_options);
    _context = new DatabaseContext(_serviceFixture.Options);
    _service = new ProviderService(_context,
      _serviceFixture.Mapper,
      _mockOptions.Object)
    {
      User = GetClaimsPrincipal()
    };

    _provider = _context.Providers.FirstOrDefault();

    if (_provider == null)
    {
      _provider = RandomEntityCreator.CreateRandomProvider(
        id: Guid.Parse(TEST_USER_ID),
        isActive: true,
        isLevel1: true);

      _context.Providers.Add(_provider);
      _context.SaveChanges();
    }


    _referralId = AddFakeReferral(_ubrn, _provider.Id);
  }

  private Guid AddFakeReferral(
    string ubrn, 
    Guid providerId,
    ReferralStatus status = ReferralStatus.ProviderAwaitingStart,
    DateTimeOffset? dateProviderSelection = null,
    string providerUbrn = null)
  {
    var referral =
      _context.Referrals.SingleOrDefault(t => t.Ubrn == ubrn);

    if (referral == null)
    {

      referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: dateProviderSelection ?? DateTimeOffset.Now,
        modifiedByUserId: providerId,
        providerId: providerId,
        providerUbrn: providerUbrn ?? ubrn,
        referringGpPracticeName: providerId.ToString(),
        status: status,
        triagedCompletionLevel: "1",
        ubrn: ubrn);

      _context.Referrals.Add(referral);

      _context.SaveChanges();

    }

    return referral.Id;
  }

  private async Task RemoveReferralById(Guid id)
  {
    var entity =
      await _context.Referrals.SingleOrDefaultAsync(t => t.Id == id);
    if (entity == null) return;

    _context.Referrals.Remove(entity);
    await _context.SaveChangesAsync();
  }

  private async Task AddFakeProvider(Guid providerId)
  {
    Entities.Provider provider =
      ServiceFixture.CreateProviderWithNoAuth(providerId);

    var providerToUse = await _context.Providers
      .Include(p => p.ProviderSubmissions)
      .Where(d => d.Id == providerId)
      .SingleOrDefaultAsync();
    if (providerToUse == null)
    {
      _context.Providers.Add(provider);
    }

    await _context.SaveChangesAsync();
  }

  private async Task RemoveFakeProvider(Guid id)
  {
    var provider =
      await _context.Providers.SingleOrDefaultAsync(t => t.Id == id);
    if (provider is null) return;
    _context.Providers.Remove(provider);
    await _context.SaveChangesAsync();
  }

  public class GetProvidersAsync : ProviderServiceTests
  {
    public GetProvidersAsync(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task GetServicesForLowTriageLevel()
    {
      // arrange
      int expectedNoOfProviders = _context.Providers
       .Where(p => p.IsActive)
       .Where(p => p.Level1 == true)
       .Count();

      // act
      IEnumerable<Provider> providers =
        await _service.GetProvidersAsync((int)Enums.TriageLevel.Low);

      // assert
      providers.Count().Should().Be(expectedNoOfProviders);
    }

    [Fact]
    public async Task GetServicesForMediumTriageLevel()
    {
      // arrange
      int expectedNoOfProviders = _context.Providers
       .Where(p => p.IsActive)
       .Where(p => p.Level2 == true)
       .Count();

      // act
      IEnumerable<Provider> providers =
        await _service.GetProvidersAsync((int)Enums.TriageLevel.Medium);

      // assert
      providers.Count().Should().Be(expectedNoOfProviders);
    }

    [Fact]
    public async Task GetServicesForHighTriageLevel()
    {
      // arrange
      int expectedNoOfProviders = _context.Providers
       .Where(p => p.IsActive)
       .Where(p => p.Level3 == true)
       .Count();

      // act
      IEnumerable<Provider> providers =
        await _service.GetProvidersAsync((int)Enums.TriageLevel.High);

      // assert
      providers.Count().Should().Be(expectedNoOfProviders);
    }

    [Fact]
    public async Task GetServicesForUndeterminedTriageLevel()
    {
      // arrange
      int expectedNoOfProviders = _context.Providers
       .Where(p => p.IsActive)
       .Where(p => p.Level1 == true)
       .Count();

      // act
      IEnumerable<Provider> providers =
        await _service.GetProvidersAsync((int)Enums.TriageLevel
         .Undetermined);

      // assert
      providers.Count().Should().Be(expectedNoOfProviders);
    }

    [Fact]
    public async Task GetProvidersAsync_UnexpectedEnumValueException()
    {
      //Arrange 
      int triage = 10;

      // Act & Assert
      UnexpectedEnumValueException exception =
        await Assert.ThrowsAsync<UnexpectedEnumValueException>(
        async () => await _service.GetProvidersAsync(triage));
    }
  }

  public class GetServiceUsers : ProviderServiceTests
  {
    public GetServiceUsers(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task ForEachType()
    {
      //Arrange
      Guid providerId = Guid.NewGuid();

      _service = new ProviderService(_context,
        _serviceFixture.Mapper,
        _mockOptions.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };
      var ubrn = "7777666600000001";

      foreach (ReferralStatus status
        in Enum.GetValues(typeof(ReferralStatus)))
      {
        var referralId = AddFakeReferral(ubrn, providerId, status);

        //Act
        IEnumerable<ServiceUser> results = await _service.GetServiceUsers();

        //Assert
        if (status == ReferralStatus.ProviderAwaitingStart)
        {
          results.Count().Should().Be(1);
        }
        else
        {

          results.Any().Should().BeFalse();
        }

        await RemoveReferralById(referralId);
      }
    }



    [Fact]
    public async Task ForEachType_order()
    {
      //Arrange
      Guid providerId = Guid.NewGuid();

      _service = new ProviderService(_context,
        _serviceFixture.Mapper,
        _mockOptions.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };
      var ubrn1 = "7777666600000001";
      var ubrn2 = "7777666600000002";

      foreach (ReferralStatus status
        in Enum.GetValues(typeof(ReferralStatus)))
      {
        var referralId = AddFakeReferral(ubrn1, providerId, status);
        var referralIdA = AddFakeReferral(ubrn2, providerId, status);

        //Act
        IEnumerable<ServiceUser> results = await _service.GetServiceUsers();

        //Assert
        if (status == ReferralStatus.ProviderAwaitingStart)
        {
          results.Count().Should().Be(2);
        }
        else
        {

          results.Any().Should().BeFalse();
        }

        await RemoveReferralById(referralId);
        await RemoveReferralById(referralIdA);
      }
    }
  }

  public class AddProviderSubmissionsAsyncTests : ProviderServiceTests, IDisposable
  {      
    private Guid _providerId;
    private Guid _referralIdToTest;
    private Referral _referral;

    public AddProviderSubmissionsAsyncTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      SetUp();
    }
    public async void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      await _context.SaveChangesAsync();
    }

    [Theory]
    [InlineData("Declined", "Not able to contact in 28 days")]
    [InlineData("Rejected", "Reason why the service user was rejected")]
    public async Task ProgrammeOutcomeUpdatedToDidNotCommence(
      string type,
      string reason)
    {
      // Arrange.
      _referral.Status = ReferralStatus.ProviderAwaitingStart.ToString();
      await _context.SaveChangesAsync();

      ServiceUserSubmissionRequest serviceUserSubmissionRequest = new()
      {
        Date = DateTime.Now,
        Reason = reason,
        Type = type,
        Ubrn = _referral.ProviderUbrn
      };

      ProviderSubmissionRequest providerSubmissionRequest =
        new(serviceUserSubmissionRequest, _providerId, _referral.Id);

      // Act.
      await _service.AddProviderSubmissionsAsync(
        providerSubmissionRequest,
        _referral.Id);

      // Assert.
      _context.Referrals.Single(r => r.Id == _referral.Id).ProgrammeOutcome
        .Should().Be(ProgrammeOutcome.DidNotCommence.ToString());
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task ProviderCompleted_AwaitingDischarge(
      ReferralSource referralSource)
    {
      // Arrange.
      _referral.Status = ReferralStatus.ProviderStarted.ToString();
      _referral.ReferralSource = referralSource.ToString();
      _context.SaveChanges();

      ServiceUserSubmissionRequest serviceUserSubmissionRequest = new()
      {
        Date = DateTimeOffset.Now,
        Type = UpdateType.Completed.ToString(),
        Ubrn = _ubrn
      };

      ProviderSubmissionRequest providerSubmissionRequest = new(
        serviceUserSubmissionRequest,
        Guid.Parse(TEST_USER_ID),
        _referral.Id);

      // Act.
      await _service.AddProviderSubmissionsAsync(
        providerSubmissionRequest,
        _referral.Id);

      // Assert.
      _referral.Status.Should()
        .Be(ReferralStatus.ProviderCompleted.ToString());
    }

    [Fact]
    public async Task ProviderAcceptedAllowed_NullException()
    {
      // Arrange.
      List<Entities.Provider> providers = await _context.Providers
          .Where(p => p.Id == Guid.Parse(TEST_USER_ID))
          .ToListAsync();
      _context.Providers.RemoveRange(providers);
      await _context.SaveChangesAsync();

    string json = ValidTestModels.SubmissionAccepted;
    ServiceUserSubmissionRequest[] requests = JsonConvert
      .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      requests[0].Ubrn = _ubrn;
      ProviderSubmissionRequest request = new(
        requests[0],
        Guid.Parse(TEST_USER_ID),
        _referralIdToTest);

      // Act & Assert.
      ProviderNotFoundException exception =
        await Assert.ThrowsAsync<ProviderNotFoundException>(
        async () =>
        await _service.AddProviderSubmissionsAsync(request, Guid.NewGuid()));
    }

    [Fact]
    public async Task ProviderAcceptedAllowed_ReferralException()
    {
      // Arrange.
      string json = ValidTestModels.SubmissionAccepted;
      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      requests[0].Ubrn = _ubrn;
      ProviderSubmissionRequest request = new(
        requests[0],
        Guid.Parse(TEST_USER_ID),
        _referralIdToTest);

      // Act & Assert.
      ReferralNotFoundException exception =
        await Assert.ThrowsAsync<ReferralNotFoundException>(
        async () =>
        await _service.AddProviderSubmissionsAsync(request, Guid.NewGuid()));
    }

      [Theory]
      [InlineData(ReferralStatus.New)]
      [InlineData(ReferralStatus.CancelledByEreferrals)]
      [InlineData(ReferralStatus.ChatBotCall1)]
      [InlineData(ReferralStatus.ChatBotTransfer)]
      [InlineData(ReferralStatus.Complete)]
      [InlineData(ReferralStatus.Exception)]
      [InlineData(ReferralStatus.ProviderStarted)]
      [InlineData(ReferralStatus.ProviderContactedServiceUser)]
      [InlineData(ReferralStatus.ProviderRejected)]
      [InlineData(ReferralStatus.ProviderTerminated)]
      [InlineData(ReferralStatus.RmcCall)]
      [InlineData(ReferralStatus.RmcDelayed)]
      [InlineData(ReferralStatus.Letter)]
      public async Task ProviderAcceptedExpectedException(
        ReferralStatus status)
      {
        // Arrange.
        _referral.Status = status.ToString();
        await _context.SaveChangesAsync();

    string json = ValidTestModels.SubmissionAccepted;

    ServiceUserSubmissionRequest[] requests = JsonConvert
      .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      requests[0].Ubrn = _ubrn;
      ProviderSubmissionRequest request = new(
        requests[0],
        Guid.Parse(TEST_USER_ID),
        _referralIdToTest);

      // Act and Assert.
      try
      {
        bool results =
          await _service.AddProviderSubmissionsAsync(request,
          _referralIdToTest);
        Assert.Fail("StatusChangeException expected");
      }
      catch (StatusChangeException e)
      {
        Assert.True(true, e.Message);
      }
      catch (Exception ex)
      {
        Assert.Fail(ex.Message);
      }
    }

    [Fact]
    public async Task ProviderAcceptedAllowed()
    {
      // Arrange.
      ReferralStatus status = ReferralStatus.ProviderAwaitingStart;
      _referral.Status = status.ToString();
      await _context.SaveChangesAsync();

    string json = ValidTestModels.SubmissionAccepted;

    ServiceUserSubmissionRequest[] requests = JsonConvert
      .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      requests[0].Ubrn = _ubrn;
      ProviderSubmissionRequest request = new(
        requests[0],
        Guid.Parse(TEST_USER_ID),
        _referralIdToTest);

      // Act.
      bool results =
        await _service.AddProviderSubmissionsAsync(
          request,
          _referralIdToTest);

      // Assert.
      Assert.True(results, status.ToString());
    }

    [Theory]
    [InlineData(ReferralSource.GpReferral,
      ReferralStatus.ProviderTerminated)]
    [InlineData(ReferralSource.GeneralReferral,
      ReferralStatus.ProviderTerminatedTextMessage)]
    [InlineData(ReferralSource.Pharmacy,
      ReferralStatus.ProviderTerminatedTextMessage)]
    [InlineData(ReferralSource.SelfReferral,
      ReferralStatus.ProviderTerminatedTextMessage)]
    [InlineData(ReferralSource.Msk,
      ReferralStatus.ProviderTerminatedTextMessage)]
    public async Task ProviderTerminated(
      ReferralSource referralSource, ReferralStatus expectedStatus)
    {
      // Arrange.
      string json = ValidTestModels.SubmissionTerminated;

    ServiceUserSubmissionRequest[] requests = JsonConvert
      .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

    requests[0].Ubrn = _ubrn;

      ProviderSubmissionRequest request = new(
        requests[0],
        Guid.Parse(TEST_USER_ID),
        _referralIdToTest);

    string expectedStatusReason = requests[0].Reason;

    Referral referral = _context.Referrals
      .Where(r => r.Id == _referralIdToTest)
      .SingleOrDefault();

    referral.Status = ReferralStatus.ProviderStarted.ToString();
    referral.ReferralSource = referralSource.ToString();

    _context.SaveChanges();

      // Act.
      bool result = await _service
        .AddProviderSubmissionsAsync(request, _referralIdToTest);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeTrue();
        referral.Status.Should().Be(expectedStatus.ToString());
        referral.StatusReason.Should().Be(expectedStatusReason);
      }
    }

      [Theory]
      [InlineData(ReferralStatus.New)]
      [InlineData(ReferralStatus.CancelledByEreferrals)]
      [InlineData(ReferralStatus.ChatBotCall1)]      
      [InlineData(ReferralStatus.Complete)]
      [InlineData(ReferralStatus.Exception)]
      [InlineData(ReferralStatus.ProviderStarted)]
      [InlineData(ReferralStatus.ProviderContactedServiceUser)]
      [InlineData(ReferralStatus.ProviderRejected)]
      [InlineData(ReferralStatus.RmcCall)]
      [InlineData(ReferralStatus.RmcDelayed)]
      [InlineData(ReferralStatus.Letter)]
      public async Task ProviderContactedExpectedException(
        ReferralStatus status)
      {
        // Arrange.
        _referral.Status = status.ToString();
        await _context.SaveChangesAsync();

    string json = ValidTestModels.SubmissionContacted;

    ServiceUserSubmissionRequest[] requests = JsonConvert
      .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      requests[0].Ubrn = _ubrn;
      ProviderSubmissionRequest request = new(
        requests[0],
        Guid.Parse(TEST_USER_ID),
        _referralIdToTest);

      // Act and Assert.
      try
      {
        bool results = await _service.AddProviderSubmissionsAsync(request,
          _referralIdToTest);
        Assert.Fail("StatusChangeException expected");
      }
      catch (StatusChangeException e)
      {
        Assert.True(true, e.Message);
      }
      catch (Exception ex)
      {
        Assert.Fail(ex.Message);
      }
    }

      [Theory]
      [InlineData(ReferralStatus.New)]
      [InlineData(ReferralStatus.CancelledByEreferrals)]
      [InlineData(ReferralStatus.ChatBotCall1)]
      [InlineData(ReferralStatus.Complete)]
      [InlineData(ReferralStatus.Exception)]
      [InlineData(ReferralStatus.ProviderStarted)]
      [InlineData(ReferralStatus.ProviderRejected)]
      [InlineData(ReferralStatus.RmcCall)]
      [InlineData(ReferralStatus.RmcDelayed)]
      [InlineData(ReferralStatus.Letter)]
      public async Task ProviderDeclinedByServiceUser_ExpectedException(
      ReferralStatus status)
      {
        // Arrange.
        _referral.Status = status.ToString();
        await _context.SaveChangesAsync();

    string json = ValidTestModels.SubmissionContacted;

    ServiceUserSubmissionRequest[] requests = JsonConvert
      .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      requests[0].Ubrn = _ubrn;
      ProviderSubmissionRequest request = new(
        requests[0],
        Guid.Parse(TEST_USER_ID),
        _referralIdToTest);
      request.ReferralStatus = ReferralStatus.ProviderDeclinedByServiceUser;

      // Act and Assert.
      try
      {
        bool results = await _service.AddProviderSubmissionsAsync(request,
          _referralIdToTest);
        Assert.Fail("StatusChangeException expected");
      }
      catch (StatusChangeException e)
      {
        Assert.True(true, e.Message);
      }
      catch (Exception ex)
      {
        Assert.Fail(ex.Message);
      }
    }

      [Theory]
      [InlineData(ReferralStatus.New)]
      [InlineData(ReferralStatus.CancelledByEreferrals)]
      [InlineData(ReferralStatus.ChatBotCall1)]
      [InlineData(ReferralStatus.Complete)]
      [InlineData(ReferralStatus.Exception)]
      [InlineData(ReferralStatus.ProviderStarted)]
      [InlineData(ReferralStatus.ProviderRejected)]
      [InlineData(ReferralStatus.RmcCall)]
      [InlineData(ReferralStatus.RmcDelayed)]
      [InlineData(ReferralStatus.Letter)]
      public async Task ProviderRejected_ExpectedException(
      ReferralStatus status)
      {
        // Arrange.
        _referral.Status = status.ToString();
        await _context.SaveChangesAsync();

    string json = ValidTestModels.SubmissionContacted;

    ServiceUserSubmissionRequest[] requests = JsonConvert
      .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      requests[0].Ubrn = _ubrn;
      ProviderSubmissionRequest request = new(
        requests[0],
        Guid.Parse(TEST_USER_ID),
        _referralIdToTest);
      request.ReferralStatus = ReferralStatus.ProviderRejected;

      //Act and Assert.
      try
      {
        bool results = await _service.AddProviderSubmissionsAsync(request,
          _referralIdToTest);
        Assert.Fail("StatusChangeException expected");
      }
      catch (StatusChangeException e)
      {
        Assert.True(true, e.Message);
      }
      catch (Exception ex)
      {
        Assert.Fail(ex.Message);
      }
    }

      [Theory]
      [InlineData(ReferralStatus.New)]
      [InlineData(ReferralStatus.CancelledByEreferrals)]
      [InlineData(ReferralStatus.ChatBotCall1)]
      [InlineData(ReferralStatus.Complete)]
      [InlineData(ReferralStatus.Exception)]
      [InlineData(ReferralStatus.ProviderStarted)]
      [InlineData(ReferralStatus.ProviderRejected)]
      [InlineData(ReferralStatus.RmcCall)]
      [InlineData(ReferralStatus.RmcDelayed)]
      [InlineData(ReferralStatus.Letter)]
      public async Task ProviderStarted_ExpectedException(
      ReferralStatus status)
      {
        // Arrange.
        _referral.Status = status.ToString();
        await _context.SaveChangesAsync();

    string json = ValidTestModels.SubmissionContacted;

    ServiceUserSubmissionRequest[] requests = JsonConvert
      .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      requests[0].Ubrn = _ubrn;
      ProviderSubmissionRequest request = new(
        requests[0],
        Guid.Parse(TEST_USER_ID),
        _referralIdToTest);
      request.ReferralStatus = ReferralStatus.ProviderStarted;

      //Act and Assert.
      try
      {
        bool results = await _service.AddProviderSubmissionsAsync(request,
          _referralIdToTest);
        Assert.Fail("StatusChangeException expected");
      }
      catch (StatusChangeException e)
      {
        Assert.True(true, e.Message);
      }
      catch (Exception ex)
      {
        Assert.Fail(ex.Message);
      }
    }

      [Theory]
      [InlineData(ReferralStatus.New)]
      [InlineData(ReferralStatus.CancelledByEreferrals)]
      [InlineData(ReferralStatus.ChatBotCall1)]
      [InlineData(ReferralStatus.Complete)]
      [InlineData(ReferralStatus.Exception)]
      [InlineData(ReferralStatus.ProviderContactedServiceUser)]
      [InlineData(ReferralStatus.ProviderRejected)]
      [InlineData(ReferralStatus.RmcCall)]
      [InlineData(ReferralStatus.RmcDelayed)]
      [InlineData(ReferralStatus.Letter)]
      public async Task ProviderCompleted_ExpectedException(
      ReferralStatus status)
      {
        // Arrange.
        _referral.Status = status.ToString();
        await _context.SaveChangesAsync();

    string json = ValidTestModels.SubmissionContacted;

    ServiceUserSubmissionRequest[] requests = JsonConvert
      .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      requests[0].Ubrn = _ubrn;
      ProviderSubmissionRequest request = new(
        requests[0],
        Guid.Parse(TEST_USER_ID),
        _referralIdToTest);
      request.ReferralStatus = ReferralStatus.ProviderCompleted;

      // Act and Assert.
      try
      {
        bool results = await _service.AddProviderSubmissionsAsync(request,
          _referralIdToTest);
        Assert.Fail("StatusChangeException expected");
      }
      catch (StatusChangeException e)
      {
        Assert.True(true, e.Message);
      }
      catch (Exception ex)
      {
        Assert.Fail(ex.Message);
      }
    }

    [Fact]
    public async Task DuplicateSubmissionInBatch_StoresSingle()
    {
      // Arrange.
      _referral.Status = ReferralStatus.ProviderAwaitingStart.ToString();
      await _context.SaveChangesAsync();

      int coaching = 50;
      int measure = 10;
      int weight = 110;
      DateTime date = DateTime.Now;

      ServiceUserUpdatesRequest update = new()
      {
        Coaching = coaching,
        Measure = measure,
        Weight = weight,
        Date = date
      };

      ServiceUserSubmissionRequestV2 serviceUserSubmissionRequest = new()
      {
        Date = DateTimeOffset.Now,
        Type = "Update",
        Updates = new[]
        {
          update,
          update
        },
        Ubrn = _referral.ProviderUbrn
      };

      ProviderSubmissionRequest providerSubmissionRequest = new(
        serviceUserSubmissionRequest,
        Guid.NewGuid(),
        _referral.Id);

      // Act.
      bool result = await _service.AddProviderSubmissionsAsync(
        providerSubmissionRequest,
        _referral.Id);

      // Assert.
      result.Should().BeTrue();
      Entities.ProviderSubmission storedSubmission = _context.ProviderSubmissions.SingleOrDefault();

      storedSubmission.Should().NotBeNull();
      storedSubmission.Measure.Should().Be(measure);
      storedSubmission.Weight.Should().Be(weight);
      storedSubmission.Coaching.Should().Be(coaching);
      storedSubmission.Date.Should().Be(date);
      storedSubmission.Provider.Should().Be(_provider);
      storedSubmission.Referral.Should().Be(_referral);
    }

    private async void SetUp()
    {
      await AddFakeProvider(Guid.Parse(TEST_USER_ID));
      _providerId = Guid.Parse(TEST_USER_ID);
      _referralIdToTest = AddFakeReferral(_ubrn, _providerId);
      _referral = await _context.Referrals
        .SingleOrDefaultAsync((r => r.Id == _referralIdToTest));
    }
  }

  public class SubmissionTests : ProviderServiceTests
  {
    private readonly string _sid = "571342f1-c67d-49bf-a9c6-40a41e6dc702";
    private string _providerName;

    public SubmissionTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.Providers.RemoveRange(_context.Providers);
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.Add(ServiceFixture.CreateProviderWithReferrals());
      _context.SaveChanges();

      var l = _context.Referrals.Select(x => x.Ubrn).OrderBy(x => x).ToList();
      var d = l.Distinct();

      _providerName = _context.Providers
        .Where(p => p.Id == GetClaimsPrincipal().GetUserId())
        .Select(p => p.Name)
        .Single();
    }

    [Fact]
    public async Task UpdateTypeUpdateInvalidReferralStatus_ProviderStarted()
    {
      // arrange
      var ubrn = "824514414411";

      var mockService = ConfigMockProviderService(
        new ProviderOptions { IgnoreStatusRequirementForUpdate = false },
        ReferralStatus.ProviderStarted.ToString(),
        ubrn);

      var requests = new List<ServiceUserSubmissionRequest>
      {
        new ServiceUserSubmissionRequest
        {
          Date = DateTimeOffset.Now.Date.AddHours(12),
          Type = UpdateType.Update.ToString(),
          Ubrn = ubrn,
          Updates = new List<ServiceUserUpdatesRequest>
          {
            new ServiceUserUpdatesRequest
            {
              Date = DateTime.Now.Date.AddHours(13),
              Weight = 100
            }
          }
        }
      };

      // act
      var responses = await mockService.Object
        .ProviderSubmissionsAsync(requests);

      responses.Count().Should().Be(1);
      mockService.Verify(x => x.AddProviderSubmissionsAsync(
          It.IsAny<ProviderSubmissionRequest>(),
          It.IsAny<Guid>()),
        Times.Once);
      responses.First().ResponseStatus.Should().Be(StatusType.Valid);
    }


    [Theory]
    [MemberData(nameof(ReferralStatusStrings))]
    public async Task UpdateTypeUpdateInvalidReferralStatus(string status)
    {
      // arrange
      var ubrn = "824514414411";

      var mockService = ConfigMockProviderService(
        new ProviderOptions { IgnoreStatusRequirementForUpdate = false },
        status,
        ubrn);

      var requests = new List<ServiceUserSubmissionRequest>
      {
        new ServiceUserSubmissionRequest
        {
          Date = DateTimeOffset.Now.Date.AddHours(12),
          Type = UpdateType.Update.ToString(),
          Ubrn = ubrn,
          Updates = new List<ServiceUserUpdatesRequest>
          {
            new ServiceUserUpdatesRequest
            {
              Date = DateTime.Now.Date.AddHours(13),
              Weight = 100
            }
          }
        }
      };

      // act
      var responses = await mockService.Object
        .ProviderSubmissionsAsync(requests);

      responses.Count().Should().Be(1);
      if (status == ReferralStatus.ProviderStarted.ToString())
      {
        mockService.Verify(x => x.AddProviderSubmissionsAsync(
            It.IsAny<ProviderSubmissionRequest>(),
            It.IsAny<Guid>()),
          Times.Once);          
        responses.First().ResponseStatus.Should().Be(StatusType.Valid);
      }
      else
      {
        mockService.Verify(x => x.AddProviderSubmissionsAsync(
            It.IsAny<ProviderSubmissionRequest>(),
            It.IsAny<Guid>()),
          Times.Never);
        responses.First().ResponseStatus.Should().Be(StatusType.Invalid);
      }
    }

    [Theory]
    [MemberData(nameof(ReferralStatusStrings))]
    public async Task UpdateTypeUpdateInvalidReferralStatusIgnored(string status)
    {
      // arrange
      var ubrn = "824514414411";

      var mockService = ConfigMockProviderService(
        new ProviderOptions { IgnoreStatusRequirementForUpdate = true },
        status,
        ubrn);

      var requests = new List<ServiceUserSubmissionRequest>
      {
        new ServiceUserSubmissionRequest
        {
          Date = DateTimeOffset.Now.Date.AddHours(12),
          Type = UpdateType.Update.ToString(),
          Ubrn = ubrn,
          Updates = new List<ServiceUserUpdatesRequest>
          {
            new ServiceUserUpdatesRequest
            {
              Date = DateTime.Now.Date.AddHours(13),
              Weight = 100
            }
          }
        }
      };

      // act
      var responses = await mockService.Object
        .ProviderSubmissionsAsync(requests);

      mockService.Verify(x => x.AddProviderSubmissionsAsync(
          It.IsAny<ProviderSubmissionRequest>(),
          It.IsAny<Guid>()),
        Times.Once);
      responses.Count().Should().Be(1);
      responses.First().ResponseStatus.Should().Be(StatusType.Valid);
    }

    private static Mock<ProviderService> 
      ConfigMockProviderService(
      ProviderOptions option, string status, string ubrn)
    {
      var mockOptions = new Mock<IOptions<ProviderOptions>>();
      mockOptions
        .Setup(x => x.Value)
        .Returns(option);

      var mockService = new Mock<ProviderService>(
        null, null, mockOptions.Object);
      mockService.CallBase = true;

      mockService.Setup(x => x.User).Returns(GetClaimsPrincipal());

      mockService
        .Setup(x => x.GetProviderNameAsync(It.IsAny<Guid>()).Result)
        .Returns("Test");

      mockService.Protected()
        .Setup<ValidateModelResult>("ValidateModel", ItExpr.IsAny<object>())
        .Returns(new ValidateModelResult { IsValid = true });

      mockService.Protected()
        .Setup<Task>(
          "GetProviderReferralByProviderUbrn",
          ItExpr.IsAny<Guid>(),
          ItExpr.IsAny<string>())
        .Returns(Task.FromResult(new Referral
        {
          Id = Guid.NewGuid(),
          DateOfProviderSelection = DateTimeOffset.Now.Date,
          DateStartedProgramme = DateTimeOffset.Now.Date,
          Status = status,
          Ubrn = ubrn
        }));

      mockService
        .Setup<bool>(x => x.AddProviderSubmissionsAsync(
          It.IsAny<ProviderSubmissionRequest>(),
          It.IsAny<Guid>()).Result)
        .Returns(true)
        .Verifiable();

      return mockService;
    }

    [Fact]
    public async Task UpdateTypeUpdateInvalidReferralStatusIgnored1()
    {
      // arrange
      var mockOptions = new Mock<IOptions<ProviderOptions>>();
      mockOptions.Setup(x => x.Value).Returns(new ProviderOptions
      {
        IgnoreStatusRequirementForUpdate = true
      });

      var service = new ProviderService(
        _context,
        _serviceFixture.Mapper,
        mockOptions.Object)
      {
        User = GetClaimsPrincipal()
      };

      var ubrn = "824514414411";

      Referral referralToTest = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: DateTimeOffset.Now.Date.AddHours(10),
        dateStartedProgramme: DateTimeOffset.Now.Date.AddHours(11),
        status: ReferralStatus.New,
        providerId: GetClaimsPrincipal().GetUserId(),
        ubrn: ubrn
      );

      _context.Referrals.Add(referralToTest);
      await _context.DbContext.SaveChangesAsync();

      var request = new ServiceUserSubmissionRequest
      {
        Date = DateTimeOffset.Now.Date.AddHours(12),
        Type = UpdateType.Update.ToString(),
        Ubrn = ubrn,
        Updates = new List<ServiceUserUpdatesRequest>
        {
          new ServiceUserUpdatesRequest
          {
            Date = DateTime.Now.Date.AddHours(13),
            Weight = 100
          }
        }
      };

      //clean up
      _context.Referrals.Remove(referralToTest);
      await _context.SaveChangesAsync();
    }


    [Fact]
    public async Task RequestIsNull_Throw_ArgumentNullException()
    {
      //Arrange
      string error = "Value cannot be null. (Parameter 'requests')";
      IEnumerable<ServiceUserSubmissionRequest> requests = null;
      try
      {
        //Act
        IEnumerable<ServiceUserSubmissionResponse> responses =
          await _service.ProviderSubmissionsAsync(requests);
        //Assert
        Assert.Null(responses);
      }
      catch (ArgumentNullException ane)
      {
        Assert.Equal(error, ane.Message);
      }

    }

    [Fact]
    public async Task RequestIsEmpty_Throw_ArgumentOutOfRangeException()
    {
      //Arrange
      string error = "Specified argument was out of the range of valid" +
                     " values. (Parameter 'requests')";
      IEnumerable<ServiceUserSubmissionRequest> requests =
        new List<ServiceUserSubmissionRequest>();
      try
      {
        //Act
        IEnumerable<ServiceUserSubmissionResponse> responses =
          await _service.ProviderSubmissionsAsync(requests);
        //Assert
        Assert.Null(responses);
      }
      catch (ArgumentOutOfRangeException ane)
      {
        Assert.Equal(error, ane.Message);
      }
    }

    [Fact]
    public async Task ReferralIdNotFoundUsingUbrn_InvalidStatus()
    {
      //arrange
      string ubrn = "877666555444";
      var providerName = _context.Providers
        .Where(p => p.Id == Guid.Parse(_sid))
        .Select(p => p.Name)
        .Single();
      string error = $"UBRN {ubrn} not found for provider.";
      IEnumerable<ServiceUserSubmissionRequest> requests =
        new List<ServiceUserSubmissionRequest>
        {
          new ServiceUserSubmissionRequest
          {
            Date = DateTimeOffset.Now,
            Type = "Started",
            Ubrn = ubrn
          }
        };

      //act
      IEnumerable<ServiceUserSubmissionResponse> responses =
        await _service.ProviderSubmissionsAsync(requests);

      //Assert
      foreach (var response in responses)
      {
        Assert.Equal(error, response.GetErrorMessage());
        Assert.Equal(Enums.StatusType.Invalid, response.ResponseStatus);

      }
    }

    [Fact]
    public async Task AddSubmissionLate_Invalid()
    {
      //Arrange
      var ubrn = "824514414414";
      Referral referralToTest = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderContactedServiceUser: null,
        providerId: GetClaimsPrincipal().GetUserId(),
        providerUbrn: ubrn,
        ubrn: ubrn
      );

      _context.Referrals.Add(referralToTest);
      await _context.DbContext.SaveChangesAsync();

      var expectedErrorStart =
        $"UBRN {ubrn} Date Of Provider " +
        $"Selection in referral is null";

      var json = InvalidTestModels.SubmissionCompletedWithUpdatesOutOfDate;

      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      // THERE IS A HACK HERE TO GET IT TO WORK.
      // THE TEST REFERRALS NEED REFACTORING
      _context.Referrals.Where(r => r.Ubrn == ubrn).ToList().ForEach(
        referral =>
        {
          var start = DateTimeOffset.Now
            .AddDays(-(_options.MaxCompletionDays + 10));
          var completed = DateTimeOffset.Now
            .AddDays(-(_options.NumDaysPastCompletedDate * 2));

          referral.DateStartedProgramme = start;
          referral.DateCompletedProgramme = completed;
          referral.Status = ReferralStatus.ProviderStarted.ToString();
        }
      );

      _context.SaveChanges();

      foreach (var request in requests)
      {
        request.Ubrn = ubrn;
        request.Date = DateTimeOffset.Now;
        foreach (var update in request.Updates)
        {
          update.Date = DateTime.UtcNow;
        }
      }

      //act
      IEnumerable<ServiceUserSubmissionResponse> responses =
        await _service.ProviderSubmissionsAsync(requests);

      //Assert
      foreach (var response in responses)
      {
        if (response.Errors.Any())
          _log.Debug(response.GetErrorMessage());

        response.GetErrorMessage().Should().StartWith(expectedErrorStart);
        response.ResponseStatus.Should().Be(StatusType.Invalid);
      }

      //clean up
      _context.Referrals.Remove(referralToTest);
      await _context.SaveChangesAsync();
    }

    [Theory]
    [InlineData(ValidTestModels.SubmissionStarted1
      , Enums.StatusType.Valid,
      "")]
    [InlineData(ValidTestModels.SubmissionStartedWithUpdates1
      , Enums.StatusType.Valid,
      "")]
    [InlineData(ValidTestModels.SubmissionRejected1
      , Enums.StatusType.Valid,
      "")]
    [InlineData(ValidTestModels.SubmissionRejected2
      , Enums.StatusType.Valid,
      "")]
    [InlineData(ValidTestModels.SubmissionUpdate
      , Enums.StatusType.Valid,
      "")]
    [InlineData(ValidTestModels.SubmissionComplete1
      , Enums.StatusType.Invalid,
      "UBRN {0} cannot accept some of the update dates " +
      "because the date of provider selection for this referral {2} is " +
      "after some of the dates provided.")]
    [InlineData(ValidTestModels.SubmissionCompletedWithUpdates
      , Enums.StatusType.Invalid,
      "UBRN {0} cannot accept some of the update dates " +
      "because the date of provider selection for this referral {2} is " +
      "after some of the dates provided.")]
    [InlineData(ValidTestModels.SumbmissionTerminated
      , Enums.StatusType.Invalid,
      "UBRN {0} cannot accept some of the update dates " +
      "because the date of provider selection for this referral {2} is " +
      "after some of the dates provided.")]
    [InlineData(InvalidTestModels.SubmissionComplete1
      , Enums.StatusType.Invalid,
      "UBRN {0} cannot accept some of the update dates " +
      "because the date of provider selection for this referral {2} is " +
      "after some of the dates provided.")]
    [InlineData(InvalidTestModels.SubmissionCompletedWithUpdates
      , Enums.StatusType.Invalid,
      "UBRN {0} cannot accept some of the update dates " +
      "because the date of provider selection for this referral {2} is " +
      "after some of the dates provided.")]
    [InlineData(InvalidTestModels.SubmissionRejectedMissingReason
      , Enums.StatusType.Invalid,
      "UBRN {0} The Reason field '' is invalid.")]
    [InlineData(InvalidTestModels.SubmissionStartDateBeforeReferralDate
      , StatusType.Invalid,
      "UBRN {0} cannot accept some of the update dates " +
      "because the date of provider selection for this referral {2} is " +
      "after some of the dates provided.")]
    [InlineData(InvalidTestModels.SubmissionUpdatesDateBeforeReferralDate
      , StatusType.Invalid,
      "UBRN {0} cannot accept some of the update dates " +
      "because the date of provider selection for this referral {2} is " +
      "after some of the dates provided.")]
    public async Task NewProviderSubmissionRequestValidations(string json,
      Enums.StatusType status, string error)
    {
      //arrange
      string expectedError = "";
      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      foreach (ServiceUserSubmissionRequest request in requests)
      {
        if (status == StatusType.Invalid)
        {
          Referral entity = await _context.Referrals
            .FirstOrDefaultAsync(t => t.Ubrn.Equals(request.Ubrn));

          if (entity == null)
          {
            Assert.Fail($"Enitity for UBRN {request.Ubrn} Not found for testing");
          }

          entity.DateOfProviderSelection =
            entity.DateOfReferral.Value.AddDays(5);
          await _context.SaveChangesAsync();

          expectedError =
          string.Format(error,
            entity.Ubrn,
            _sid,
            entity.DateOfProviderSelection);

          if (string.IsNullOrWhiteSpace(expectedError))
          {
            expectedError = string.Format(
              error, 
              entity.Ubrn, 
              entity.ProviderId);
          }
        }
        else if (status == StatusType.Valid)
        {
          Referral entity =
            await _context.Referrals.FirstOrDefaultAsync(t =>
              t.Ubrn.Equals(request.Ubrn));

          if (entity == null)
          {
            continue;
          }
          request.Date = DateTimeOffset.Now;
          if (request.Updates.Any())
          {
            request.Updates.ToList().ForEach(t => t.Date = DateTime.Now);
          }
          entity.DateOfProviderSelection = DateTimeOffset.Now.AddDays(-1);
          await _context.SaveChangesAsync();
        }
      }


      //act
      IEnumerable<ServiceUserSubmissionResponse> responses =
        await _service.ProviderSubmissionsAsync(requests);

      //Assert
      foreach (var response in responses)
      {

        if (response.ResponseStatus == StatusType.Valid)
        {
          response.Errors.Count.Should().Be(0);
          response.ResponseStatus.Should().Be(status);
        }
        else
        {
          _log.Information(response.GetErrorMessage());
          response.Errors.Count.Should().BeGreaterThan(0);
          response.ResponseStatus.Should().Be(status);
          response.Errors.First().Should().Be(expectedError);
        }
      }
    }

    [Fact]
    public async Task NewProviderSubmissionUpdatesDate_InValid()
    {
      //arrange
      var ubrn = "824514414415";
      Referral referralToTest = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderContactedServiceUser: null,
        providerId: GetClaimsPrincipal().GetUserId(),
        ubrn: ubrn,
        status: ReferralStatus.ProviderAwaitingStart
      );

      _context.Referrals.Add(referralToTest);
      await _context.DbContext.SaveChangesAsync();

      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(InvalidTestModels
          .SubmissionUpdatesDateBeforeReferralDate);

      foreach (ServiceUserSubmissionRequest request in requests)
      {

        Referral entity =
          await _context.Referrals.FirstOrDefaultAsync(t =>
            t.Ubrn.Equals(request.Ubrn));

        if (entity == null) continue;

        //set start date to request date as validating updates
        entity.DateOfProviderSelection = request.Date;
        await _context.SaveChangesAsync();

      }

      //act
      IEnumerable<ServiceUserSubmissionResponse> responses =
        await _service.ProviderSubmissionsAsync(requests);

      //Assert
      foreach (var response in responses)
      {
        if (response.ResponseStatus != StatusType.Valid)
          _log.Information(response.GetErrorMessage());

        response.ResponseStatus.Should().Be(StatusType.Invalid);
      }
    }

    [Fact]
    public async Task NewProviderSubmissionUpdatesDate_IsValid()
    {
      //arrange
      List<Guid> referralIds = new List<Guid>();
      Guid providerId = Guid.NewGuid();

      _service = new ProviderService(_context,
        _serviceFixture.Mapper,
        _mockOptions.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };

      await AddFakeProvider(providerId);

      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(ValidTestModels
          .SubmissionUpdatesAfterBeforeReferralDate);

      foreach (ServiceUserSubmissionRequest request in requests)
      {
        referralIds.Add(AddFakeReferral(request.Ubrn, providerId,
          ReferralStatus.ProviderAwaitingStart,
          request.Date.Value.AddDays(-1)));
      }

      //act
      IEnumerable<ServiceUserSubmissionResponse> responses =
        await _service.ProviderSubmissionsAsync(requests);

      //Assert
      foreach (var response in responses)
      {
        if (response.ResponseStatus != StatusType.Valid)
          _log.Information(response.GetErrorMessage());

        response.ResponseStatus.Should().Be(StatusType.Valid);
      }

      foreach (Guid id in referralIds)
      {
        await RemoveReferralById(id);
      }

      await RemoveFakeProvider(providerId);
    }

    [Fact]
    public async Task NewProviderSubmissionUpdatesDate_WithDate_IsInValid()
    {
      //arrange
      int makeInvalidDateOffset = -100;
      List<Guid> referralIds = new List<Guid>();
      Guid providerId = Guid.NewGuid();

      _service = new ProviderService(_context,
        _serviceFixture.Mapper,
        _mockOptions.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };

      await AddFakeProvider(providerId);

      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(ValidTestModels
          .SubmissionHasDateStartedProgram);

      Guid? guidTEST_USER = Guid.Parse(TEST_USER_ID);
      foreach (ServiceUserSubmissionRequest request in requests)
      {
        referralIds.Add(AddFakeReferral(request.Ubrn, providerId,
          ReferralStatus.ProviderAwaitingStart,
          request.Date.Value.AddDays(-1)));


        Entities.Referral referral = await _context.Referrals
          .Where(r => r.Ubrn == request.Ubrn &&
                      r.ProviderId == providerId)
          .SingleOrDefaultAsync();

        if (referral != null)
          referral.DateStartedProgramme =
            DateTimeOffset.Now.AddDays(makeInvalidDateOffset);

        await _context.SaveChangesAsync();
      }

      //act
      IEnumerable<ServiceUserSubmissionResponse> responses =
        await _service.ProviderSubmissionsAsync(requests);

      //Assert
      foreach (var response in responses)
      {
        if (response.ResponseStatus != StatusType.Valid)
          _log.Information(response.GetErrorMessage());

        response.ResponseStatus.Should().Be(StatusType.Invalid);
      }

      foreach (Guid id in referralIds)
      {
        await RemoveReferralById(id);
      }

      await RemoveFakeProvider(providerId);
    }


    [Fact]
    public async Task NewProviderSubmissionUpdatesDate_WithType_IsInValid()
    {
      //arrange
      List<Guid> referralIds = new List<Guid>();
      Guid providerId = Guid.NewGuid();

      _service = new ProviderService(_context,
        _serviceFixture.Mapper,
        _mockOptions.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };

      await AddFakeProvider(providerId);

      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(ValidTestModels
          .SubmissionHasDateStartedProgram);

      Guid? guidTEST_USER = Guid.Parse(TEST_USER_ID);
      foreach (ServiceUserSubmissionRequest request in requests)
      {
        referralIds.Add(AddFakeReferral(request.Ubrn, providerId,
          ReferralStatus.ProviderAwaitingStart,
          request.Date.Value.AddDays(-1)));


        Entities.Referral referral = await _context.Referrals
          .Where(r => r.Ubrn == request.Ubrn &&
                      r.ProviderId == providerId)
          .SingleOrDefaultAsync();

        if (referral != null)
        {
          referral.DateStartedProgramme = DateTimeOffset.Now;
        }

        request.Type = UpdateType.Update.ToString();
      }

      //act
      IEnumerable<ServiceUserSubmissionResponse> responses =
        await _service.ProviderSubmissionsAsync(requests);

      //Assert
      foreach (var response in responses)
      {
        if (response.ResponseStatus != StatusType.Valid)
          _log.Information(response.GetErrorMessage());

        response.ResponseStatus.Should().Be(StatusType.Invalid);
      }

      foreach (Guid id in referralIds)
      {
        await RemoveReferralById(id);
      }

      await RemoveFakeProvider(providerId);
    }


    [Fact]
    public async Task NewProviderSubmissionUpdatesDate_StatusChangeException()
    {
      //arrange
      List<Guid> referralIds = new List<Guid>();
      Guid providerId = Guid.NewGuid();

      _service = new ProviderService(_context,
        _serviceFixture.Mapper,
        _mockOptions.Object)
      {
        User = GetClaimsPrincipalWithId(providerId.ToString())
      };

      await AddFakeProvider(providerId);

      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(ValidTestModels
          .SubmissionHasDateStartedProgram);

      Guid? guidTEST_USER = Guid.Parse(TEST_USER_ID);
      foreach (ServiceUserSubmissionRequest request in requests)
      {
        referralIds.Add(AddFakeReferral(request.Ubrn, providerId,
          ReferralStatus.FailedToContact,
          request.Date.Value.AddDays(-1)));


        Entities.Referral referral = await _context.Referrals
          .Where(r => r.Ubrn == request.Ubrn &&
                      r.ProviderId == providerId)
          .SingleOrDefaultAsync();

        if (referral != null)
        {
          referral.DateStartedProgramme = DateTimeOffset.Now;
        }
      }

      //act
      IEnumerable<ServiceUserSubmissionResponse> responses =
        await _service.ProviderSubmissionsAsync(requests);

      //Assert
      foreach (var response in responses)
      {
        if (response.ResponseStatus != StatusType.Valid)
          _log.Information(response.GetErrorMessage());

        response.ResponseStatus.Should().Be(StatusType.Invalid);
      }

      foreach (Guid id in referralIds)
      {
        await RemoveReferralById(id);
      }

      await RemoveFakeProvider(providerId);
    }


    [Theory]
    [InlineData(InvalidTestModels.SubmissionUpdatesDateBeforeReferralDate
      , StatusType.Invalid)]
    [InlineData(ValidTestModels.SubmissionUpdatesAfterBeforeReferralDate,
      StatusType.Invalid)]
    public async Task NullProviderSubmissionUpdatesDateValidation(string json,
      Enums.StatusType status)
    {
      //arrange
      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      foreach (ServiceUserSubmissionRequest request in requests)
      {
        if (!string.IsNullOrWhiteSpace(request.Type) &&
            request.UpdateType == UpdateType.Started)
        {
          Referral entity =
            await _context.Referrals.FirstOrDefaultAsync(t =>
              t.Ubrn.Equals(request.Ubrn));

          if (entity == null) continue;

          //set start date to request date as validating updates
          entity.DateOfProviderSelection = null;
          await _context.SaveChangesAsync();
        }
      }

      //act
      IEnumerable<ServiceUserSubmissionResponse> responses =
        await _service.ProviderSubmissionsAsync(requests);

      //Assert
      foreach (var response in responses)
      {
        if (response.ResponseStatus != StatusType.Valid)
          _log.Information(response.GetErrorMessage());

        Assert.Equal(status, response.ResponseStatus);
      }
    }

    [Theory]
    [InlineData(ValidTestModels.SubmissionStarted1, true, "")]
    public async Task ProviderUpdatedWithInvalidStartDate(
      string json, bool isValid, string expectedError)
    {
      //arrange
      int numberOfDays = 80;
      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      foreach (ServiceUserSubmissionRequest request in requests)
      {
        Referral entity =
          await _context.Referrals.FirstOrDefaultAsync(t =>
            t.Ubrn.Equals(request.Ubrn));

        request.Updates.ToList().ForEach(t => t.Date = DateTime.Now);
        request.Date = DateTimeOffset.Now;

        if (entity == null)
          continue;
        entity.Status = ReferralStatus.ProviderAwaitingStart.ToString();
        entity.DateOfProviderSelection =
          DateTimeOffset.Now.AddDays(-(numberOfDays + 10));
        entity.DateOfReferral =
          DateTimeOffset.Now.AddDays(-(numberOfDays + 11));
        entity.DateStartedProgramme =
          DateTimeOffset.Now.AddDays(-numberOfDays);
        await _context.SaveChangesAsync();

        expectedError = string.Format(
          expectedError,
          entity.Ubrn,
          _providerName,
          entity.DateStartedProgramme.Value.Date
            .AddDays(_options.CompletionDays));

        IEnumerable<ServiceUserSubmissionResponse> responses =
          await _service.ProviderSubmissionsAsync(requests);

        //Assert
        foreach (ServiceUserSubmissionResponse response in responses)
        {
          if (isValid)
          {
            response.Errors.Count.Should().Be(0);
            response.ResponseStatus.Should().Be(StatusType.Valid);
          }
          else
          {
            response.Errors.Count.Should().BeGreaterThan(0);
            response.ResponseStatus.Should().Be(StatusType.Invalid);
            response.Errors.First().Should().Be(expectedError);
          }
        }
      }
    }

    [Theory]
    [InlineData(ValidTestModels.SubmissionComplete1)]
    [InlineData(ValidTestModels.SubmissionCompletedWithUpdates)]
    public async Task ProviderCompletedWithValidStartDate(string json)
    {
      //arrange
      int numberOfDays = 86;
      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      foreach (ServiceUserSubmissionRequest request in requests)
      {
        Referral entity =
          await _context.Referrals.FirstOrDefaultAsync(t =>
            t.Ubrn.Equals(request.Ubrn));

        request.Date = DateTimeOffset.Now;
        request.Updates.ToList().ForEach(t => t.Date = DateTime.UtcNow);

        if (entity == null) continue;
        entity.DateOfProviderSelection =
          DateTimeOffset.Now.AddDays(-(numberOfDays + 10));
        entity.DateOfReferral =
          DateTimeOffset.Now.AddDays(-(numberOfDays + 11));
        entity.DateStartedProgramme =
          DateTimeOffset.Now.AddDays(-numberOfDays);
        entity.Status = ReferralStatus.ProviderStarted.ToString();
        await _context.SaveChangesAsync();

        IEnumerable<ServiceUserSubmissionResponse> responses =
          await _service.ProviderSubmissionsAsync(requests);

        //Assert
        foreach (ServiceUserSubmissionResponse response in responses)
        {
          response.Errors.Count.Should().Be(0);
          response.ResponseStatus.Should().Be(StatusType.Valid);
        }
      }
    }

    [Fact]
    public async Task UpdateDatedBeforeDateOfStartedProgrammeReturnsInvalidResponse()
    {
      // Arrange.
      string ubrn = "GP0123456890";
      DateTime dateOfProviderSelection = DateTime.UtcNow.AddDays(-7);
      DateTime dateStartedProgramme = dateOfProviderSelection.AddDays(2);
      DateTime updateDate = dateOfProviderSelection.AddDays(1);

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateOfProviderSelection: dateOfProviderSelection,
        dateStartedProgramme: dateStartedProgramme,
        providerId: Guid.Parse(_sid),
        providerUbrn: ubrn,
        ubrn: ubrn);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      ServiceUserUpdatesRequest update = new()
      {
        Weight = WmsHub.Common.Helpers.Constants.MAX_WEIGHT_KG - 1,
        Date = updateDate
      };

      ServiceUserSubmissionRequestV2 request = new()
      {
        Date = DateTimeOffset.UtcNow,
        Type = "Update",
        Ubrn = ubrn,
        Updates = [update]
      };

      // Act.
      IEnumerable<ServiceUserSubmissionResponse> response =
        await _service.ProviderSubmissionsAsync([request]);

      // Assert.
      response.Should().HaveCount(1);
      ServiceUserSubmissionResponse serviceUserSubmissionResponse = response.Single();
      serviceUserSubmissionResponse.ResponseStatus.Should().Be(StatusType.Invalid);
      serviceUserSubmissionResponse.GetErrorMessage().Should().Match(
        $"*{request.Ubrn}*{referral.DateStartedProgramme}*");
      _context.Referrals.Single(r => r.Ubrn == ubrn).ProviderSubmissions.Should().BeEmpty();
    }
  }

  public class UpdateProviderAuthAsyncTests : ProviderServiceTests
  {
    public UpdateProviderAuthAsyncTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task Valid_ProviderAuth_IsActive()
    {
      //Arrange
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithNoAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        IpWhitelist = "127.0.0.1,::1",
        ProviderId = id
      };
      //Act
      bool result = await _service.UpdateProviderAuthAsync(request);

      //Assert
      result.Should().BeTrue();
      entity.ProviderAuth.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Valid_ProviderAuth_NoIpWhiteList_IsActive()
    {
      //Arrange
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithNoAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        StartRange = "192.168.0.1",
        EndRange = "192.168.0.5",
        IpWhitelist = "",
        ProviderId = id
      };
      //Act
      bool result = await _service.UpdateProviderAuthAsync(request);

      //Assert
      result.Should().BeTrue();
      entity.ProviderAuth.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Valid_Existing_ProviderAuth_IsActive()
    {
      //Arrange
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        IpWhitelist = "127.0.0.1,::1",
        ProviderId = id
      };
      //Act
      bool result = await _service.UpdateProviderAuthAsync(request);

      //Assert
      result.Should().BeTrue();
      entity.ProviderAuth.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task IncorrectMobile_Throws_ArgumentException()
    {
      //Arrange
      string expected =
        "Validation had the following error(s):The MobileNumber field " +
        "'Mobile Number must start with '+' followed by country code' " +
        "is invalid.";
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "07776204159",
        IpWhitelist = "127.0.0.1,::1",
        ProviderId = id
      };
      //Act
      try
      {
        bool result = await _service.UpdateProviderAuthAsync(request);

        //Assert
        Assert.Fail("ArgumentException expected");
      }
      catch (ArgumentException ex)
      {
        ex.Message.Should().Be(expected);
      }
      catch (Exception ex)
      {
        Assert.Fail(ex.Message);
      }
    }


    [Fact]
    public async Task ProviderAuthUpdateRequest_ArgumentNullException()
    {
      //Arrange 
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        IpWhitelist = "127.0.0.1,::1",
        ProviderId = Guid.NewGuid()
      };

      // Act & Assert
      ArgumentNullException exception =
        await Assert.ThrowsAsync<ArgumentNullException>(
        async () => await _service.UpdateProviderAuthAsync(request));
    }

    [Fact]
    public async Task ProviderAuthUpdateRequest_KeyViaSmsFalse()
    {
      //Arrange 
      Entities.Provider entity = RandomEntityCreator.CreateRandomProvider();
      Entities.ProviderAuth auth =
        RandomEntityCreator.CreateRandomProviderAuth();
      entity.ProviderAuth = auth;
      _context.ProviderAuth.Add(auth);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();

      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = false,
        KeyViaEmail = true,
        MobileNumber = "+447776204159",
        IpWhitelist = "127.0.0.1,::1",
        ProviderId = entity.Id,
        EmailContact = "test@test.com"
      };

      // Act
      bool result = await _service.UpdateProviderAuthAsync(request);

      // Assert
      result.Should().BeTrue();
    }

    [Fact]
    public async Task ProviderAuthUpdateRequest_KeyViaEmailFalse()
    {
      //Arrange 
      Entities.Provider entity = RandomEntityCreator.CreateRandomProvider();
      Entities.ProviderAuth auth =
        RandomEntityCreator.CreateRandomProviderAuth();
      entity.ProviderAuth = auth;
      _context.ProviderAuth.Add(auth);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();

      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        KeyViaEmail = false,
        MobileNumber = "+447776204159",
        IpWhitelist = "127.0.0.1,::1",
        ProviderId = entity.Id,
        EmailContact = "test@test.com"
      };

      // Act
      bool result = await _service.UpdateProviderAuthAsync(request);

      // Assert
      result.Should().BeTrue();
    }


    [Fact]
    public void Validate_ProviderAuthUpdateRequest_ip_IsValid()
    {
      //Arrange
      Guid id = Guid.NewGuid();
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        IpWhitelist = "127.0.0.1,::1",
        ProviderId = id
      };
      //Act
      ValidationContext context = new ValidationContext(instance: request);

      ValidateModelResult result = new ValidateModelResult();
      result.IsValid = Validator.TryValidateObject(
        request, context, result.Results, validateAllProperties: true);
      //Assert
      result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ProviderAuthUpdateRequest_range_IsValid()
    {
      //Arrange
      Guid id = Guid.NewGuid();
      string expected = "192.168.0.1-5";
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        StartRange = "192.168.0.1",
        EndRange = "192.168.0.5",
        ProviderId = id,
        IpWhitelist = "::1"
      };
      //Act
      ValidationContext context = new ValidationContext(instance: request);

      ValidateModelResult result = new ValidateModelResult();
      result.IsValid = Validator.TryValidateObject(
        request, context, result.Results, validateAllProperties: true);
      //Assert
      result.IsValid.Should().BeTrue();
      request.IpRange.Should().Be(expected);
    }

    [Fact]
    public void Validate_ProviderAuthUpdateRequest_RangeWrongDomain()
    {
      //Arrange
      Guid id = Guid.NewGuid();
      string expected =
        "The EndRange field 'EndRange 192.168.10.5 IPv4 Address must be" +
        " in same domain as the StartRange 192.168.0.1' is invalid.";
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        StartRange = "192.168.0.1",
        EndRange = "192.168.10.5",
        ProviderId = id,
        IpWhitelist = "::1"
      };
      //Act
      ValidationContext context = new ValidationContext(instance: request);

      ValidateModelResult result = new ValidateModelResult();
      result.IsValid = Validator.TryValidateObject(
        request, context, result.Results, validateAllProperties: true);
      //Assert
      result.IsValid.Should().BeFalse();
      result.Results.ForEach(t => t.ErrorMessage.Should().Be(expected));
    }

    [Fact]
    public async Task Valid_IP_RangeAdded()
    {
      //Arrange
      string expected = "::1,192.168.0.1-5";
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        StartRange = "192.168.0.1",
        EndRange = "192.168.0.5",
        ProviderId = id,
        IpWhitelist = "::1"
      };
      //Act
      bool result = await _service.UpdateProviderAuthAsync(request);

      //Assert
      result.Should().BeTrue();
      entity.ProviderAuth.IpWhitelist.Should().Be(expected);
    }

    [Theory]
    [InlineData("localhost", false)]
    [InlineData("129.168.0.o", false)]
    [InlineData("192.168.0.1,::1", true)]
    [InlineData("127.0.0.1,::1,35.23.75.95-101", true)]
    public async Task IncorrectIpWhitelist_Throws_ArgumentException(
      string ipAddress, bool isValid)
    {
      //Arrange
      string expected =
        "Validation had the following error(s):The IpWhitelist field " +
        $"'IpWhitelist value of {ipAddress} is not a valid IPv4 address'" +
        " is invalid.";
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        IpWhitelist = ipAddress,
        ProviderId = id
      };
      //Act
      try
      {
        bool result = await _service.UpdateProviderAuthAsync(request);

        //Assert
        Assert.True(isValid,
          isValid ? "passed" : "ArgumentException expected");
      }
      catch (ArgumentException ex)
      {
        ex.Message.Should().Be(expected);
      }
      catch (Exception ex)
      {
        Assert.Fail(ex.Message);
      }
    }


    [Fact]
    public async Task Valid_IP_RangeAddedToExisting()
    {
      //Arrange
      string expected = "127.0.0.1,::1,192.168.0.1-5";
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        IpWhitelist = "127.0.0.1,::1",
        StartRange = "192.168.0.1",
        EndRange = "192.168.0.5",
        ProviderId = id
      };
      //Act
      bool result = await _service.UpdateProviderAuthAsync(request);

      //Assert
      result.Should().BeTrue();
      entity.ProviderAuth.IpWhitelist.Should().Be(expected);
    }

    [Fact]
    public async Task Valid_IGNORE_FOR_TESTING_Allowed()
    {
      //Arrange
      var ignore = "**IGNORE_FOR_TESTING**";
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest
      {
        KeyViaSms = true,
        MobileNumber = "+447776204159",
        IpWhitelist = ignore,
        ProviderId = id
      };
      //act
      bool result = await _service.UpdateProviderAuthAsync(request);
      //Assert
      //Assert
      result.Should().BeTrue();
      entity.ProviderAuth.IpWhitelist.Should().Be(ignore);

      //Cleanup
      await CleanupProviders(entity);
    }

  }

  private async Task CleanupProviders(Entities.Provider entity)
  {
    _context.Providers.Remove(entity);
    await _context.SaveChangesAsync();
  }

  public class UpdateProviderAsyncTests : ProviderServiceTests
  {
    public UpdateProviderAsyncTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithNoAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderRequest request = new ProviderRequest()
      {
        Id = entity.Id,
        Name = entity.Name,
        Level1 = true,
        Level2 = true,
        Level3 = true,
        Summary = "Test Summary 1",
        Summary2 = "Test Summary 2",
        Summary3 = "Test Summary 3"
      };
      //Act
      ProviderResponse result = await _service.UpdateProvidersAsync(request);

      //Assert
      result.Should().NotBeNull();
      _context.Providers.Remove(entity);
      await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Summary_Empty_Valid()
    {
      // Arrange
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithNoAuth(id);
      entity.Summary = "TEST";
      entity.Summary2 = "TEST";
      entity.Summary3 = "TEST";

      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderRequest request = new ProviderRequest()
      {
        Id = entity.Id,
        Name = entity.Name,
        Level1 = true,
        Level2 = true,
        Level3 = true,
        Summary = string.Empty,
        Summary2 = string.Empty,
        Summary3 = string.Empty
      };
      //Act
      ProviderResponse result = await _service.UpdateProvidersAsync(request);

      //Assert
      result.Should().NotBeNull();
      entity.Summary.Should().BeNullOrWhiteSpace();
      entity.Summary2.Should().BeNullOrWhiteSpace();
      entity.Summary3.Should().BeNullOrWhiteSpace();
      //Cleanup
      _context.Providers.Remove(entity);
      await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Valid_ProviderRequestDifferences()
    {
      // Arrange
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithNoAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      ProviderRequest request = new ProviderRequest()
      {
        Id = entity.Id,
        Name = "DifferentName",
        Logo = "DifferentLogo",
        Website = "DifferentWebsite",
        Level1 = !entity.Level1,
        Level2 = !entity.Level2,
        Level3 = !entity.Level3,
        Summary = "Test Summary 1",
        Summary2 = "Test Summary 2",
        Summary3 = "Test Summary 3"
      };
      //Act
      ProviderResponse result = await _service.UpdateProvidersAsync(request);

      //Assert
      result.Should().NotBeNull();
      _context.Providers.Remove(entity);
      await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Invalid_ProviderNotFound_Exception()
    {
      //Arrange
      Guid id = Guid.NewGuid();
      ProviderRequest request = new ProviderRequest()
      {
        Id = id,
        Name = "test",
        Level1 = true,
        Level2 = true,
        Level3 = true,
        Summary = "Test Summary 1",
        Summary2 = "Test Summary 2",
        Summary3 = "Test Summary 3"
      };
      try
      {
        //Act
        ProviderResponse result =
          await _service.UpdateProvidersAsync(request);
        //Assert
        Assert.Fail("ProviderNotFoundException expected");
      }
      catch (ProviderNotFoundException ex)
      {
        Assert.True(true, ex.Message);
      }
      catch (Exception ex)
      {
        Assert.Fail($"ProviderNotFoundException expected but got: {ex.Message}");
      }
    }
  }

  public class GetAllActiveProvidersAsyncTest : ProviderServiceTests
  {
    public GetAllActiveProvidersAsyncTest(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task Valid()
    {
      // Arrange
      Guid id = Guid.NewGuid();
      Entities.Provider entity = ServiceFixture.CreateProviderWithNoAuth(id);
      _context.Providers.Add(entity);
      await _context.SaveChangesAsync();
      //Act
      var result = await _service.GetAllActiveProvidersAsync();

      //Assert
      result.Should().NotBeNull();

      _context.Providers.Remove(entity);
      await _context.SaveChangesAsync();
    }
  }

  public class ValidateProviderKeyAsyncTests : ProviderServiceTests
  {
    public ValidateProviderKeyAsyncTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.Providers.RemoveRange(_context.Providers);
      _context.SaveChanges();
    }

    [Fact]
    public async Task ValidateProviderKeyAsync_Throw_ArgumentNullException()
    {
      //Arrange
      string key = "";

      // Act & Assert
      ArgumentNullException exception =
        await Assert.ThrowsAsync<ArgumentNullException>(
        async () => await _service.ValidateProviderKeyAsync(key));
    }

    [Fact]
    public async Task ValidateProviderKeyAsync_Guid_Returned()
    {
      //Arrange
      Guid apiKey = Guid.NewGuid();
      Entities.Provider provider = RandomEntityCreator.CreateRandomProvider(
        apiKey: apiKey.ToString(),
        apiKeyExpires: DateTimeOffset.Now.AddDays(4)
        );
      _context.Providers.Add(provider);
      await _context.SaveChangesAsync();
      string expectedProviderId = provider.Id.ToString();

      // Act
      Guid returnedGuid =
        await _service.ValidateProviderKeyAsync(apiKey.ToString());

      // Assert
      returnedGuid.Should().Be(expectedProviderId);

      //clean up
      _context.Remove(provider);
      await _context.SaveChangesAsync();
    }
  }

  public class UpdateProviderLevelsAsyncTests : ProviderServiceTests
  {
    public UpdateProviderLevelsAsyncTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context.Providers.RemoveRange(_context.Providers);
      _context.Providers.Add(ServiceFixture.CreateProviderWithReferrals());
      _context.SaveChanges();
    }

    [Fact]
    public async Task UpdateProviderLevelsAsync_ProviderLevels_Updated()
    {
      //Arrange
      ProviderLevelStatusChangeRequest request =
        new ProviderLevelStatusChangeRequest();
      Entities.Provider provider = _context.Providers
        .Where(p => p.Id.ToString() ==  ServiceFixture.PROVIDER_API_USER_ID)
        .SingleOrDefault();
      provider.Level1 = false;
      provider.Level2 = false;
      provider.Level3 = false;
      await _context.SaveChangesAsync();

      request.Id = provider.Id;
      request.Level1 = true;
      request.Level2 = true;
      request.Level3 = true;

      // Act 
      ProviderResponse response =
        await _service.UpdateProviderLevelsAsync(request);

      // Assert
      response.Level1.Should().BeTrue();
      response.Level2.Should().BeTrue();
      response.Level3.Should().BeTrue();
      response.ResponseStatus.Should().Be(StatusType.Valid);

      //clean up
      _context.Remove(provider);
      await _context.SaveChangesAsync();
    }
  }

  public class GetProviderAsyncTests : ProviderServiceTests
  {
    public GetProviderAsyncTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task GetProviderAsyncTests_ProviderNotFoundException()
    {
      //Arrange
      Guid guid = Guid.NewGuid();

      // Act & Assert
      ProviderNotFoundException exception =
        await Assert.ThrowsAsync<ProviderNotFoundException>(
        async () => await _service.GetProviderAsync(guid));
    }

    [Fact]
    public async Task GetProviderAsyncTests_ReturnResponse()
    {
      //Arrange
      _context.Providers.RemoveRange(_context.Providers);
      _context.Providers.Add(ServiceFixture.CreateProviderWithReferrals());
      _context.SaveChanges();
      Entities.Provider provider = _context.Providers.SingleOrDefault();

      // Act
      ProviderResponse response =
       await _service.GetProviderAsync(provider.Id);

      // Assert
      response.ResponseStatus.Should().Be(StatusType.Valid);
      response.Id.Should().Be(provider.Id);
      response.Logo.Should().Be(provider.Logo);
      response.Name.Should().Be(provider.Name);
      response.Level1.Should().Be(provider.Level1);
      response.Level2.Should().Be(provider.Level2);
      response.Level3.Should().Be(provider.Level3);

      //clean up
      _context.Remove(provider);
      await _context.SaveChangesAsync();
    }
  }

  public class DeleteTestReferralsAsyncTests : ProviderServiceTests
  {
    public DeleteTestReferralsAsyncTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task DeleteTestReferralsAsync_TrueResponse()
    {
      // Act
      bool response =
       await _service.DeleteTestReferralsAsync();

      // Assert
      response.Should().Be(true);
    }
    [Fact]
    public async Task DeleteTestReferralsAsync_FalseResponse()
    {
      // Arrange
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();

      // Act
      bool response =
       await _service.DeleteTestReferralsAsync();

      // Assert
      response.Should().BeFalse();
    }
  }

  public class UpdateProviderKeyAsyncTests : ProviderServiceTests
  {
    public UpdateProviderKeyAsyncTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task UpdateProviderKeyAsync_TrueResponse()
    {
      // Act
      int validDays = 365;
      ProviderResponse req = new ProviderResponse();
      req.Id = Guid.Parse(ServiceTestsBase.TEST_USER_ID);

      NewProviderApiKeyResponse response =
       await _service.UpdateProviderKeyAsync(req);

      // Assert
      response.Should().NotBeNull();
      response.ResponseStatus.Should().Be(StatusType.Valid);
      response.Id.Should().Be(req.Id);
      response.ApiKeyExpiry = DateTimeOffset.Now.AddDays(validDays);
    }

    [Fact]
    public async Task UpdateProviderKeyAsync_TrueResponseDifferentExpiry()
    {
      // Act
      int validDays = 100;
      ProviderResponse req = new ProviderResponse();
      req.Id = Guid.Parse(ServiceTestsBase.TEST_USER_ID);

      NewProviderApiKeyResponse response =
       await _service.UpdateProviderKeyAsync(req, validDays);

      // Assert
      response.Should().NotBeNull();
      response.ResponseStatus.Should().Be(StatusType.Valid);
      response.Id.Should().Be(req.Id);
      response.ApiKeyExpiry = DateTimeOffset.Now.AddDays(validDays);
    }
  }

  public class CreateTestReferralsAsyncTests : ProviderServiceTests
  {
    public CreateTestReferralsAsyncTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task CreateTestReferralsAsync_ProviderNotFoundException()
    {
      // Arrange
      _context.Providers.RemoveRange(_context.Providers);
      List<Entities.Referral> refs = await _context.Referrals
        .Where(r => r.ReferringGpPracticeName ==
        ServiceTestsBase.TEST_USER_ID)
        .ToListAsync();
      _context.Referrals.RemoveRange(refs);
      _context.SaveChanges();

      // Act & Assert
      ProviderNotFoundException exception =
        await Assert.ThrowsAsync<ProviderNotFoundException>(
        async () => await _service.CreateTestReferralsAsync(1));
    }

    [Fact]
    public async Task CreateTestReferralsAsync_FalseResponse()
    {
      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral();
      referral.ReferringGpPracticeName = TEST_USER_ID;
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act
      Business.Models.Referral[] response = await _service.CreateTestReferralsAsync(1);

      // Assert
      response.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateTestReferralsAsync_TrueResponse()
    {
      // Arrange
      List<Entities.Referral> refs = await _context.Referrals
       .Where(r => r.ReferringGpPracticeName == ServiceTestsBase.TEST_USER_ID)
       .ToListAsync();
      _context.Referrals.RemoveRange(refs);

      Entities.Provider provider = _context.Providers
        .Where(p => p.Id == Guid.Parse(ServiceTestsBase.TEST_USER_ID))
        .SingleOrDefault();

      provider.Level1 = true;
      provider.Level2 = true;
      provider.Level3 = true;
      await _context.SaveChangesAsync();

      // Act
      Business.Models.Referral[] response = await _service.CreateTestReferralsAsync(1);

      // Assert
      response.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("B1530C62-9AB2-4043-9C79-F82A2A32897B")]
    [InlineData("E778091E-E5B7-47B0-90D2-E28D6236A5F5")]
    [InlineData("EBE236FA-D293-41EC-9B34-9F1BFAE992CE")]
    [InlineData("1EA6450D-7506-4A9C-944C-91F328CB2083")]
    [InlineData("AF432ECF-6BF2-461D-AD5B-80701103699B")]
    [InlineData("9AE1768C-BF1A-4FB0-965F-2B22D27C7BE0")]
    [InlineData("21E521D3-3EC4-4C4D-962A-61D003EB21B7")]
    [InlineData("05DA4135-9AD3-48B6-900C-FE31F4697835")]
    [InlineData("CE235712-395B-449D-9118-AF4736EE1844")]
    [InlineData("A7AFB83E-99C4-46A0-86B5-9CD688DD82CF")]
    [InlineData("1BE04438-6D16-4924-BAA0-8F2A9DA415E6")]
    [InlineData("83D4F0C0-E010-4BEE-BE81-7E86AA9F48F6")]
    [InlineData("2D11868B-6200-4C14-9F49-2A17D735A573")]
    public async Task
      CreateTestReferralsAsync_MultiProviders_TrueResponse(string providerId)
    {
      // Arrange
      _service = new ProviderService(_context,
        _serviceFixture.Mapper,
        _mockOptions.Object)
      {
        User = GetClaimsPrincipalWithId(providerId)
      };

      List<Entities.Referral> refs = await _context.Referrals
       .Where(r => r.ReferringGpPracticeName == providerId)
       .ToListAsync();
      _context.Referrals.RemoveRange(refs);

      Entities.Provider provider = _context.Providers
        .Where(p => p.Id == Guid.Parse(providerId))
        .SingleOrDefault();
      if (provider == null)
      {
        provider = RandomEntityCreator.CreateRandomProvider(
          id: Guid.Parse(providerId));
        _context.Providers.Add(provider);
      }

      provider.Level1 = true;
      provider.Level2 = true;
      provider.Level3 = true;
      await _context.SaveChangesAsync();

      // Act
      Business.Models.Referral[] response = await _service.CreateTestReferralsAsync(1);

      // Assert
      response.Should().NotBeEmpty();
    }
  }

  //public class GetRejectionReasonsAsyncTests 
  //  : ProviderServiceTests, IDisposable
  //{
  //  public GetRejectionReasonsAsyncTests(ServiceFixture serviceFixture,
  //    ITestOutputHelper testOutputHelper)
  //    : base(serviceFixture, testOutputHelper)
  //  {
  //    CleanUp();
  //  }

  //  public void Dispose()
  //  {
  //    GC.SuppressFinalize(this);
  //    CleanUp();
  //  }

  //  private void CleanUp()
  //  {
  //    _context.ProviderRejectionReasons
  //      .RemoveRange(_context.ProviderRejectionReasons);

  //    _context.SaveChanges();
  //  }

  //  [Theory]
  //  [InlineData(ReferralStatusReasonGroup.ProviderRejected)]
  //  [InlineData(ReferralStatusReasonGroup.ServiceUserDeclined)]
  //  [InlineData(ReferralStatusReasonGroup.ProviderTerminated)]
  //  public async Task ValidListReturned(ReferralStatusReasonGroup group)
  //  {
  //    // Arrange
  //    Entities.ReferralStatusReason providerRejectionReason1 = new()
  //    {
  //      Description = "Test description 1.",
  //      Group = group,
  //      IsActive = true,
  //      IsRmcReason = false,
  //      ModifiedAt = DateTimeOffset.Now,
  //      ModifiedByUserId = Guid.Empty
  //    };
  //    Entities.ReferralStatusReason providerRejectionReason2 = new()
  //    {
  //      Description = "Test description 2.",
  //      Group = group,
  //      IsActive = true,
  //      IsRmcReason = false,
  //      ModifiedAt = DateTimeOffset.Now,
  //      ModifiedByUserId = Guid.Empty
  //    };
  //    _context.ProviderRejectionReasons.Add(providerRejectionReason1);
  //    _context.ProviderRejectionReasons.Add(providerRejectionReason2);
  //    await _context.SaveChangesAsync();

  //    // Act
  //    ReferralStatusReason[] response = await _service
  //      .GetRejectionReasonsAsync();

  //    // Assert
  //    response.Length.Should().Be(2);
  //  }

  //  [Fact]
  //  public async Task ValidActiveOnlyListReturned()
  //  {
  //    // Arrange
  //    Entities.ReferralStatusReason reason1 = new()
  //    {
  //      ModifiedAt = DateTimeOffset.Now,
  //      IsActive = false,
  //      Description = "This is a test description 1",
  //      ModifiedByUserId = Guid.Empty
  //    };
  //    Entities.ReferralStatusReason reason2 = new()
  //    {
  //      ModifiedAt = DateTimeOffset.Now,
  //      IsActive = true,
  //      Description = "This is a test description 2",
  //      ModifiedByUserId = Guid.Empty
  //    };
  //    _context.ProviderRejectionReasons.Add(reason1);
  //    _context.ProviderRejectionReasons.Add(reason2);
  //    await _context.SaveChangesAsync();

  //    // Act
  //    ReferralStatusReason response =
  //      (await _service.GetRejectionReasonsAsync())
  //      .FirstOrDefault();

  //    //Assert
  //    response.Length.Should().Be(1);
  //    response.FirstOrDefault().Title.Should().Be("TestDescription2");
  //  }

  //  [Fact]
  //  public async Task InValidNoActiveListReturned()
  //  {
  //    //Arrange
  //    var reason = new Business.Entities.ReferralStatusReason
  //    {
  //      ModifiedAt = DateTimeOffset.Now,
  //      IsActive = false,
  //      Description = "This is a test description",
  //      Title = "TestDescription",
  //      ModifiedByUserId = Guid.Empty
  //    };
  //    _context.ProviderRejectionReasons.Add(reason);
  //    await _context.SaveChangesAsync();
  //    //Act
  //    try
  //    {
  //      var response = await _service.GetRejectionReasonsAsync();
  //      //Assert
  //      Assert.True(false, "ArgumentNullException Expected");
  //    }
  //    catch (ArgumentNullException)
  //    {
  //      Assert.True(true);
  //    }
  //    catch (Exception ex)
  //    {
  //      Assert.True(false, ex.Message);
  //    }
  //    finally
  //    {
  //      //Cleanup
  //      _context.ProviderRejectionReasons.Remove(reason);
  //      await _context.SaveChangesAsync();
  //    }


  //  }

  //}

  //public class SetRejectionReasonsAsyncTests : ProviderServiceTests
  //{
  //  private Guid _id;
  //  public SetRejectionReasonsAsyncTests(ServiceFixture serviceFixture,
  //    ITestOutputHelper testOutputHelper)
  //    : base(serviceFixture, testOutputHelper)
  //  {
  //    //Cleanup
  //    var entities = _context.ProviderRejectionReasons.ToList();

  //    _context.ProviderRejectionReasons.RemoveRange(entities);
  //    _context.SaveChanges();

  //    var entity = new Business.Entities.ReferralStatusReason
  //    {
  //      ModifiedAt = DateTimeOffset.Now,
  //      IsActive = true,
  //      Description = "This is a test description",
  //      Title = "TestDescription",
  //      ModifiedByUserId = Guid.Empty
  //    };
  //    _context.ProviderRejectionReasons.Add(entity);
  //    _context.SaveChanges();

  //    _id = entity.Id;
  //  }
  //  [Fact]
  //  public async Task ValidNewReason()
  //  {
  //    //Arrange

  //    ReferralStatusReasonRequest reason =
  //      new ReferralStatusReasonRequest
  //      {
  //        Title = "TestNewTitle",
  //        Description = "This is a new submission test"
  //      };
  //    //Act
  //    ReferralStatusReason response =
  //      await _service.SetNewRejectionReasonAsync(reason);
  //    //Assert
  //    response.Id.Should().NotBe(Guid.Empty);
  //    response.IsActive.Should().BeTrue();
  //  }

  //  [Fact]
  //  public async Task ValidReasonUpdateDescriptionById()
  //  {
  //    //Arrange
  //    string expected = "Update of Test Description";
  //    ProviderRejectionReasonUpdate reason = new ProviderRejectionReasonUpdate
  //    {
  //      Id = _id,
  //      Description = expected
  //    };
  //    //Act
  //    ReferralStatusReason response =
  //      await _service.UpdateRejectionReasonsAsync(reason);
  //    //Assert
  //    response.Id.Should().Be(_id);
  //    response.IsActive.Should().BeTrue();
  //    response.Description.Should().Be(expected);
  //  }

  //  [Fact]
  //  public async Task ValidReasonUpdateIsActiveById()
  //  {
  //    //Arrange
  //    ProviderRejectionReasonUpdate reason = new ProviderRejectionReasonUpdate
  //    {
  //      Id = _id,
  //      IsActive = false
  //    };
  //    //Act
  //    ReferralStatusReason response =
  //      await _service.UpdateRejectionReasonsAsync(reason);
  //    //Assert
  //    response.Id.Should().Be(_id);
  //    response.IsActive.Should().BeFalse();
  //  }

  //  [Fact]
  //  public async Task ValidReasonUpdateDescriptionByTitle()
  //  {
  //    //Arrange
  //    string expected = "Update of Test Description";
  //    ProviderRejectionReasonUpdate reason = new ProviderRejectionReasonUpdate
  //    {
  //      Title = "TestDescription",
  //      Description = expected
  //    };
  //    //Act
  //    ReferralStatusReason response =
  //      await _service.UpdateRejectionReasonsAsync(reason);
  //    //Assert
  //    response.Id.Should().Be(_id);
  //    response.IsActive.Should().BeTrue();
  //    response.Description.Should().Be(expected);
  //  }
  //  [Fact]
  //  public async Task ValidReasonUpdateIsActiveByTitle()
  //  {
  //    //Arrange
  //    ProviderRejectionReasonUpdate reason = new ProviderRejectionReasonUpdate
  //    {
  //      Title = "TestDescription",
  //      IsActive = false
  //    };
  //    //Act
  //    ReferralStatusReason response =
  //      await _service.UpdateRejectionReasonsAsync(reason);
  //    //Assert
  //    response.Id.Should().Be(_id);
  //    response.IsActive.Should().BeFalse();
  //  }
  //  [Fact]
  //  public async Task InValidReasonUpdateIsActiveById_not_Found()
  //  {
  //    //Arrange
  //    ProviderRejectionReasonUpdate reason = new ProviderRejectionReasonUpdate
  //    {
  //      Id = Guid.NewGuid(),
  //      IsActive = false
  //    };
  //    //Act
  //    try
  //    {
  //      var response = await _service.UpdateRejectionReasonsAsync(reason);
  //      //Assert
  //      Assert.True(false,
  //        "ProviderRejectionReasonDoesNotExistsException expected");
  //    }
  //    catch (ProviderRejectionReasonDoesNotExistException)
  //    {
  //      Assert.True(true);
  //    }
  //    catch (Exception ex)
  //    {
  //      Assert.True(false, ex.Message);
  //    }

  //  }
  //  [Fact]
  //  public async Task InValidReasonUpdateIsActiveByTitle_not_Found()
  //  {
  //    //Arrange
  //    ProviderRejectionReasonUpdate reason = new ProviderRejectionReasonUpdate
  //    {
  //      Title = "TestUpdateDoesNotExist",
  //      IsActive = false
  //    };
  //    //Act
  //    //Act
  //    try
  //    {
  //      var response = await _service.UpdateRejectionReasonsAsync(reason);
  //      //Assert
  //      Assert.True(false,
  //        "ProviderRejectionReasonDoesNotExistsException expected");
  //    }
  //    catch (ProviderRejectionReasonDoesNotExistException)
  //    {
  //      Assert.True(true);
  //    }
  //    catch (Exception ex)
  //    {
  //      Assert.True(false, ex.Message);
  //    }

  //  }
  //}
}
