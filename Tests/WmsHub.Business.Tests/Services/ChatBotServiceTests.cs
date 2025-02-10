using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.ChatBotService;
using WmsHub.Business.Services;
using WmsHub.Common.Exceptions;
using WmsHub.Common.SignalR;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class ChatBotServiceTests : ServiceTestsBase, IDisposable
{
  private readonly DatabaseContext _context;
  private ChatBotService _service;
  private readonly Mock<IHubClients> _mockClients = new();
  private readonly Mock<IHubContext<SignalRHub>> _mockHubContext = new();

  public ChatBotServiceTests(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper) 
    : base(serviceFixture, testOutputHelper)
  {
    _context = new DatabaseContext(_serviceFixture.Options);

    Mock<IClientProxy> mockClientProxy = new();
    _mockClients.Setup(clients => clients.All)
      .Returns(mockClientProxy.Object);

    _mockHubContext.Setup(x => x.Clients).Returns(() => _mockClients.Object);
    LoadService();
  }

  public virtual void Dispose()
  {
    _context.Dispose();
    _service = null;
    GC.SuppressFinalize(this);
  }

  public void LoadService()
  {
    if (_service != null)
    {
      return;
    }

    _service = new ChatBotService(
      _context,
      TestConfiguration.CreateArcusOptions(),
      _mockHubContext.Object)
    {
      User = GetClaimsPrincipal()
    };

    ServiceFixture.LoadDefaultData(_context);
  }

  public class UpdateReferralWithCall : ChatBotServiceTests
  {
    public UpdateReferralWithCall(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ArgumentNullException()
    {
      // Arrange.
      UpdateReferralWithCallRequest model = null;

      // Act & assert.
      ArgumentNullException exception =
        await Assert.ThrowsAsync<ArgumentNullException>(
          async () => await _service.UpdateReferralWithCall(model));
    }

    [Fact]
    public async Task UpdateReferral_InValid_Test()
    {
      // Arrange.
      UpdateReferralWithCallRequest request = Create();
      request.Timestamp = default;
      string expectedError = $"The Timestamp field '01/01/0001 " +
        "00:00:00 +00:00' is invalid.";

      // Act.
      UpdateReferralWithCallResponse response =
        await _service.UpdateReferralWithCall(request);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeEquivalentTo(request);
        response.Should().BeOfType<UpdateReferralWithCallResponse>();
        response.ResponseStatus.Should().Be(StatusType.Invalid);
        response.GetErrorMessage().Should().Be(expectedError);
      }
    }

    [Fact]
    public async Task UpdateReferral_Valid_Test()
    {
      // Arrange.
      UpdateReferralWithCallRequest request = Create();

      // Act.
      UpdateReferralWithCallResponse response =
        await _service.UpdateReferralWithCall(request);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<UpdateReferralWithCallResponse>();
        response.ResponseStatus.Should().Be(StatusType.Valid);
        response.Should().BeEquivalentTo(request);

        Call call = _context.Calls.SingleOrDefault(c => c.Id == request.Id);
        call.Should().NotBeNull();
        call.Called.Should().Be(request.Timestamp);
        call.Number.Should().Be(request.Number);
        call.Outcome.Should().Be(request.Outcome);

        Referral referral = _context.Referrals.SingleOrDefault(
          r => r.Id == call.ReferralId);
        referral.Should().NotBeNull();
        referral.Status.Should().Be(ReferralStatus.ChatBotCall1.ToString());
      }
    }

    [Fact]
    public async Task UpdateReferral_IdDoesNotExist_Test()
    {
      // Arrange.
      UpdateReferralWithCallRequest request = Create(id: Guid.NewGuid());

      // Act.
      UpdateReferralWithCallResponse response =
        await _service.UpdateReferralWithCall(request);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<UpdateReferralWithCallResponse>();
        response.ResponseStatus.Should().Be(StatusType.CallIdDoesNotExist);
        response.Should().BeEquivalentTo(request);
      }
    }

    [Fact]
    public async Task UpdateReferral_TelephoneNumberMismatch_Test()
    {
      // Arrange.
      UpdateReferralWithCallRequest request =
        Create(number: ServiceFixture.NEVER_USED_TELEPHONE_NUMBER);

      // Act.
      UpdateReferralWithCallResponse response =
        await _service.UpdateReferralWithCall(request);

      // Assert
      using (new AssertionScope())
      {
        response.Should().BeOfType<UpdateReferralWithCallResponse>();
        response.ResponseStatus.Should()
          .Be(StatusType.TelephoneNumberMismatch);
        response.Should().BeEquivalentTo(request);
      }
    }

    [Fact]
    public async Task UpdateReferralWithCall_TransferToRmc()
    {
      // Arrange. 
      Referral referral = ServiceFixture.CreateReferral(
       status: ReferralStatus.ChatBotCall1,
       isTelephoneValid: false,
       mobile: ServiceFixture.GetUniqueMobileNumber());

      Call call1 = ServiceFixture.CreateCall(
        called: DateTimeOffset.Now,
        number: referral.Mobile,
        outcome: ChatBotCallOutcome.HungUp.ToString(),
        sent: DateTimeOffset.Now.AddMinutes(-5));
      Call call2 = ServiceFixture.CreateCall(number: referral.Mobile);

      referral.Calls = new List<Call>() { call1, call2 };
      _context.Referrals.Add(referral);
      _context.SaveChanges();

      UpdateReferralWithCallRequest request = Create(
        call2.Id,
        ChatBotCallOutcome.TransferringToRmc.ToString(),
        call2.Number);

      // Act.
      UpdateReferralWithCallResponse response =
        await _service.UpdateReferralWithCall(request);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().NotBeNull();
        response.ResponseStatus.Should().Be(StatusType.Valid);

        Referral updatedReferral = _context.Referrals
         .Include(r => r.Calls)
         .Single(r => r.Id == referral.Id);
        updatedReferral.Should().NotBeNull();
        updatedReferral.Status.Should()
          .Be(ReferralStatus.ChatBotTransfer.ToString());
        updatedReferral.IsMobileValid.Should().BeTrue();
        _mockClients.Verify(clients => clients.All, Times.AtLeastOnce);
      }
    }

    [InlineData(
      ChatBotCallOutcome.InvalidNumber,
      Tel.IsValid_False,
      Mob.IsValid_True,
      Mob.IsValid_False,
      Tel.IsValid_False,
      true)]
    [InlineData(
      ChatBotCallOutcome.InvalidNumber,
      Tel.IsValid_True,
      Mob.IsValid_True,
      Mob.IsValid_True,
      Tel.IsValid_False,
      false)]
    [InlineData(
      ChatBotCallOutcome.InvalidNumber,
      Tel.IsValid_True,
      Mob.IsValid_False,
      Mob.IsValid_False,
      Tel.IsValid_False,
      false)]
    [InlineData(
      ChatBotCallOutcome.InvalidNumber,
      Tel.IsValid_True,
      Mob.IsValid_True,
      Mob.IsValid_False,
      Tel.IsValid_True,
      true)]
    [Theory]
    public async Task UpdateReferralInvalidNumberIsTelephone(
      ChatBotCallOutcome outcome,
      bool isTelephoneValid,
      bool isMobileValid,
      bool expectedIsMobileValid,
      bool expectedIsTelephoneValid,
      bool useMobileToCall)
    {
      // Arrange. 
      string mobile = ServiceFixture.GetUniqueMobileNumber();
      string phone = ServiceFixture.GetUniqueTelephoneNumber();
      string numberCalling = useMobileToCall ? mobile : phone;
      Referral referral = ServiceFixture.CreateReferral(
        status: ReferralStatus.ChatBotCall1,
        isTelephoneValid: isTelephoneValid,
        telephone: phone,
        isMobileValid: isMobileValid,
        mobile: mobile);

      Call call1 = ServiceFixture.CreateCall(
        called: DateTimeOffset.Now,
        number: isTelephoneValid
          ? referral.Telephone
          : referral.Mobile,
        outcome: ChatBotCallOutcome.HungUp.ToString(),
        sent: DateTimeOffset.Now.AddMinutes(-5));
      Call call2 = ServiceFixture.CreateCall(number: numberCalling);

      referral.Calls = new List<Call>() { call1, call2 };
      _context.Referrals.Add(referral);
      _context.SaveChanges();

      UpdateReferralWithCallRequest request = Create(
        call2.Id,
        outcome.ToString(),
        call2.Number);

      // Act.
      UpdateReferralWithCallResponse response =
        await _service.UpdateReferralWithCall(request);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().NotBeNull();
        response.ResponseStatus.Should().Be(StatusType.Valid);

        Referral updatedReferral = _context.Referrals
          .Include(r => r.Calls)
          .Single(r => r.Id == referral.Id);
        updatedReferral.Should().NotBeNull();
        updatedReferral.Status.Should()
          .Be(ReferralStatus.FailedToContact.ToString());
        updatedReferral.ProgrammeOutcome.Should().Be(
          ProgrammeOutcome.InvalidContactDetails.ToString());
        updatedReferral.IsTelephoneValid.Should()
          .Be(expectedIsTelephoneValid);
        updatedReferral.IsMobileValid.Should().Be(expectedIsMobileValid);
      }
    }

    //public static IEnumerable<object[]> NumberStatusOutcomeStatusData()
    //{
    //  IEnumerable<object[]> data = new List<object[]>
    //  {
    //    CreateNsosObj("+441743000001", ChatBotCallOutcome.CallerReached),
    //    CreateNsosObj("+441743000002",
    //      ChatBotCallOutcome.TransferredToPhoneNumber),
    //    CreateNsosObj("+441743000003", ChatBotCallOutcome.TransferredToQueue),
    //    CreateNsosObj("+441743000004",
    //      ChatBotCallOutcome.TransferredToVoicemail),
    //    CreateNsosObj("+441743000005", ChatBotCallOutcome.VoicemailLeft),
    //    CreateNsosObj("+441743000006", ChatBotCallOutcome.Connected),
    //    CreateNsosObj("+441743000007", ChatBotCallOutcome.HungUp),
    //    CreateNsosObj("+441743000008", ChatBotCallOutcome.Engaged),
    //    CreateNsosObj("+441743000009", ChatBotCallOutcome.CallGuardian),
    //    CreateNsosObj("+441743000010", ChatBotCallOutcome.NoAnswer),
    //    CreateNsosObj("+441743000011", ChatBotCallOutcome.InvalidNumber),
    //    CreateNsosObj("+441743000012", ChatBotCallOutcome.Error),
    //  };

    //  return data;
    //}

    //public static object[] CreateNsosObj(
    //  string number,
    //  ChatBotCallOutcome outcome,
    //  ReferralStatus initialStatus = ReferralStatus.ChatBotCall1,
    //  ReferralStatus expectedStatus = ReferralStatus.ChatBotCall2) => new
    //  object[]
    //  {
    //    number,
    //    initialStatus,
    //    outcome,
    //    expectedStatus };

    private static UpdateReferralWithCallRequest Create(
      Guid id = default,
      string outcome = null,
      string number = null,
      DateTimeOffset timestamp = default) => new(
        id: id == default
          ? ServiceFixture.ChatBotCall1Referral.Calls.Single().Id
          : id,
        outcome: outcome ?? ChatBotCallOutcome.CallerReached.ToString(),
        number: number ??
          ServiceFixture.ChatBotCall1Referral.Calls.Single().Number,
        timestamp: timestamp == default
          ? DateTimeOffset.Now
          : timestamp);
  }

  public class GetReferralWithCalls : ChatBotServiceTests
  {
    public GetReferralWithCalls(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ArgumentNullException()
    {
      // Arrange.
      string expectedMessage = "Value cannot be null. (Parameter 'request')";
      UpdateReferralWithCallRequest request = null;

      try
      {
        // Act & assert.
        Task obj = TestPrivate_Helper.RunInstanceMethodAsync(
         typeof(ChatBotService), "GetReferralWithCalls", _service,
         new object[1] { request });
        await obj;
      }
      catch (ArgumentNullException ex)
      {
        ex.Message.Should().Be(expectedMessage);
      }
    }
  }

  public class ValidateReferralCanBeUpdated : ChatBotServiceTests
  {
    public ValidateReferralCanBeUpdated(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public void ArgumentNullException_Request_Null()
    {
      // Arrange.
      string expectedMessage = "Value cannot be null. (Parameter 'request')";
      Referral referral = RandomEntityCreator.CreateRandomReferral();
      UpdateReferralWithCallRequest request = null;
      UpdateReferralWithCallResponse response = null;

      // Act & assert.
      Task obj = TestPrivate_Helper.RunStaticMethodAsync(
        typeof(ChatBotService), "ValidateReferralCanBeUpdated",
        new object[3] { referral, request, response });

      if (obj.Exception.InnerExceptions.Count > 0)
      {
        obj.Exception.InnerExceptions[0].InnerException.Should()
          .BeOfType(typeof(ArgumentNullException));
        obj.Exception.InnerExceptions[0].InnerException.Message.Should()
          .Be(expectedMessage);
      }
      else
      {
        Assert.False(true);
      }
    }

    [Fact]
    public void ArgumentNullException_Response_Null()
    {
      // Arrange.
      string expectedMessage = "Value cannot be null. (Parameter 'response')";
      Referral referral = RandomEntityCreator.CreateRandomReferral();
      UpdateReferralWithCallRequest request = new();
      UpdateReferralWithCallResponse response = null;

      // Act & assert.
      Task obj = TestPrivate_Helper.RunStaticMethodAsync(
        typeof(ChatBotService), "ValidateReferralCanBeUpdated",
        new object[3]
        {
          referral,
          request,
          response
        });

      if (obj.Exception.InnerExceptions.Count > 0)
      {
        obj.Exception.InnerExceptions[0].InnerException.Should()
          .BeOfType(typeof(ArgumentNullException));
        obj.Exception.InnerExceptions[0].InnerException.Message.Should()
          .Be(expectedMessage);
      }
      else
      {
        Assert.False(true);
      }
    }
  }

  public class UpdateReferral : ChatBotServiceTests
  {
    public UpdateReferral(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ArgumentNullException_Referralt_Null()
    {
      // Arrange.
      string expectedMessage = "Value cannot be null. (Parameter 'referral')";
      Referral referral = null;
      UpdateReferralWithCallRequest request = null;

      try
      {
        // Act & assert.
        Task obj = TestPrivate_Helper.RunInstanceMethodAsync(
          typeof(ChatBotService),
          "UpdateReferral",
          _service,
          new object[2] {
            referral,
            request });
        await obj;
      }
      catch (ArgumentNullException ex)
      {
        ex.Message.Should().Be(expectedMessage);
      }
    }

    [Fact]
    public async Task ArgumentNullException_Request_Null()
    {
      // Arrange.
      string expectedMessage = "Value cannot be null. (Parameter 'updateReferralWithCallRequest')";
      Referral referral = RandomEntityCreator.CreateRandomReferral();
      UpdateReferralWithCallRequest request = null;

      try
      {
        // Act & assert.
        Task obj = TestPrivate_Helper.RunInstanceMethodAsync(
          typeof(ChatBotService),
          "UpdateReferral",
          _service,
          new object[2] {
            referral,
            request });
        await obj;
      }
      catch (ArgumentNullException ex)
      {
        ex.Message.Should().Be(expectedMessage);
      }
    }

    [Fact]
    public async Task ArgumentException_ReferralNotFound()
    {
      // Arrange.
      Guid requestId = Guid.NewGuid();
      UpdateReferralWithCallRequest request = new()
      {
        Id = requestId
      };
      Referral referral =
        RandomEntityCreator.CreateRandomReferral();
      string expectedMessage = $"Unable to find a call with an id of {requestId} for the referral" +
        $" id {referral.Id}.";

      try
      {
        // Act & assert.
        Task obj = TestPrivate_Helper.RunInstanceMethodAsync(
         typeof(ChatBotService), "UpdateReferral", _service,
         new object[2] { referral, request });
        await obj;
      }
      catch (ChatBotCallNotFoundException ex)
      {
        ex.Message.Should().Be(expectedMessage);
      }
    }
  }

  public class RemoveReferrals : ChatBotServiceTests
  {
    public RemoveReferrals(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task Removal_Success()
    {
      // Arrange.
      List<Referral> referrals = new();
      Referral referral1 = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New);
      Referral referral2 = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New);

      _context.Referrals.Add(referral1);
      _context.Referrals.Add(referral2);
      await _context.SaveChangesAsync();
      referrals.Add(referral1);
      referrals.Add(referral2);

      // Act.
      string response = await _service.RemoveReferrals(referrals);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<string>();
        response.Should().Be("");

        Referral refCheck1 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral1.Id);
        Referral refCheck2 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral2.Id);
        refCheck1.Should().BeNull();
        refCheck2.Should().BeNull();
      }
    }

    [Fact]
    public async Task Removal_UnSuccessful_StatusReason()
    {
      // Arrange.
      List<Referral> referrals = new();
      Referral referral1 = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New);
      Referral referral2 = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New);

      _context.Referrals.Add(referral1);
      _context.Referrals.Add(referral2);
      await _context.SaveChangesAsync();

      referral1.StatusReason = "Invalid Reason";
      string response1 =
        $"Fake entry using StatusReason " +
        $"{referral1.StatusReason} is not loaded\r\n";
      referral2.StatusReason = "Another Invalid Reason";
      string expectedResponse = response1 +
        $"Fake entry using StatusReason {referral2.StatusReason} " +
        $"is not loaded\r\n";

      referrals.Add(referral1);
      referrals.Add(referral2);

      // Act.
      string response = await _service.RemoveReferrals(referrals);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<string>();
        response.Should().Be(expectedResponse);

        Referral refCheck1 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral1.Id);
        Referral refCheck2 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral2.Id);
        refCheck1.Should().NotBeNull();
        refCheck2.Should().NotBeNull();
      }
    }

    [Fact]
    public async Task Removal_UnSuccessful_Ubrn()
    {
      // Arrange.
      List<Referral> referrals = new();
      Referral referral1 = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New);

      _context.Referrals.Add(referral1);
      await _context.SaveChangesAsync();

      referral1.Ubrn = "9999999997";
      string expectedResponse =
        $"Fake entry using UBRN {referral1.Ubrn} is not loaded\r\n";
      referrals.Add(referral1);

      // Act.
      string response = await _service.RemoveReferrals(referrals);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<string>();
        response.Should().Be(expectedResponse);

        Referral refCheck1 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral1.Id);
        refCheck1.Should().NotBeNull();
      }
    }
  }

  public class AddReferrals : ChatBotServiceTests, IDisposable
  {
    public AddReferrals(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    public override void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
      base.Dispose();
    }

    [Fact]
    public async Task ArgumentException_StatusReason()
    {
      // Arrange.
      List<Referral> model = new();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New,
        statusReason: "TestingChatBot");
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      model.Add(referral);

      // Act & assert.
      ArgumentException exception =
        await Assert.ThrowsAsync<ArgumentException>(
        async () => await _service.AddReferrals(model));
    }

    [Fact]
    public async Task ArgumentException_Ubrn()
    {
      // Arrange.
      List<Referral> model = new();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New,
        ubrn: "9191919191");
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      referral.StatusReason = "NonExistent Reason";
      model.Add(referral);

      // Act & assert.
      ArgumentException exception =
        await Assert.ThrowsAsync<ArgumentException>(
          async () => await _service.AddReferrals(model));
    }

    [Fact]
    public async Task ValidCreation()
    {
      // Arrange.
      bool expectedResult = true;
      List<Referral> model = new();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New,
        ubrn: "9191919191",
        statusReason: "NonExistent Reason");
      model.Add(referral);

      // Act. 
      bool result = await _service.AddReferrals(model);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().Be(expectedResult);
        Referral refToRemove = _context.Referrals
          .Where(r => r.StatusReason == referral.StatusReason)
          .SingleOrDefault();
        refToRemove.Should().NotBeNull();
      }
    }
  }

  public class GetReferralsWithCalls : ChatBotServiceTests, IDisposable
  {
    private readonly Expression<Func<Referral, bool>>
      _hasProvider = x => x.ProviderId != null;

    public GetReferralsWithCalls(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    public override void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Calls.RemoveRange(_context.Calls);
      _context.Providers.RemoveRange(_context.Providers);
      _context.SaveChanges();
      base.Dispose();
    } 

    [Fact]
    public async Task ReturnNoList_InResponse()
    {
      // Arrange.
      int expectedCount = 0;
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.ChatBotCall1);
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      Call call1 = RandomEntityCreator.CreateRandomChatBotCall(
        isActive: true);
      call1.Sent = default;
      call1.ModifiedAt = DateTimeOffset.Now.AddHours(0);
      call1.ReferralId = referral.Id;
      _context.Calls.Add(call1);
      await _context.SaveChangesAsync();

      // Act.
      List<Referral> response =
        await _service.GetReferralsWithCalls(_hasProvider);

      // Assert.
      response.Count.Should().Be(expectedCount);
    }

    [Fact]
    public async Task ReturnList_InResponse()
    {
      // Arrange.
      int expectedCount = 1;
      Provider provider = RandomEntityCreator.CreateRandomProvider();
      _context.Providers.Add(provider);
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.ChatBotCall1);
      referral.ProviderId = provider.Id;
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      Call call1 = RandomEntityCreator.CreateRandomChatBotCall(
        isActive: true);
      call1.Sent = default;
      call1.ModifiedAt = DateTimeOffset.Now.AddHours(0);
      call1.ReferralId = referral.Id;
      _context.Calls.Add(call1);
      await _context.SaveChangesAsync();

      // Act.
      List<Referral> response =
        await _service.GetReferralsWithCalls(_hasProvider);

      // Assert.
      response.Count.Should().Be(expectedCount);
    }
  }

  public class GetReferralCallList : ChatBotServiceTests
  {
    public GetReferralCallList(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReturnListInResponse()
    {
      // Arrange.
      StatusType expectedStatus = StatusType.Valid;
      Referral referral1 = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.ChatBotCall1);
      Call call1 = RandomEntityCreator.CreateRandomChatBotCall(
        isActive: true,
        sent: default);
      call1.Sent = default;

      _context.Referrals.Add(referral1);
      await _context.SaveChangesAsync();

      call1.ReferralId = referral1.Id;
      _context.Calls.Add(call1);
      await _context.SaveChangesAsync();

      GetReferralCallListRequest request = new();

      // Act.
      GetReferralCallListResponse response =
        await _service.GetReferralCallList(request);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<GetReferralCallListResponse>();
        response.Status.Should().Be(expectedStatus);
        response.Arcus.Callees.Count().Should().Be(1);
      }
    }
  }

  public class UpdateReferralCallListSent : ChatBotServiceTests
  {
    public UpdateReferralCallListSent(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ArgumentNullException()
    {
      // Arrange.
      List<ICallee> model = null;

      // Act & assert.
      ArgumentNullException exception =
        await Assert.ThrowsAsync<ArgumentNullException>(
        async () => await _service.UpdateReferralCallListSent(model));
    }

    [Fact]
    public async Task UpdateCallees_Success()
    {
      // Arrange.
      List<ICallee> model = new();
      DateTimeOffset sentDate = DateTimeOffset.Now.LocalDateTime;

      Call call1 = RandomEntityCreator.CreateRandomChatBotCall();
      Call call2 = RandomEntityCreator.CreateRandomChatBotCall();
      call1.Sent = default;
      call2.Sent = default;

      _context.Calls.Add(call1);
      _context.Calls.Add(call2);
      await _context.SaveChangesAsync();

      Callee callee1 = new()
      {
        Id = call1.Id.ToString()
      };
      Callee callee2 = new()
      {
        Id = call2.Id.ToString()
      };
      model.Add(callee1);
      model.Add(callee2);

      // Act.
      await _service.UpdateReferralCallListSent(model);

      // Assert.
      using (new AssertionScope())
      {
        call1.Sent.Should().NotBe(default);
        call2.Sent.Should().NotBe(default);
        call1.Sent.Should().BeOnOrAfter(sentDate);
        call2.Sent.Should().BeOnOrAfter(sentDate);
      }
    }
  }

  public class UpdateNullNumbersAsync : ChatBotServiceTests, IDisposable
  {
    public UpdateNullNumbersAsync(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    public override void Dispose()
    {
      _context.Calls.RemoveRange(_context.Calls);
      _context.SaveChanges();
      base.Dispose();
    }

    [Fact]
    public async Task NoCallsHaveNullNumbers_ZeroChanges()
    {
      // Arrange.
      Call call = RandomEntityCreator.CreateRandomChatBotCall();
      _context.Calls.Add(call);
      _context.SaveChanges();
      _context.Entry(call).State = EntityState.Detached;

      // Act.
      int numOfCallsUpdated = await _service.UpdateNullNumbersAsync();
      Call callUpdated = _context.Calls.Single(c => c.Id == call.Id);

      // Assert.
      using (new AssertionScope())
      {
        numOfCallsUpdated.Should().Be(0, because: "No numbers are null.");
        callUpdated.Should().BeEquivalentTo(call,
          because: "The entity should not have been updated");
      }
    }

    [Fact]
    public async Task OneCallsHasNullNumbers_OutcomeAndSentUpdated()
    {
      // Arrange.
      Call call = RandomEntityCreator.CreateRandomChatBotCall();
      call.Number = null;
      _context.Calls.Add(call);
      _context.SaveChanges();
      _context.Entry(call).State = EntityState.Detached;

      // Act.
      int numOfCallsUpdated = await _service.UpdateNullNumbersAsync();
      Call callUpdated = _context.Calls.Single(c => c.Id == call.Id);

      // Assert.
      using (new AssertionScope())
      {
        numOfCallsUpdated.Should().Be(1, because: "One number is null.");
        callUpdated.Should().BeEquivalentTo(call, options => options
          .Excluding(c => c.ModifiedAt)
          .Excluding(c => c.ModifiedByUserId)
          .Excluding(c => c.Outcome)
          .Excluding(c => c.Sent),
          because: "The entity should be updated and modified.");

        callUpdated.ModifiedAt.Should().BeAfter(call.ModifiedAt);
        callUpdated.ModifiedByUserId.Should().Be(TEST_USER_ID);
        callUpdated.Outcome.Should()
          .Be(ChatBotCallOutcome.InvalidNumber.ToString());
        callUpdated.Sent.Should().Be(new DateTime(1900, 1, 1));
      }
    }
  }

  public class PrepareCallsAsync : ChatBotServiceTests, IDisposable
  {

    public PrepareCallsAsync(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      CleanUp();
    }

    public override void Dispose()
    {
      CleanUp();
      base.Dispose();
      GC.SuppressFinalize(this);
    }

    private void CleanUp()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Calls.RemoveRange(_context.Calls);
      _context.TextMessages.RemoveRange(_context.TextMessages);
      _context.SaveChanges();
    }

    [Fact]
    public async Task InvalidNumbersNotAddedToCallsTable()
    {
      // Arrange.
      Referral refInvalidMobileAndTel =
        RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage2,
          isMobileValid: false,
          isTelephoneValid: false);
      refInvalidMobileAndTel.Mobile = null;
      refInvalidMobileAndTel.Telephone = null;

      _context.Referrals.Add(refInvalidMobileAndTel);
      _context.SaveChanges();
      _context.Entry(refInvalidMobileAndTel).State = EntityState.Detached;

      // Act.
      PrepareCallsForTodayResponse response =
        await _service.PrepareCallsAsync();

      // Assert.
      response.CallsPrepared.Should().Be(0);
      Referral refCheck = _context.Referrals
        .Single(r => r.Id == refInvalidMobileAndTel.Id);
      refCheck.Should().BeEquivalentTo(
        refInvalidMobileAndTel,
        options => options
          .Excluding(r => r.Status)
          .Excluding(r => r.ProgrammeOutcome)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.Audits));
      refCheck.Status.Should().Be(ReferralStatus.FailedToContact.ToString());
      refCheck.ProgrammeOutcome.Should().Be(ProgrammeOutcome.InvalidContactDetails.ToString());
      refCheck.ModifiedAt.Should().BeAfter(refInvalidMobileAndTel.ModifiedAt);
      refCheck.ModifiedByUserId.Should().Be(TEST_USER_ID);
      _context.Calls.Any(c => c.ReferralId == refInvalidMobileAndTel.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Prepare_NoUpdate_Success()
    {
      // Arrange.
      int expectedCallsPrepared = 0;
      Referral referral1 = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.TextMessage2);
      Referral referral2 = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.ChatBotCall1);

      TextMessage text1 = RandomEntityCreator.CreateRandomTextMessage(
        isActive: true,
        sent: DateTimeOffset.Now.AddDays(0));
      Call call1 = RandomEntityCreator.CreateRandomChatBotCall(
        isActive: true,
        sent: DateTimeOffset.Now.AddDays(-1));

      _context.Referrals.Add(referral1);
      _context.Referrals.Add(referral2);
      await _context.SaveChangesAsync();

      text1.ReferralId = referral1.Id;
      call1.ReferralId = referral2.Id;
      _context.TextMessages.Add(text1);
      _context.Calls.Add(call1);
      await _context.SaveChangesAsync();

      // Act.
      PrepareCallsForTodayResponse response =
        await _service.PrepareCallsAsync();

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<PrepareCallsForTodayResponse>();
        response.CallsPrepared.Should().Be(expectedCallsPrepared);
        Referral refCheck1 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral1.Id);
        Referral refCheck2 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral2.Id);
        refCheck1.Should().NotBeNull();
        refCheck2.Should().NotBeNull();
        refCheck1.Status.Should().Be(ReferralStatus.TextMessage2.ToString());
        refCheck2.Status.Should().Be(ReferralStatus.ChatBotCall1.ToString());
      }
    }

    [Fact]
    public async Task Prepare_Update_Success()
    {
      // Arrange.
      int expectedCallsPrepared = 1;
      Referral referral1 = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.TextMessage2);

      TextMessage text1 = RandomEntityCreator.CreateRandomTextMessage(
        isActive: true,
        sent: DateTimeOffset.Now.AddDays(-4));

      _context.Referrals.Add(referral1);
      await _context.SaveChangesAsync();
      _context.Entry(referral1).State = EntityState.Detached;

      text1.ReferralId = referral1.Id;
      _context.TextMessages.Add(text1);
      await _context.SaveChangesAsync();

      // Act.
      PrepareCallsForTodayResponse response =
        await _service.PrepareCallsAsync();

      // Assert.
      response.Should().BeOfType<PrepareCallsForTodayResponse>();
      response.CallsPrepared.Should().Be(expectedCallsPrepared);
      Referral refCheck1 =
        _context.Referrals.SingleOrDefault(r => r.Id == referral1.Id);
      refCheck1.Should().BeEquivalentTo(referral1, options => options
        .Excluding(r => r.Status)
        .Excluding(r => r.ModifiedAt)
        .Excluding(r => r.ModifiedByUserId)
        .Excluding(r => r.Audits)
        .Excluding(r => r.Calls)
        .Excluding(r => r.TextMessages));
      refCheck1.Should().NotBeNull();
      refCheck1.Status.Should().Be(ReferralStatus.ChatBotCall1.ToString());
      refCheck1.ModifiedAt.Should().BeAfter(referral1.ModifiedAt);
      refCheck1.ModifiedByUserId.Should().Be(TEST_USER_ID);
    }
  }

  public class UpdateReferralStatusForCallOutcome : ChatBotServiceTests
  {
    public UpdateReferralStatusForCallOutcome(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public void UpdateReferralStatusForCallOutcome_UpdateSuccess()
    {
      Referral referral =
        RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
      Call requestedCall =
        RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
      string outcome = ChatBotCallOutcome.CallerReached.ToString();

      TestPrivate_Helper.RunStaticMethod(
        typeof(ChatBotService),
        "UpdateReferralStatusForCallOutcome",
        new object[3] {
          referral,
          requestedCall,
          outcome});

      referral.IsTelephoneValid.Should().BeTrue();
    }

    [Fact]
    public void Exception_Null_Referral()
    {
      // Arrange.
      string expectedMessage = "Value cannot be null.";
      Referral referral = null;
      Call requestedCall =
        RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
      string outcome = ChatBotCallOutcome.CallerReached.ToString();

      try
      {
        // Act & assert.
        TestPrivate_Helper.RunStaticMethod(
          typeof(ChatBotService),
          "UpdateReferralStatusForCallOutcome",
          new object[3] {
            referral,
            requestedCall,
            outcome });
      }
      catch (ArgumentNullException ex)
      {
        Assert.Contains(expectedMessage, ex.Message);
      }
    }

    [Fact]
    public void Exception_Null_RequestedCall()
    {
      // Arrange.
      string expectedMessage = "Value cannot be null.";
      Referral referral =
        RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
      Call requestedCall = null;
      string outcome = ChatBotCallOutcome.CallerReached.ToString();

      try
      {
        // Act & assert.
        TestPrivate_Helper.RunStaticMethod(
          typeof(ChatBotService),
          "UpdateReferralStatusForCallOutcome",
          new object[3] {
            referral,
            requestedCall,
            outcome });
      }
      catch (ArgumentNullException ex)
      {
        Assert.Contains(expectedMessage, ex.Message);
      }
    }
    [Fact]
    public void Exception_Null_Outcome()
    {
      // Arrange.
      string expectedMessage = "Value cannot be null or white space";
      Referral referral =
        RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
      Call requestedCall =
        RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
      string outcome = "";

      try
      {
        // Act & assert.
        TestPrivate_Helper.RunStaticMethod(
          typeof(ChatBotService),
          "UpdateReferralStatusForCallOutcome",
          new object[3] {
            referral,
            requestedCall,
            outcome });
      }
      catch (ArgumentNullOrWhiteSpaceException ex)
      {
        Assert.Contains(expectedMessage, ex.Message);
      }
    }

    [Fact]
    public void Exception_Outcome_Exception()
    {
      // Arrange.
      string expectedMessage = "Unknown outcome of";
      Referral referral =
        RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
      Call requestedCall =
        RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
      string outcome = "InvalidOutcome";

      try
      {
        // Act & assert.
        TestPrivate_Helper.RunStaticMethod(
          typeof(ChatBotService),
          "UpdateReferralStatusForCallOutcome",
          new object[3] {
            referral,
            requestedCall,
            outcome });
      }
      catch (ArgumentException ex)
      {
        Assert.Contains(expectedMessage, ex.Message);
      }
    }

    [Fact]
    public void Exception_Null_ReferralCalls()
    {
      // Arrange.
      string expectedMessage = "Calls is null";
      Referral referral =
        RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
      referral.Calls = null;
      Call requestedCall =
      RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
      string outcome = ChatBotCallOutcome.CallerReached.ToString();
      try
      {
        // Act & assert.
        TestPrivate_Helper.RunStaticMethod(
          typeof(ChatBotService),
          "UpdateReferralStatusForCallOutcome",
          new object[3] {
            referral,
            requestedCall,
            outcome });
      }
      catch (ArgumentNullException ex)
      {
        Assert.Contains(expectedMessage, ex.Message);
      }
    }

    [Fact]
    public void Exception_RequestCall_Number_Mismatch()
    {
      // Arrange.
      string reqNumber = "77865454347";
      string expectedMessage = $"Number {reqNumber} not found in referral";
      Referral referral =
        RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
      Call requestedCall =
      RandomEntityCreator.CreateRandomChatBotCall(number: reqNumber);
      string outcome = ChatBotCallOutcome.CallerReached.ToString();
      try
      {
        // Act & assert.
        TestPrivate_Helper.RunStaticMethod(
          typeof(ChatBotService),
          "UpdateReferralStatusForCallOutcome",
          new object[3] {
            referral,
            requestedCall,
            outcome });
      }
      catch (ChatBotNumberNotFoundException ex)
      {
        Assert.Contains(expectedMessage, ex.Message);
      }
    }

    [Fact]
    public void Exception_ReferralInvalidStatusException()
    {
      // Arrange.
      ReferralStatus expectedStatus = ReferralStatus.CancelledByEreferrals;

      string expectedMessage = $"Expected a status of ChatBotCall1 but found {expectedStatus}.";
      Referral referral =
        RandomEntityCreator.CreateRandomReferral(
          telephone: "1234567890",
          status: expectedStatus);
      Call requestedCall =
      RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
      string outcome = ChatBotCallOutcome.TransferringToRmc.ToString();
      try
      {
        // Act & assert.
        TestPrivate_Helper.RunStaticMethod(
          typeof(ChatBotService),
          "UpdateReferralStatusForCallOutcome",
          new object[3] {
            referral,
            requestedCall,
            outcome });
      }
      catch (ReferralInvalidStatusException ex)
      {
        Assert.Contains(expectedMessage, ex.Message);
      }
    }
  }

  public class UpdateReferralTransferRequestAsync :
    ChatBotServiceTests, IDisposable
  {
    public override void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
      base.Dispose();
    }

    public UpdateReferralTransferRequestAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ArgumentNullException()
    {
      // Arrange.
      UpdateReferralTransferRequest request = null;

      // Act & assert.
      ArgumentNullException exception =
        await Assert.ThrowsAsync<ArgumentNullException>(
          async () =>
          await _service.UpdateReferralTransferRequestAsync(request));
    }

    [Fact]
    public async Task UpdateReferralTransferRequestAsync_Unfound()
    {
      string telephone = "+1234567890";
      string outcome = ChatBotCallOutcome.TransferringToRmc.ToString();
      StatusType expectedStatus = StatusType.Invalid;

      UpdateReferralTransferRequest request = new()
      {
        Number = telephone,
        Outcome = outcome,
        Timestamp = DateTimeOffset.Now
      };

      Referral referral =
        RandomEntityCreator.CreateRandomReferral(telephone: telephone);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      UpdateReferralTransferResponse response =
        await _service.UpdateReferralTransferRequestAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<UpdateReferralTransferResponse>();
        response.ResponseStatus.Should().Be(expectedStatus);
      }
    }

    [Fact]
    public async Task UpdateReferralTransferRequestAsync_TelephoneInValid()
    {
      string telephone = "1234567890";
      string outcome = ChatBotCallOutcome.TransferringToRmc.ToString();
      StatusType expectedStatus = StatusType.Invalid;
      string expectedErrorMessage = "The field Number is not a valid " +
        "telephone number.";
      UpdateReferralTransferRequest request = new()
      {
        Number = telephone,
        Outcome = outcome,
        Timestamp = DateTimeOffset.Now
      };

      Referral referral =
        RandomEntityCreator.CreateRandomReferral(telephone: telephone);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      UpdateReferralTransferResponse response =
        await _service.UpdateReferralTransferRequestAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<UpdateReferralTransferResponse>();
        response.ResponseStatus.Should().Be(expectedStatus);
        response.Errors[0].Should().Be(expectedErrorMessage);
      }
    }

    [Fact]
    public async Task UpdateReferralWithTransferRequestAsync_UpdateSuccess()
    {
      string expectedRefStatus = ReferralStatus.ChatBotTransfer.ToString();
      string telephone = "+1234567890";
      string outcome = ChatBotCallOutcome.TransferringToRmc.ToString();
      StatusType expectedStatus = StatusType.Valid;

      UpdateReferralTransferRequest request = new()
      {
        Number = telephone,
        Outcome = outcome,
        Timestamp = DateTimeOffset.Now
      };

      Referral referral =
        RandomEntityCreator.CreateRandomReferral(telephone: telephone);
      referral.Status = ReferralStatus.ChatBotCall1.ToString();

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      UpdateReferralTransferResponse response =
        await _service.UpdateReferralTransferRequestAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<UpdateReferralTransferResponse>();
        response.ResponseStatus.Should().Be(expectedStatus);
        referral.Status.Should().Be(expectedRefStatus);
      }
    }

    [Fact]
    public async Task UpdateReferralWithTransferRequestAsync_NonDistinct()
    {
      string telephone = "+1234567890";
      string outcome = ChatBotCallOutcome.TransferringToRmc.ToString();
      StatusType expectedStatus = StatusType.Invalid;

      UpdateReferralTransferRequest request = new()
      {
        Number = telephone,
        Outcome = outcome,
        Timestamp = DateTimeOffset.Now
      };

      Referral referral1 =
        RandomEntityCreator.CreateRandomReferral(telephone: telephone);
      referral1.Status = ReferralStatus.ChatBotCall1.ToString();
      Referral referral2 =
        RandomEntityCreator.CreateRandomReferral(mobile: telephone);
      referral2.Status = ReferralStatus.ChatBotCall1.ToString();

      _context.Referrals.Add(referral1);
      _context.Referrals.Add(referral2);
      await _context.SaveChangesAsync();

      // Act.
      UpdateReferralTransferResponse response =
        await _service.UpdateReferralTransferRequestAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<UpdateReferralTransferResponse>();
        response.ResponseStatus.Should().Be(expectedStatus);
      }
    }
  }

  private class Tel
  {
    public const bool IsValid_False = false;
    public const bool IsValid_True = true;
  }

  private class Mob
  {
    public const bool IsValid_False = false;
    public const bool IsValid_True = true;
  }
}

public class TestPrivate_Helper
{

  public static void RunStaticMethod(
    Type t,
    string strMethod,
    object[] aobjParams)
  {
    BindingFlags eFlags = BindingFlags.Static | BindingFlags.NonPublic;
    RunMethod(t, strMethod, null, aobjParams, eFlags);
  }

  public async static Task RunStaticMethodAsync(
    Type t,
    string strMethod,
    object[] aobjParams)
  {
    try
    {
      BindingFlags eFlags = BindingFlags.Static | BindingFlags.NonPublic;
      await RunMethodAsync(t, strMethod, null, aobjParams, eFlags);
    }
    catch
    {
      throw;
    }
  }

  public async static Task RunInstanceMethodAsync(
    Type t,
    string strMethod,
    object objInstance,
    object[] aobjParams)
  {
    try
    {
      BindingFlags eFlags = BindingFlags.Instance | BindingFlags.NonPublic;
      await RunMethodAsync(t, strMethod, objInstance, aobjParams, eFlags);
    }
    catch
    {
      throw;
    }
  }

  private static void RunMethod(
    Type t,
    string strMethod,
    object objInstance,
    object[] aobjParams,
    BindingFlags eFlags)
  {
    MethodInfo mInfo;
    mInfo = t.GetMethod(strMethod, eFlags);
    if (mInfo == null)
    {
      throw new ArgumentException("There is no method '" +
        strMethod + "' for type '" + t.ToString() + "'.");
    }

    try
    {
      mInfo.Invoke(objInstance, aobjParams);
    }
    catch (Exception ex)
    {
      if (ex is TargetInvocationException)
      {
        throw ex.GetBaseException();
      }
      else
      {
        throw;
      }
    }
  }

  private async static Task RunMethodAsync(
    Type t,
    string strMethod,
    object objInstance,
    object[] aobjParams,
    BindingFlags eFlags)
  {
    MethodInfo mInfo;
    try
    {
      mInfo = t.GetMethod(strMethod, eFlags);
      if (mInfo == null)
      {
        throw new ArgumentException("There is no method '" +
         strMethod + "' for type '" + t.ToString() + "'.");
      }

      Task resultTask = (Task)mInfo.Invoke(objInstance, aobjParams);
      if (resultTask is Task task)
      {
        await task;
      }
    }
    catch
    {
      throw;
    }
  }
}
