using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
using static WmsHub.Business.Enums.ChatBotCallOutcome;
using static WmsHub.Business.Enums.ReferralStatus;

namespace WmsHub.Business.Tests.Services
{
  [Collection("Service collection")]
  public class ChatBotServiceTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly ChatBotService _service;
    private readonly Mock<IHubClients> _mockClients = new();

    public ChatBotServiceTests(ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context = new DatabaseContext(_serviceFixture.Options);

      Mock<IClientProxy> mockClientProxy = new();
      _mockClients.Setup(clients => clients.All)
        .Returns(mockClientProxy.Object);

      var hubContext = new Mock<IHubContext<SignalRHub>>();
      hubContext.Setup(x => x.Clients).Returns(() => _mockClients.Object);

      _service = new ChatBotService(
        _context,
        _serviceFixture.Mapper,
        TestConfiguration.CreateArcusOptions(),
        hubContext.Object)
      {
        User = GetClaimsPrincipal()
      };

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
        // arrange
        UpdateReferralWithCallRequest model = null;

        // act & assert
        ArgumentNullException exception =
          await Assert.ThrowsAsync<ArgumentNullException>(
          async () => await _service.UpdateReferralWithCall(model));
      }

      [Fact]
      public async Task UpdateReferral_InValid_Test()
      {
        // arrange

        var request = Create();
        request.Timestamp = default;
        string expectedError = $"The Timestamp field '01/01/0001 " +
          "00:00:00 +00:00' is invalid.";

        // act
        var response = await _service.UpdateReferralWithCall(request);

        // assert
        response.Should().BeEquivalentTo(request);
        response.Should().BeOfType<UpdateReferralWithCallResponse>();
        response.ResponseStatus.Should().Be(StatusType.Invalid);
        response.GetErrorMessage().Should().Be(expectedError);
      }

      [Fact]
      public async Task UpdateReferral_Valid_Test()
      {
        // arrange
        var request = Create();

        // act
        var response = await _service.UpdateReferralWithCall(request);

        // assert
        response.Should().BeOfType<UpdateReferralWithCallResponse>();
        response.ResponseStatus.Should().Be(StatusType.Valid);
        response.Should().BeEquivalentTo(request);

        var call = _context.Calls.SingleOrDefault(c => c.Id == request.Id);
        call.Should().NotBeNull();
        call.Called.Should().Be(request.Timestamp);
        call.Number.Should().Be(request.Number);
        call.Outcome.Should().Be(request.Outcome);

        var referral = _context.Referrals.SingleOrDefault(
          r => r.Id == call.ReferralId);
        referral.Should().NotBeNull();
        referral.Status.Should().Be(ChatBotCall1.ToString());
      }

      [Fact]
      public async Task UpdateReferral_IdDoesNotExist_Test()
      {
        // arrange
        var request = Create(id: Guid.NewGuid());

        // act
        var response = await _service.UpdateReferralWithCall(request);

        // assert
        response.Should().BeOfType<UpdateReferralWithCallResponse>();
        response.ResponseStatus.Should().Be(StatusType.CallIdDoesNotExist);
        response.Should().BeEquivalentTo(request);
      }

      [Fact]
      public async Task UpdateReferral_TelephoneNumberMismatch_Test()
      {
        // arrange
        var request =
          Create(number: ServiceFixture.NEVER_USED_TELEPHONE_NUMBER);

        // act
        var response = await _service.UpdateReferralWithCall(request);

        // assert
        response.Should().BeOfType<UpdateReferralWithCallResponse>();
        response.ResponseStatus.Should().Be(StatusType.TelephoneNumberMismatch);
        response.Should().BeEquivalentTo(request);
      }

      //[MemberData(nameof(NumberStatusOutcomeStatusData))]
      [InlineData(CallerReached, true, 0)]
      [InlineData(TransferredToPhoneNumber, true, 0)]
      [InlineData(TransferredToQueue, true, 0)]
      [InlineData(TransferredToVoicemail, true, 0)]
      [InlineData(VoicemailLeft, true, 0)]
      [InlineData(Connected, true, 0)]
      [InlineData(HungUp, true, 0)]
      [InlineData(Engaged, true, 0)]
      [InlineData(CallGuardian, false, 0)]
      [InlineData(NoAnswer, true, 0)]
      [InlineData(InvalidNumber, false, 0)]
      [InlineData(Error, true, 0)]
      [Theory]
      public async Task UpdateReferralWithCall_SecondChatBotCallOutcome(
        ChatBotCallOutcome outcome,
        bool expectedIsMobileValid,
        int signalRTimesCalled)
      {
        // arrange
        var referral = ServiceFixture.CreateReferral(
          status: ChatBotCall1,
          isTelephoneValid: false,
          mobile: ServiceFixture.GetUniqueMobileNumber());

        var call1 = ServiceFixture.CreateCall(
          called: DateTimeOffset.Now,
          number: referral.Mobile,
          outcome: HungUp.ToString(),
          sent: DateTimeOffset.Now.AddMinutes(-5));
        var call2 = ServiceFixture.CreateCall(number: referral.Mobile);

        referral.Calls = new List<Entities.Call>() { call1, call2 };
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        var request = Create(call2.Id, outcome.ToString(), call2.Number);

        // act
        var response = await _service.UpdateReferralWithCall(request);

        // assert
        response.Should().NotBeNull();
        response.ResponseStatus.Should().Be(StatusType.Valid);

        var updatedReferral = _context.Referrals
          .Include(r => r.Calls)
          .Single(r => r.Id == referral.Id);
        updatedReferral.Should().NotBeNull();
        updatedReferral.Status.Should()
         .Be(ReferralStatus.ChatBotCall1.ToString());
        updatedReferral.IsMobileValid.Should().Be(expectedIsMobileValid);

        if (signalRTimesCalled == 0)
          _mockClients.Verify(clients => clients.All, Times.Never);
        else
          _mockClients.Verify(clients => clients.All, Times.Once);
      }

      [Fact]
      public async Task UpdateReferralWithCall_TransferToRMc()
      {
        // arrange      // [InlineData(TransferringToRmc,  true, 1)]
        var referral = ServiceFixture.CreateReferral(
         status: ChatBotCall1,
         isTelephoneValid: false,
         mobile: ServiceFixture.GetUniqueMobileNumber());

        var call1 = ServiceFixture.CreateCall(
          called: DateTimeOffset.Now,
          number: referral.Mobile,
          outcome: HungUp.ToString(),
          sent: DateTimeOffset.Now.AddMinutes(-5));
        var call2 = ServiceFixture.CreateCall(number: referral.Mobile);

        referral.Calls = new List<Entities.Call>() { call1, call2 };
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        var request = Create(call2.Id, TransferringToRmc.ToString(),
          call2.Number);

        // act
        var response = await _service.UpdateReferralWithCall(request);

        // assert
        response.Should().NotBeNull();
        response.ResponseStatus.Should().Be(StatusType.Valid);

        var updatedReferral = _context.Referrals
         .Include(r => r.Calls)
         .Single(r => r.Id == referral.Id);
        updatedReferral.Should().NotBeNull();
        updatedReferral.Status.Should()
         .Be(ReferralStatus.ChatBotTransfer.ToString());
        updatedReferral.IsMobileValid.Should().BeTrue();
        _mockClients.Verify(clients => clients.All, Times.AtLeastOnce);
      }

      [InlineData(CallGuardian, Tel.IsValid_False, Mob.IsValid_True,
        Mob.IsValid_False, Tel.IsValid_False, true)]
      [InlineData(CallGuardian, Tel.IsValid_True, Mob.IsValid_True,
        Mob.IsValid_True, Tel.IsValid_False, false)]
      [InlineData(CallGuardian, Tel.IsValid_True, Mob.IsValid_False,
        Mob.IsValid_False, Tel.IsValid_False, false)]
      [InlineData(InvalidNumber, Tel.IsValid_False, Mob.IsValid_True,
        Mob.IsValid_False, Tel.IsValid_False, true)]
      [InlineData(InvalidNumber, Tel.IsValid_True, Mob.IsValid_True,
        Mob.IsValid_True, Tel.IsValid_False, false)]
      [InlineData(InvalidNumber, Tel.IsValid_True, Mob.IsValid_False,
        Mob.IsValid_False, Tel.IsValid_False, false)]
      [InlineData(CallGuardian, Tel.IsValid_True, Mob.IsValid_True,
        Mob.IsValid_False, Tel.IsValid_True, true)]
      [InlineData(InvalidNumber, Tel.IsValid_True, Mob.IsValid_True,
        Mob.IsValid_False, Tel.IsValid_True, true)]
      [Theory]
      public async Task UpdateReferralCallGuardianIsTelephone(
        ChatBotCallOutcome outcome,
        bool isTelephoneValid,
        bool isMobileValid,
        bool expectedIsMobileValid,
        bool expectedIsTelephoneValid,
        bool useMobileToCall)
      {
        //Arrange 
        string mobile = ServiceFixture.GetUniqueMobileNumber();
        string phone = ServiceFixture.GetUniqueTelephoneNumber();
        string numberCalling = useMobileToCall ? mobile : phone;
        Referral referral = ServiceFixture.CreateReferral(
          status: ChatBotCall1,
          isTelephoneValid: isTelephoneValid,
          telephone: phone,
          isMobileValid: isMobileValid,
          mobile: mobile);

        Call call1 = ServiceFixture.CreateCall(
          called: DateTimeOffset.Now,
          number: isTelephoneValid ? referral.Telephone : referral.Mobile,
          outcome: HungUp.ToString(),
          sent: DateTimeOffset.Now.AddMinutes(-5));
        Call call2 = ServiceFixture.CreateCall(number: numberCalling);

        referral.Calls = new List<Entities.Call>() { call1, call2 };
        _context.Referrals.Add(referral);
        _context.SaveChanges();

        UpdateReferralWithCallRequest request =
          Create(call2.Id, outcome.ToString(), call2.Number);

        // act
        UpdateReferralWithCallResponse response =
          await _service.UpdateReferralWithCall(request);

        response.Should().NotBeNull();
        response.ResponseStatus.Should().Be(StatusType.Valid);

        Referral updatedReferral = _context.Referrals
          .Include(r => r.Calls)
          .Single(r => r.Id == referral.Id);
        updatedReferral.Should().NotBeNull();
        updatedReferral.Status.Should()
         .Be(ReferralStatus.ChatBotCall1.ToString());
        updatedReferral.IsTelephoneValid.Should().Be(expectedIsTelephoneValid);
        updatedReferral.IsMobileValid.Should().Be(expectedIsMobileValid);
      }


      public static IEnumerable<object[]> NumberStatusOutcomeStatusData()
      {
        IEnumerable<object[]> data = new List<object[]>
        {
          CreateNsosObj("+441743000001", CallerReached),
          CreateNsosObj("+441743000002", TransferredToPhoneNumber),
          CreateNsosObj("+441743000003", TransferredToQueue),
          CreateNsosObj("+441743000004", TransferredToVoicemail),
          CreateNsosObj("+441743000005", VoicemailLeft),
          CreateNsosObj("+441743000006", Connected),
          CreateNsosObj("+441743000007", HungUp),
          CreateNsosObj("+441743000008", Engaged),
          CreateNsosObj("+441743000009", CallGuardian),
          CreateNsosObj("+441743000010", NoAnswer),
          CreateNsosObj("+441743000011", InvalidNumber),
          CreateNsosObj("+441743000012", Error),
        };

        return data;
      }

      public static object[] CreateNsosObj(
        string number,
        Enums.ChatBotCallOutcome outcome,
        Enums.ReferralStatus initialStatus = ChatBotCall1,
        Enums.ReferralStatus expectedStatus = ChatBotCall2)
      {
        return new object[]
        { number, initialStatus, outcome, expectedStatus };
      }

      private static UpdateReferralWithCallRequest Create(
        Guid id = default,
        string outcome = null,
        string number = null,
        DateTimeOffset timestamp = default)
      {
        return new UpdateReferralWithCallRequest(
          id: id == default
            ? ServiceFixture.ChatBotCall1Referral.Calls.Single().Id
            : id,
          outcome: outcome ?? CallerReached.ToString(),
          number: number ??
            ServiceFixture.ChatBotCall1Referral.Calls.Single().Number,
          timestamp: timestamp == default
            ? DateTimeOffset.Now
            : timestamp
        );
      }
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
        //Arrange
        string expectedMessage = "Value cannot be null. (Parameter 'request')";
        UpdateReferralWithCallRequest request = null;
        
        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunInstanceMethod(
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
      public async Task ArgumentNullException_Request_Null()
      {
        //Arrange
        string expectedMessage = "Value cannot be null. (Parameter 'request')";
        Entities.Referral referral = RandomEntityCreator.CreateRandomReferral();
        UpdateReferralWithCallRequest request = null;
        UpdateReferralWithCallResponse response = null;

        // act & assert
        var obj = TestPrivate_Helper.RunStaticMethod(
          typeof(ChatBotService), "ValidateReferralCanBeUpdated",
          new object[3] { referral, request, response });

        if (obj.Exception.InnerExceptions.Count > 0)
        {
          obj.Exception.InnerExceptions[0].InnerException.Should()
            .BeOfType(typeof(System.ArgumentNullException));
          obj.Exception.InnerExceptions[0].InnerException.Message.Should()
            .Be(expectedMessage);
        }
        else
        {
          Assert.False(true);
        }
      }

      [Fact]
      public async Task ArgumentNullException_Response_Null()
      {
        //Arrange
        string expectedMessage = "Value cannot be null. (Parameter 'response')";
        Entities.Referral referral = RandomEntityCreator.CreateRandomReferral();
        UpdateReferralWithCallRequest request = 
          new UpdateReferralWithCallRequest();
        UpdateReferralWithCallResponse response = null;

        // act & assert
        var obj = TestPrivate_Helper.RunStaticMethod(
          typeof(ChatBotService), "ValidateReferralCanBeUpdated",
          new object[3] { referral, request, response });

        if (obj.Exception.InnerExceptions.Count > 0)
        {
          obj.Exception.InnerExceptions[0].InnerException.Should()
            .BeOfType(typeof(System.ArgumentNullException));
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
        //Arrange
        string expectedMessage = "Value cannot be null. (Parameter 'referral')";
        Entities.Referral referral = null;
        UpdateReferralWithCallRequest request = null;

        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunInstanceMethod(
           typeof(ChatBotService), "UpdateReferral", _service,
           new object[2] { referral, request });
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
        //Arrange
        string expectedMessage = "Value cannot be null. (Parameter 'request')";
        Entities.Referral referral = RandomEntityCreator.CreateRandomReferral();
        UpdateReferralWithCallRequest request = null;

        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunInstanceMethod(
           typeof(ChatBotService), "UpdateReferral", _service,
           new object[2] { referral, request });
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
        //Arrange
        Guid requestId = Guid.NewGuid();
        string expectedMessage = $"Unable to find a referral that " +
          $"has a call id of {requestId}";
        Entities.Referral referral = 
          RandomEntityCreator.CreateRandomReferral();
        UpdateReferralWithCallRequest request = 
          new UpdateReferralWithCallRequest()
        {
          Id = requestId
        };

        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunInstanceMethod(
           typeof(ChatBotService), "UpdateReferral", _service,
           new object[2] { referral, request });
          await obj;
        }
        catch (ArgumentException ex)
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
        // arrange
        List<Entities.Referral> referrals = new();
        var referral1 = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New);
        var referral2 = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New);

        _context.Referrals.Add(referral1);
        _context.Referrals.Add(referral2);
        await _context.SaveChangesAsync();
        referrals.Add(referral1);
        referrals.Add(referral2);

        // act
        string response = await _service.RemoveReferrals(referrals);

        // assert
        response.Should().BeOfType<string>();
        response.Should().Be("");

        var refCheck1 = 
          _context.Referrals.SingleOrDefault(r => r.Id == referral1.Id);
        var refCheck2 = 
          _context.Referrals.SingleOrDefault(r => r.Id == referral2.Id);
        refCheck1.Should().BeNull();
        refCheck2.Should().BeNull();
      }

      [Fact]
      public async Task Removal_UnSuccessful_StatusReason()
      {
        // arrange
        List<Entities.Referral> referrals = new();
        var referral1 = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New);
        var referral2 = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New);

        _context.Referrals.Add(referral1);
        _context.Referrals.Add(referral2);
        await _context.SaveChangesAsync();

        referral1.StatusReason = "Invalid Reason";
        string response1 = 
          $"Fake entry using StatusReason " +
          $"{ referral1.StatusReason} is not loaded\r\n";
        referral2.StatusReason = "Another Invalid Reason";
        string expectedResponse = response1 + 
          $"Fake entry using StatusReason { referral2.StatusReason} " +
          $"is not loaded\r\n";
        
        referrals.Add(referral1);
        referrals.Add(referral2);

        // act
        string response = await _service.RemoveReferrals(referrals);

        // assert
        response.Should().BeOfType<string>();
        response.Should().Be(expectedResponse);

        var refCheck1 = 
          _context.Referrals.SingleOrDefault(r => r.Id == referral1.Id);
        var refCheck2 = 
          _context.Referrals.SingleOrDefault(r => r.Id == referral2.Id);
        refCheck1.Should().NotBeNull();
        refCheck2.Should().NotBeNull();
      }

      [Fact]
      public async Task Removal_UnSuccessful_Ubrn()
      {
        // arrange
        List<Entities.Referral> referrals = new();
        var referral1 = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New);

        _context.Referrals.Add(referral1);
        await _context.SaveChangesAsync();

        referral1.Ubrn = "9999999997";
        string expectedResponse = 
          $"Fake entry using UBRN {referral1.Ubrn} is not loaded\r\n";
        referrals.Add(referral1);

        // act
        string response = await _service.RemoveReferrals(referrals);

        // assert
        response.Should().BeOfType<string>();
        response.Should().Be(expectedResponse);

        var refCheck1 = 
          _context.Referrals.SingleOrDefault(r => r.Id == referral1.Id);
        refCheck1.Should().NotBeNull();
      }
    }

    public class AddReferrals : ChatBotServiceTests
    {
      private readonly Expression<Func<Entities.Referral, bool>>
        HasProvider = x => x.ProviderId != null;

      public AddReferrals(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Fact]
      public async Task ArgumentException_StatusReason()
      {
        // arrange
        List<Entities.Referral> model = new();
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New,
          statusReason: "TestingChatBot");
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();
        model.Add(referral);

        // act & assert
        ArgumentException exception =
          await Assert.ThrowsAsync<ArgumentException>(
          async () => await _service.AddReferrals(model));

        //clean up
        _context.Remove(referral);
        await _context.SaveChangesAsync();
      }

      [Fact]
      public async Task ArgumentException_Ubrn()
      {
        // arrange
        List<Entities.Referral> model = new();
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New,
          ubrn: "9191919191");
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();
        referral.StatusReason = "NonExistant Reason";
        model.Add(referral);

        // act & assert
        ArgumentException exception =
          await Assert.ThrowsAsync<ArgumentException>(
          async () => await _service.AddReferrals(model));

        //clean up
        _context.Remove(referral);
        await _context.SaveChangesAsync();
      }

      [Fact]
      public async Task ValidCreation()
      {
        // arrange
        bool expectedResult = true;
        List<Entities.Referral> model = new();
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New,
          ubrn: "9191919191",
          statusReason: "NonExistant Reason");
        model.Add(referral);

        // act 
        bool result = await _service.AddReferrals(model);

        // assert
        result.Should().Be(expectedResult);
        Entities.Referral refToRemove = 
          _context.Referrals
           .Where(r => r.StatusReason == referral.StatusReason)
           .SingleOrDefault();
        refToRemove.Should().NotBeNull();

        //clean up
        if (refToRemove != null)
          _context.Remove(refToRemove);
        await _context.SaveChangesAsync();
      }
    }

    public class GetReferralsWithCalls : ChatBotServiceTests
    {
      private readonly Expression<Func<Entities.Referral, bool>>
        HasProvider = x => x.ProviderId != null;

      public GetReferralsWithCalls(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Fact]
      public async Task ReturnNoList_InResponse()
      {
        // arrange
        int expectedCount = 0;
        var provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.ChatBotCall1);
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        var call1 = RandomEntityCreator.CreateRandomChatBotCall(
          isActive: true);
        call1.Sent = default;
        call1.ModifiedAt = DateTimeOffset.Now.AddHours(0);
        call1.ReferralId = referral.Id;
        _context.Calls.Add(call1);
        await _context.SaveChangesAsync();

        // act
        List<Entities.Referral> response =
          await _service.GetReferralsWithCalls(HasProvider);

        // assert
        response.Count.Should().Be(expectedCount);

        //clean up
        _context.Referrals.Remove(referral);
        _context.Providers.Remove(provider);
        _context.Calls.Remove(call1);
        await _context.SaveChangesAsync();
      }

      [Fact]
      public async Task ReturnList_InResponse()
      {
        // arrange
        int expectedCount = 1;
        var provider = RandomEntityCreator.CreateRandomProvider();
        _context.Providers.Add(provider);
        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.ChatBotCall1);
        referral.ProviderId = provider.Id;
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        var call1 = RandomEntityCreator.CreateRandomChatBotCall(
          isActive: true);
        call1.Sent = default;
        call1.ModifiedAt = DateTimeOffset.Now.AddHours(0);
        call1.ReferralId = referral.Id;
        _context.Calls.Add(call1);
        await _context.SaveChangesAsync();

        // act
        List<Entities.Referral> response =
          await _service.GetReferralsWithCalls(HasProvider);

        // assert
        response.Count.Should().Be(expectedCount);

        //clean up
        _context.Referrals.Remove(referral);
        _context.Providers.Remove(provider);
        _context.Calls.Remove(call1);
        await _context.SaveChangesAsync();
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
        // arrange
        Enums.StatusType expectedStatus = Enums.StatusType.Valid;
        var referral1 = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.ChatBotCall1);
        var call1 = RandomEntityCreator.CreateRandomChatBotCall(
          isActive: true,
          sent: default);
        call1.Sent = default;

        _context.Referrals.Add(referral1);
        await _context.SaveChangesAsync();

        call1.ReferralId = referral1.Id;
        _context.Calls.Add(call1);
        await _context.SaveChangesAsync();

        GetReferralCallListRequest request = new();

        // act
        GetReferralCallListResponse response =
          await _service.GetReferralCallList(request);

        // assert
        response.Should().BeOfType<GetReferralCallListResponse>();
        response.Status.Should().Be(expectedStatus);
        response.Arcus.Callees.Count().Should().Be(1);

        //clean up
        _context.Referrals.Remove(referral1);
        _context.Calls.Remove(call1);
        await _context.SaveChangesAsync();
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
        // arrange
        List<ICallee> model = null;

        // act & assert
        ArgumentNullException exception =
          await Assert.ThrowsAsync<ArgumentNullException>(
          async () => await _service.UpdateReferralCallListSent(model));
      }

      [Fact]
      public async Task UpdateCallees_Success()
      {
        // arrange
        List<ICallee> model = new();
        DateTimeOffset sentDate = DateTimeOffset.Now.LocalDateTime;

        Entities.Call call1 = RandomEntityCreator.CreateRandomChatBotCall();
        Entities.Call call2 = RandomEntityCreator.CreateRandomChatBotCall();
        call1.Sent = default;
        call2.Sent = default;

        _context.Calls.Add(call1);
        _context.Calls.Add(call2);
        await _context.SaveChangesAsync();

        Callee callee1 = new();
        callee1.Id = call1.Id.ToString();
        Callee callee2 = new();
        callee2.Id = call2.Id.ToString();
        model.Add(callee1);
        model.Add(callee2);

        // act
        await _service.UpdateReferralCallListSent(model);

        // assert
        call1.Sent.Should().NotBe(default);
        call2.Sent.Should().NotBe(default);
        call1.Sent.Should().BeOnOrAfter(sentDate);
        call2.Sent.Should().BeOnOrAfter(sentDate);

        //clean up
        _context.Calls.Remove(call1);
        _context.Calls.Remove(call2);
        await _context.SaveChangesAsync();
      }
    }

    public class UpdateNullNumbersAsync : ChatBotServiceTests
    {
      public UpdateNullNumbersAsync(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Fact]
      public async Task NoCallsHaveNullNumbers_ZeroChanges()
      {
        // arrange
        var call = RandomEntityCreator.CreateRandomChatBotCall();
        _context.Calls.Add(call);
        _context.SaveChanges();
        _context.Entry(call).State = EntityState.Detached;

        // act
        int numOfCallsUpdated =await _service.UpdateNullNumbersAsync();
        var callUpdated = _context.Calls.Single(c => c.Id == call.Id);

        // assert
        numOfCallsUpdated.Should().Be(0, because: "No numbers are null.");
        callUpdated.Should().BeEquivalentTo(call, 
          because: "The entity should not have been updated");

        // clean up
        _context.Calls.Remove(_context.Calls.Single(c => c.Id == call.Id));
        _context.SaveChanges();
      }

      [Fact]
      public async Task OneCallsHasNullNumbers_OutcomeAndSentUpdated()
      {
        // arrange
        var call = RandomEntityCreator.CreateRandomChatBotCall();
        call.Number = null;
        _context.Calls.Add(call);
        _context.SaveChanges();
        _context.Entry(call).State = EntityState.Detached;

        // act
        int numOfCallsUpdated = await _service.UpdateNullNumbersAsync();
        var callUpdated = _context.Calls.Single(c => c.Id == call.Id);

        // assert
        numOfCallsUpdated.Should().Be(1, because: "One number is null.");
        callUpdated.Should().BeEquivalentTo(call, options => options
          .Excluding(c => c.ModifiedAt)
          .Excluding(c => c.ModifiedByUserId)
          .Excluding(c => c.Outcome)
          .Excluding(c => c.Sent), 
          because: "The entity should be updated and modified.");

        callUpdated.ModifiedAt.Should().BeAfter(call.ModifiedAt);
        callUpdated.ModifiedByUserId.Should().Be(TEST_USER_ID);
        callUpdated.Outcome.Should().Be(InvalidNumber.ToString());
        callUpdated.Sent.Should().Be(new DateTime(1900,1,1));

        // clean up
        _context.Calls.Remove(_context.Calls.Single(c => c.Id == call.Id));
        _context.SaveChanges();
      }
    }


    public class PrepareCallsAsync : ChatBotServiceTests
    {
      public PrepareCallsAsync(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Fact]
      public async Task InvalidNumbersNotAddedToCallsTable()
      {
        // arrange
        var refInvalidMobileAndTel = RandomEntityCreator.CreateRandomReferral(
          status: TextMessage2, isMobileValid: false, isTelephoneValid: false);
        refInvalidMobileAndTel.Mobile = null;
        refInvalidMobileAndTel.Telephone = null;

        _context.Referrals.Add(refInvalidMobileAndTel);
        _context.SaveChanges();
        _context.Entry(refInvalidMobileAndTel).State = EntityState.Detached;

        // act
        PrepareCallsForTodayResponse response =
          await _service.PrepareCallsAsync();

        // assert
        response.CallsPrepared.Should().Be(0);
        var refCheck = _context.Referrals
          .Single(r => r.Id == refInvalidMobileAndTel.Id);
        refCheck.Should().BeEquivalentTo(refInvalidMobileAndTel, options => 
          options.Excluding(r => r.Status)
            .Excluding(r => r.ModifiedAt)
            .Excluding(r => r.ModifiedByUserId)
            .Excluding(r => r.Audits));
        refCheck.Status.Should().Be(ChatBotCall2.ToString(),
          because: "The status should be advanced to ChatBotCall2.");
        refCheck.ModifiedAt.Should().BeAfter(refInvalidMobileAndTel.ModifiedAt);
        refCheck.ModifiedByUserId.Should().Be(TEST_USER_ID);
        _context.Calls
          .Any(c => c.ReferralId == refInvalidMobileAndTel.Id)
          .Should().BeFalse(because: "The call record should not be created.");

        //clean up
        _context.Referrals.Remove(refCheck);
        _context.SaveChanges();
      }

      [Fact]
      public async Task Prepare_NoUpdate_Success()
      {
        // arrange
        int expectedCallsPrepared = 0;
        var referral1 = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage2);
        var referral2 = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.ChatBotCall1);

        var text1 = RandomEntityCreator.CreateRandomTextMessage(
          isActive: true,
          sent: DateTimeOffset.Now.AddDays(0));
        var call1 = RandomEntityCreator.CreateRandomChatBotCall(
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

        // act
        PrepareCallsForTodayResponse response =
          await _service.PrepareCallsAsync();

        // assert
        response.Should().BeOfType<PrepareCallsForTodayResponse>();
        response.CallsPrepared.Should().Be(expectedCallsPrepared);
        var refCheck1 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral1.Id);
        var refCheck2 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral2.Id);
        refCheck1.Should().NotBeNull();
        refCheck2.Should().NotBeNull();
        refCheck1.Status.Should().Be(ReferralStatus.TextMessage2.ToString());
        refCheck2.Status.Should().Be(ReferralStatus.ChatBotCall1.ToString());

        //clean up
        _context.Referrals.Remove(referral1);
        _context.Referrals.Remove(referral2);
        _context.Calls.Remove(call1);
        _context.TextMessages.Remove(text1);
        await _context.SaveChangesAsync();
      }

      [Fact]
      public async Task Prepare_Update_Success()
      {
        // arrange
        int expectedCallsPrepared = 2;
        var referral1 = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.TextMessage2);
        var referral2 = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.ChatBotCall1);

        var text1 = RandomEntityCreator.CreateRandomTextMessage(
          isActive: true,
          sent: DateTimeOffset.Now.AddDays(-4));
        var call1 = RandomEntityCreator.CreateRandomChatBotCall(
          isActive: true,
          sent: DateTimeOffset.Now.AddDays(-4));

        _context.Referrals.Add(referral1);
        _context.Referrals.Add(referral2);
        await _context.SaveChangesAsync();
        _context.Entry(referral1).State = EntityState.Detached;
        _context.Entry(referral2).State = EntityState.Detached;

        text1.ReferralId = referral1.Id;
        call1.ReferralId = referral2.Id;
        _context.TextMessages.Add(text1);
        _context.Calls.Add(call1);
        await _context.SaveChangesAsync();

        // act
        PrepareCallsForTodayResponse response =
          await _service.PrepareCallsAsync();

        // assert
        response.Should().BeOfType<PrepareCallsForTodayResponse>();
        response.CallsPrepared.Should().Be(expectedCallsPrepared);
        var refCheck1 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral1.Id);
        var refCheck2 =
          _context.Referrals.SingleOrDefault(r => r.Id == referral2.Id);
        refCheck1.Should().BeEquivalentTo(referral1, options => options
          .Excluding(r => r.Status)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.Audits)
          .Excluding(r => r.Calls)
          .Excluding(r => r.TextMessages));
        refCheck2.Should().BeEquivalentTo(referral2, options => options
          .Excluding(r => r.Status)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.Audits)
          .Excluding(r => r.Calls)
          .Excluding(r => r.TextMessages));
        refCheck1.Should().NotBeNull();
        refCheck2.Should().NotBeNull();
        refCheck1.Status.Should().Be(ReferralStatus.ChatBotCall1.ToString());
        refCheck2.Status.Should().Be(ReferralStatus.ChatBotCall2.ToString());
        refCheck1.ModifiedAt.Should().BeAfter(referral1.ModifiedAt);
        refCheck2.ModifiedAt.Should().BeAfter(referral2.ModifiedAt);
        refCheck1.ModifiedByUserId.Should().Be(TEST_USER_ID);
        refCheck2.ModifiedByUserId.Should().Be(TEST_USER_ID);

        //clean up
        _context.Referrals.Remove(refCheck1);
        _context.Referrals.Remove(refCheck2);
        _context.Calls.Remove(call1);
        _context.TextMessages.Remove(text1);
        await _context.SaveChangesAsync();
      }
    }

    public class UpdateReferralStatusForCallOutcomeAsync : ChatBotServiceTests
    {
      public UpdateReferralStatusForCallOutcomeAsync(
        ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Fact]
      public async Task UpdateReferralStatusForCallOutcomeAsync_UpdateSuccess()
      {
        Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
        Entities.Call requestedCall =
          RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
        string outcome = ChatBotCallOutcome.CallerReached.ToString();

        var obj = TestPrivate_Helper.RunStaticMethod(
         typeof(ChatBotService), "UpdateReferralStatusForCallOutcomeAsync",
         new object[3] { referral, requestedCall, outcome });

        referral.IsTelephoneValid.Should().BeTrue();
      }

      [Fact]
      public async Task Exception_Null_Referral()
      {
        //Arrange
        string expectedMessage = "Value cannot be null.";
        Entities.Referral referral = null;
        Entities.Call requestedCall =
          RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
        string outcome = ChatBotCallOutcome.CallerReached.ToString();

        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunStaticMethod(
           typeof(ChatBotService), "UpdateReferralStatusForCallOutcomeAsync",
           new object[3] { referral, requestedCall, outcome });
          await obj;
        }
        catch (System.ArgumentNullException ex)
        {
          Assert.Contains(expectedMessage, ex.Message);
        }
      }

      [Fact]
      public async Task Exception_Null_RequestedCall()
      {
        //Arrange
        string expectedMessage = "Value cannot be null.";
        Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
        Entities.Call requestedCall = null;
        string outcome = ChatBotCallOutcome.CallerReached.ToString();

        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunStaticMethod(
           typeof(ChatBotService), "UpdateReferralStatusForCallOutcomeAsync",
           new object[3] { referral, requestedCall, outcome });
          await obj;
        }
        catch (System.ArgumentNullException ex)
        {
          Assert.Contains(expectedMessage, ex.Message);
        }
      }
      [Fact]
      public async Task Exception_Null_Outcome()
      {
        //Arrange
        string expectedMessage = "Value cannot be null or white space";
        Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
        Entities.Call requestedCall =
          RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
        string outcome = "";

        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunStaticMethod(
           typeof(ChatBotService), "UpdateReferralStatusForCallOutcomeAsync",
           new object[3] { referral, requestedCall, outcome });
          await obj;
        }
        catch (ArgumentNullOrWhiteSpaceException ex)
        {
          Assert.Contains(expectedMessage, ex.Message);
        }
      }

      [Fact]
      public async Task Exception_Outcome_Exception()
      {
        //Arrange
        string expectedMessage = "Unknown outcome of";
        Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
        Entities.Call requestedCall =
          RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
        string outcome = "InvalidOutcome";

        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunStaticMethod(
           typeof(ChatBotService), "UpdateReferralStatusForCallOutcomeAsync",
           new object[3] { referral, requestedCall, outcome });
          await obj;
        }
        catch (ArgumentException ex)
        {
          Assert.Contains(expectedMessage, ex.Message);
        }
      }

      [Fact]
      public async Task Exception_Null_ReferralCalls()
      {
        //Arrange
        string expectedMessage = "Calls is null";
        Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
        referral.Calls = null;
        Entities.Call requestedCall =
        RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
        string outcome = ChatBotCallOutcome.CallerReached.ToString();
        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunStaticMethod(
           typeof(ChatBotService), "UpdateReferralStatusForCallOutcomeAsync",
           new object[3] { referral, requestedCall, outcome });
          await obj;
        }
        catch (ArgumentNullException ex)
        {
          Assert.Contains(expectedMessage, ex.Message);
        }
      }

      [Fact]
      public async Task Exception_RequestCall_Number_Mismatch()
      {
        //Arrange
        string reqNumber = "77865454347";
        string expectedMessage = $"Number {reqNumber} not found in referral";
        Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(telephone: "1234567890");
        Entities.Call requestedCall =
        RandomEntityCreator.CreateRandomChatBotCall(number: reqNumber);
        string outcome = ChatBotCallOutcome.CallerReached.ToString();
        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunStaticMethod(
           typeof(ChatBotService), "UpdateReferralStatusForCallOutcomeAsync",
           new object[3] { referral, requestedCall, outcome });
          await obj;
        }
        catch (ChatBotNumberNotFoundException ex)
        {
          Assert.Contains(expectedMessage, ex.Message);
        }
      }

      [Fact]
      public async Task Exception_ReferralInvalidStatusException()
      {
        //Arrange
        string expectedMessage = $"Expected a status of ChatBotCall1 or " +
          "ChatBotCall2 but found CancelledByEreferrals";
        Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(
            telephone: "1234567890",
            status: ReferralStatus.CancelledByEreferrals);
        Entities.Call requestedCall =
        RandomEntityCreator.CreateRandomChatBotCall(number: "1234567890");
        string outcome = ChatBotCallOutcome.TransferringToRmc.ToString();
        try
        {
          // act & assert
          var obj = TestPrivate_Helper.RunStaticMethod(
           typeof(ChatBotService), "UpdateReferralStatusForCallOutcomeAsync",
           new object[3] { referral, requestedCall, outcome });
          await obj;
        }
        catch (ReferralInvalidStatusException ex)
        {
          Assert.Contains(expectedMessage, ex.Message);
        }
      }
    }



    public class UpdateReferralTransferRequestAsync : ChatBotServiceTests
    {
      public UpdateReferralTransferRequestAsync(
        ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Fact]
      public async Task ArgumentNullException()
      {
        // arrange
        UpdateReferralTransferRequest request = null;

        // act & assert
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

        UpdateReferralTransferRequest request =
          new UpdateReferralTransferRequest();
        request.Number = telephone;
        request.Outcome = outcome;
        request.Timestamp = DateTimeOffset.Now;

        Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(telephone: telephone);

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        // act
        UpdateReferralTransferResponse response =
          await _service.UpdateReferralTransferRequestAsync(request);

        // assert
        response.Should().BeOfType<UpdateReferralTransferResponse>();
        response.ResponseStatus.Should().Be(expectedStatus);

        //clean up
        _context.Referrals.Remove(referral);
        await _context.SaveChangesAsync();
      }

      [Fact]
      public async Task UpdateReferralTransferRequestAsync_TelephoneInValid()
      {
        string telephone = "1234567890";
        string outcome = ChatBotCallOutcome.TransferringToRmc.ToString();
        StatusType expectedStatus = StatusType.Invalid;
        string expectedErrorMessage = "The field Number is not a valid " +
          "telephone number.";
        UpdateReferralTransferRequest request =
          new UpdateReferralTransferRequest();
        request.Number = telephone;
        request.Outcome = outcome;
        request.Timestamp = DateTimeOffset.Now;

        Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(telephone: telephone);

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        // act
        UpdateReferralTransferResponse response =
          await _service.UpdateReferralTransferRequestAsync(request);

        // assert
        response.Should().BeOfType<UpdateReferralTransferResponse>();
        response.ResponseStatus.Should().Be(expectedStatus);
        response.Errors[0].Should().Be(expectedErrorMessage);

        //clean up
        _context.Referrals.Remove(referral);
        await _context.SaveChangesAsync();
      }

      [Fact]
      public async Task UpdateReferralWithTransferRequestAsync_UpdateSuccess()
      {
        string expectedRefStatus = ReferralStatus.ChatBotTransfer.ToString();
        string telephone = "+1234567890";
        string outcome = ChatBotCallOutcome.TransferringToRmc.ToString();
        StatusType expectedStatus = StatusType.Valid;

        UpdateReferralTransferRequest request =
          new UpdateReferralTransferRequest();
        request.Number = telephone;
        request.Outcome = outcome;
        request.Timestamp = DateTimeOffset.Now;

        Entities.Referral referral =
          RandomEntityCreator.CreateRandomReferral(telephone: telephone);
        referral.Status = ReferralStatus.ChatBotCall1.ToString();

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        // act
        UpdateReferralTransferResponse response =
          await _service.UpdateReferralTransferRequestAsync(request);

        // assert
        response.Should().BeOfType<UpdateReferralTransferResponse>();
        response.ResponseStatus.Should().Be(expectedStatus);
        referral.Status.Should().Be(expectedRefStatus);

        //clean up
        _context.Referrals.Remove(referral);
        await _context.SaveChangesAsync();
      }

      [Fact]
      public async Task UpdateReferralWithTransferRequestAsync_NonDistinct()
      {
        string telephone = "+1234567890";
        string outcome = ChatBotCallOutcome.TransferringToRmc.ToString();
        StatusType expectedStatus = StatusType.Invalid;

        UpdateReferralTransferRequest request =
          new UpdateReferralTransferRequest();
        request.Number = telephone;
        request.Outcome = outcome;
        request.Timestamp = DateTimeOffset.Now;

        Entities.Referral referral1 =
          RandomEntityCreator.CreateRandomReferral(telephone: telephone);
        referral1.Status = ReferralStatus.ChatBotCall1.ToString();
        Entities.Referral referral2 =
          RandomEntityCreator.CreateRandomReferral(mobile: telephone);
        referral2.Status = ReferralStatus.ChatBotCall2.ToString();

        _context.Referrals.Add(referral1);
        _context.Referrals.Add(referral2);
        await _context.SaveChangesAsync();

        // act
        UpdateReferralTransferResponse response =
          await _service.UpdateReferralTransferRequestAsync(request);

        // assert
        response.Should().BeOfType<UpdateReferralTransferResponse>();
        response.ResponseStatus.Should().Be(expectedStatus);

        //clean up
        _context.Referrals.Remove(referral1);
        _context.Referrals.Remove(referral2);
        await _context.SaveChangesAsync();
      }
    }

    class Tel
    {
      public const bool IsValid_False = false;
      public const bool IsValid_True = true;
    }

    class Mob
    {
      public const bool IsValid_False = false;
      public const bool IsValid_True = true;
    }
  }

  public class TestPrivate_Helper
  {
    public async static Task RunStaticMethod(
      System.Type t,
      string strMethod,
      object[] aobjParams)
    {
      try
      {
        BindingFlags eFlags = BindingFlags.Static | BindingFlags.NonPublic;
        await RunMethod(t, strMethod, null, aobjParams, eFlags);
      }
      catch
      {
        throw;
      }
    }

    public async static Task RunInstanceMethod(
      System.Type t, 
      string strMethod,
      object objInstance, 
      object[] aobjParams)
    {
      try
      {
        BindingFlags eFlags = BindingFlags.Instance | BindingFlags.NonPublic;
      await RunMethod(t, strMethod, objInstance, aobjParams, eFlags);
      }
      catch
      {
        throw;
      }
    }

    private async static Task RunMethod(
      System.Type t,
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
}