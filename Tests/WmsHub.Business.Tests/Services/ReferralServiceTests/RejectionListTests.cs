//using System;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Serilog;
//using WmsHub.Business.Enums;
//using WmsHub.Business.Models.ProviderRejection;
//using Xunit;
//using Xunit.Abstractions;

//namespace WmsHub.Business.Tests.Services;

//public partial class ReferralServiceTests : ServiceTestsBase
//{
//  public class RejectionListTests : ReferralServiceTests, IDisposable
//  {
//    public void Dispose()
//    {
//      ResetContext();
//    }

//    public RejectionListTests(
//      ServiceFixture serviceFixture,
//      ITestOutputHelper testOutputHelper) 
//      : base(serviceFixture, testOutputHelper)
//    {
//      Log.Logger = new LoggerConfiguration()
//        .WriteTo.TestOutput(testOutputHelper)
//        .CreateLogger();
//    }

//    [Fact]
//    public async Task ValidList()
//    {
//      //arrange
//      Entities.ProviderRejectionReason item = new() 
//      {
//        Group = (int)ProviderRejectionReasonGroup.None,
//        Description = "Test Description",
//        Id = Guid.NewGuid(),
//        IsActive = true,
//        IsRmcReason = true,
//        ModifiedAt = DateTimeOffset.Now,
//        ModifiedByUserId = Guid.NewGuid()
//      };

//      _context.ProviderRejectionReasons.Add(item);
//      await _context.SaveChangesAsync();
//      //act
//      try
//      {
//        ProviderRejectionReason[] list = await _service.GetRejectionList();
//        list.Should().NotBeEmpty();
//      }
//      catch (Exception ex)
//      {
//        Assert.True(false,ex.Message);
//      }
//      finally
//      {
//        ResetContext();
//      }


//    }

//    [Fact]
//    public async Task ValidList_Only_Active()
//    {
//      //arrange
//      ResetContext();
//      Entities.ProviderRejectionReason item1 = new()
//      {
//        Group = (int)ProviderRejectionReasonGroup.None,
//        Description = "Test Description1",
//        Title = "Test 1",
//        Id = Guid.NewGuid(),
//        IsActive = true,
//        IsRmcReason = true,
//        ModifiedAt = DateTimeOffset.Now,
//        ModifiedByUserId = Guid.NewGuid()
//      };

//      Entities.ProviderRejectionReason item2 = new()
//      {
//        Group = (int)ProviderRejectionReasonGroup.ProviderRejected,
//        Description = "Test Description2",
//        Title = "Test 2",
//        Id = Guid.NewGuid(),
//        IsActive = false,
//        IsRmcReason = true,
//        ModifiedAt = DateTimeOffset.Now,
//        ModifiedByUserId = Guid.NewGuid()
//      };

//      _context.ProviderRejectionReasons.Add(item1);
//      _context.ProviderRejectionReasons.Add(item2);
//      await _context.SaveChangesAsync();

//      //act
//      try
//      {
//        ProviderRejectionReason[] list = await _service.GetRejectionList();
//        list.Should().NotBeEmpty();
//        list.Length.Should().Be(1);
//        list[0].Title.Should().Be("Test 1");
//      }
//      catch (Exception ex)
//      {
//        Assert.True(false, ex.Message);
//      }
//      finally
//      {
//        ResetContext();
//      }
//    }

//    [Fact]
//    public async Task ValidList_Only_RmcReasons()
//    {
//      //arrange
//      ResetContext();
//      Entities.ProviderRejectionReason item1 = new()
//      {
//        Group = (int)ProviderRejectionReasonGroup.None,
//        Description = "Test Description1",
//        Title = "Test 1",
//        Id = Guid.NewGuid(),
//        IsActive = true,
//        IsRmcReason = true,
//        ModifiedAt = DateTimeOffset.Now,
//        ModifiedByUserId = Guid.NewGuid()
//      };

//      Entities.ProviderRejectionReason item2 = new()
//      {
//        Group = (int)ProviderRejectionReasonGroup.ProviderRejected,
//        Description = "Test Description2",
//        Title = "Test 2",
//        Id = Guid.NewGuid(),
//        IsActive = true,
//        IsRmcReason = false,
//        ModifiedAt = DateTimeOffset.Now,
//        ModifiedByUserId = Guid.NewGuid()
//      };

//      _context.ProviderRejectionReasons.Add(item1);
//      _context.ProviderRejectionReasons.Add(item2);
//      await _context.SaveChangesAsync();
//      //act
//      try
//      {
//        ProviderRejectionReason[] list = await _service.GetRejectionList();
//        list.Should().NotBeEmpty();
//        list.Length.Should().Be(1);
//        list[0].Title.Should().Be("Test 1");
//      }
//      catch (Exception ex)
//      {
//        Assert.True(false, ex.Message);
//      }
//      finally
//      {
//        ResetContext();
//      }
//    }

//    [Fact]
//    public async Task ValidList_No_Active_EmptyList()
//    {
//      //arrange
//      Entities.ProviderRejectionReason item1 = new()
//      {
//        Group = (int)ProviderRejectionReasonGroup.ServiceUserDeclined,
//        Description = "Test Description1",
//        Id = Guid.NewGuid(),
//        IsActive = false,
//        IsRmcReason = true,
//        ModifiedAt = DateTimeOffset.Now,
//        ModifiedByUserId = Guid.NewGuid()
//      };

//      Entities.ProviderRejectionReason item2 = new()
//      {
//        Group = (int)ProviderRejectionReasonGroup.ProviderRejected,
//        Description = "Test Description2",
//        Id = Guid.NewGuid(),
//        IsActive = false,
//        IsRmcReason = true,
//        ModifiedAt = DateTimeOffset.Now,
//        ModifiedByUserId = Guid.NewGuid()
//      };

//      _context.ProviderRejectionReasons.Add(item1);
//      _context.ProviderRejectionReasons.Add(item2);
//      await _context.SaveChangesAsync();

//      //act
//      try
//      {
//        ProviderRejectionReason[] list = await _service.GetRejectionList();
//        list.Should().BeEmpty();
//      }
//      catch (Exception ex)
//      {
//        Assert.True(false, ex.Message);
//      }
//      finally
//      {
//        ResetContext();
//      }
//    }

//    [Fact]
//    public async Task ValidList_No_RmcReasons_EmptyList()
//    {
//      //arrange
//      Entities.ProviderRejectionReason item1 = new()
//      {
//        Group = (int)ProviderRejectionReasonGroup.ServiceUserDeclined,
//        Description = "Test Description1",
//        Id = Guid.NewGuid(),
//        IsActive = true,
//        IsRmcReason = false,
//        ModifiedAt = DateTimeOffset.Now,
//        ModifiedByUserId = Guid.NewGuid()
//      };

//      Entities.ProviderRejectionReason item2 = new()
//      {
//        Group = (int)ProviderRejectionReasonGroup.ProviderRejected,
//        Description = "Test Description2",
//        Id = Guid.NewGuid(),
//        IsActive = true,
//        IsRmcReason = false,
//        ModifiedAt = DateTimeOffset.Now,
//        ModifiedByUserId = Guid.NewGuid()
//      };

//      _context.ProviderRejectionReasons.Add(item1);
//      _context.ProviderRejectionReasons.Add(item2);
//      await _context.SaveChangesAsync();
//      //act
//      try
//      {
//        ProviderRejectionReason[] list = await _service.GetRejectionList();
//        list.Should().BeEmpty();
//      }
//      catch (Exception ex)
//      {
//        Assert.True(false, ex.Message);
//      }
//      finally
//      {
//        ResetContext();
//      }


//    }

//    private void ResetContext()
//    {
//      _context.ProviderRejectionReasons.RemoveRange(
//        _context.ProviderRejectionReasons);
//      _context.SaveChanges();
//    }
//  }
//}