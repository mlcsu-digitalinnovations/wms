using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Exceptions;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  [Collection("Service collection")]
  public class PracticeServiceTests : ServiceTestsBase, IDisposable
  {
    protected readonly DatabaseContext _context;
    protected readonly PracticeService _service;

    public PracticeServiceTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _context = new DatabaseContext(_serviceFixture.Options);
      _service = new PracticeService(
        _context,
        _serviceFixture.Mapper)
      {
        User = GetClaimsPrincipal()
      };

      CleanUp();
    }

    protected Entities.Practice CreateRandomPracticeInDatabase(
      bool isActive = true, string modifiedByUserId = TEST_USER_ID)
    {
      Entities.Practice practice = RandomEntityCreator.CreateRandomPractice(
        isActive: isActive,
        modifiedByUserId: Guid.Parse(modifiedByUserId));

      _context.Add(practice);
      _context.SaveChanges();
      return practice;
    }

    public void Dispose()
    {
      CleanUp();
    }

    private void CleanUp()
    {
      // clean up - remove all practices
      _context.Practices.RemoveRange(_context.Practices);
      _context.SaveChanges();
      _context.Practices.Count().Should().Be(0);
    }

    public class PracticeServiceConstructor : PracticeServiceTests
    {
      public PracticeServiceConstructor(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public void PracticeServiceInstantiate()
      {
        // assert
        _service.Should().NotBeNull();
      }
    }

    public class CreateAsync : PracticeServiceTests
    {
      public CreateAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ArgumentNullException()
      {
        // assert
        await Assert.ThrowsAsync<ArgumentNullException>(
          async () => await _service.CreateAsync(null));
      }

      [Fact]
      public async Task PracticeExistsException()
      {
        // arrange
        Entities.Practice practiceEntity = CreateRandomPracticeInDatabase();

        Practice practiceModel = RandomModelCreator.CreateRandomPractice(
          odsCode: practiceEntity.OdsCode);

        // act
        await Assert.ThrowsAsync<PracticeExistsException>(
          async () => await _service.CreateAsync(practiceModel));
      }

      [Theory]
      // invalid ods code
      [InlineData("", "a@a.com", "name", "emis")]
      [InlineData(null, "a@a.com", "name", "Emis")]
      // invalid email address when provided
      [InlineData("M12345", "", "name", "systmone")]
      [InlineData("M12345", "d.com", "name", "vision")]
      [InlineData("M12345", "@d.com", "name", "Vision")]
      [InlineData("M12345", "a.a.com", "name", "EMIS")]
      // invalid system name
      [InlineData("M12345", "a@a.com", "name", "")]
      [InlineData("M12345", "a@a.com", "name", null)]
      [InlineData("M12345", "a@a.com", "name", "e mis")]
      [InlineData("M12345", "a@a.com", "name", "S1")]
      [InlineData("M12345", "a@a.com", "name", "vis1on")]
      public async Task PracticeInvalidException(
        string odsCode, string email, string name, string systemName)
      {
        // arrange
        Practice practiceModel = new Practice
        {
          OdsCode = odsCode,
          Email = email,
          Name = name,
          SystemName = systemName,
        };

        // act
        await Assert.ThrowsAsync<PracticeInvalidException>(
          async () => await _service.CreateAsync(practiceModel));
      }

      [Theory]
      // all properties
      [InlineData("M12345", "a@a.com", "name", "Emis")]
      [InlineData("M23456", "a@a.com", "name", "SystmOne")]
      [InlineData("M34567", "a@a.com", "name", "Vision")]
      // missing email
      [InlineData("M23456", null, "name", "SystmOne")]
      // missing name
      [InlineData("M12345", "a@a.com", "", "Emis")]
      [InlineData("M23456", "a@a.com", null, "SystmOne")]
      // missing email and name
      [InlineData("M23456", null, "", "SystmOne")]
      [InlineData("M23456", null, null, "SystmOne")]
      public async Task Successful(
        string odsCode, string email, string name, string systemName)
      {
        // arrange
        IPractice expected = new Practice
        {
          OdsCode = odsCode,
          Email = email,
          Name = name,
          SystemName = systemName,
          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
          IsActive = true
        };

        // act
        IPractice result = await _service.CreateAsync(expected);

        // assert        
        result.Should().BeOfType<Practice>();
        result.Should().BeEquivalentTo(expected, option => option
          .Excluding(p => p.Id)
          .Excluding(p => p.ModifiedAt));
        result.Id.Should().NotBe(Guid.Empty);
        result.ModifiedAt.Should().BeBefore(DateTimeOffset.Now);

        Entities.Practice actual = _context.Practices.FirstOrDefault();
        actual.Should().BeEquivalentTo(result);
      }

      [Fact]
      public async Task SuccessfulWithExistingDeactivatedPractice()
      {
        // arrange
        Entities.Practice deactivatedPractice = CreateRandomPracticeInDatabase(
          isActive: false);

        IPractice expected = new Practice
        {
          Id = deactivatedPractice.Id,
          OdsCode = deactivatedPractice.OdsCode,
          Email = deactivatedPractice.Email,
          Name = deactivatedPractice.Name,
          SystemName = deactivatedPractice.SystemName,
          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
          IsActive = true
        };

        // act
        IPractice result = await _service.CreateAsync(expected);

        // assert
        result.Should().BeOfType<Practice>();
        result.Should().BeEquivalentTo(expected, option => option
          .Excluding(p => p.ModifiedAt));
        result.Id.Should().NotBe(Guid.Empty);
        result.ModifiedAt.Should().BeBefore(DateTimeOffset.Now);

        Entities.Practice actual = _context.Practices.FirstOrDefault();
        actual.Should().BeEquivalentTo(result);
      }
    }

    public class GetAsync : PracticeServiceTests
    {
      public GetAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task WhereAllPracticesAreActive()
      {
        // arrange
        var expected1 = CreateRandomPracticeInDatabase();
        var expected2 = CreateRandomPracticeInDatabase();

        // act
        IEnumerable<IPractice> practices = await _service.GetAsync();

        practices.Count().Should().Be(2);
        var practice1 = practices.Single(p => p.OdsCode == expected1.OdsCode);
        practice1.Should().BeEquivalentTo(expected1);
        var practice2 = practices.Single(p => p.OdsCode == expected2.OdsCode);
        practice2.Should().BeEquivalentTo(expected2);
      }

      [Fact]
      public async Task WhereNotAllPracticesAreActive()
      {
        // arrange
        var expected = CreateRandomPracticeInDatabase();
        CreateRandomPracticeInDatabase(isActive: false);

        // act
        IEnumerable<IPractice> practices = await _service.GetAsync();

        practices.Count().Should().Be(1);
        var practice1 = practices.Single(p => p.OdsCode == expected.OdsCode);
        practice1.Should().BeEquivalentTo(expected);
      }

    }

    public class GetByObdCodeAsync : PracticeServiceTests
    {
      public GetByObdCodeAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ArgumentNullOrWhiteSpaceException()
      {
        // assert
        await Assert.ThrowsAsync<ArgumentNullOrWhiteSpaceException>(
          async () => await _service.GetByObsCodeAsync(null));
      }

      [Fact]
      public async Task OdsCodeExists()
      {
        // arrange
        Entities.Practice entity = RandomEntityCreator.CreateRandomPractice(
          modifiedByUserId: Guid.Parse(TEST_USER_ID));
        _context.Add(entity);
        _context.SaveChanges();

        Practice expected = _serviceFixture.Mapper.Map<Practice>(entity);

        // act
        Practice actual = await _service.GetByObsCodeAsync(entity.OdsCode);

        // assert
        actual.Should().BeOfType<Practice>();
        actual.Should().BeEquivalentTo(expected, option => option
          .Excluding(p => p.Id)
          .Excluding(p => p.ModifiedAt));
        actual.Id.Should().NotBe(Guid.Empty);
        actual.ModifiedAt.Should().BeBefore(DateTimeOffset.Now);
      }

      [Fact]
      public async Task OdsCodeNotFound()
      {
        // arrange
        CreateRandomPracticeInDatabase();

        // act
        Practice actual = await _service
          .GetByObsCodeAsync("NotAPracticeOdsCode");

        // assert
        actual.Should().BeNull();
      }
    }

    public class UpdateAsync : PracticeServiceTests
    {
      public UpdateAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task ArgumentNullException()
      {
        // assert
        await Assert.ThrowsAsync<ArgumentNullException>(
          async () => await _service.UpdateAsync(null));
      }

      [Fact]
      public async Task PracticeNotFoundException()
      {
        // arrange
        Practice practiceModel = RandomModelCreator.CreateRandomPractice();

        // act
        await Assert.ThrowsAsync<PracticeNotFoundException>(
          async () => await _service.UpdateAsync(practiceModel));
      }

      [Theory]
      // invalid ods code
      [InlineData("", "a@a.com", "name", "emis")]
      [InlineData(null, "a@a.com", "name", "Emis")]
      // invalid email address when provided
      [InlineData("M12345", "", "name", "systmone")]
      [InlineData("M12345", "d.com", "name", "vision")]
      [InlineData("M12345", "@d.com", "name", "Vision")]
      [InlineData("M12345", "a.a.com", "name", "EMIS")]
      // invalid system name
      [InlineData("M12345", "a@a.com", "name", "")]
      [InlineData("M12345", "a@a.com", "name", null)]
      [InlineData("M12345", "a@a.com", "name", "e mis")]
      [InlineData("M12345", "a@a.com", "name", "S1")]
      [InlineData("M12345", "a@a.com", "name", "vis1on")]
      public async Task PracticeInvalidException(
        string odsCode, string email, string name, string systemName)
      {
        // arrange
        Practice practiceModel = new()
        {
          OdsCode = odsCode,
          Email = email,
          Name = name,
          SystemName = systemName,
        };

        // act
        await Assert.ThrowsAsync<PracticeInvalidException>(
          async () => await _service.UpdateAsync(practiceModel));
      }

      [Theory]
      // all properties
      [InlineData("a@a.com", "name", "Emis")]
      [InlineData("a@a.com", "name", "SystmOne")]
      [InlineData("a@a.com", "name", "Vision")]
      // missing email
      [InlineData(null, "name", "SystmOne")]
      // missing name
      [InlineData("a@a.com", "", "Emis")]
      [InlineData("a@a.com", null, "SystmOne")]
      // missing email and name
      [InlineData(null, "", "SystmOne")]
      [InlineData(null, null, "SystmOne")]
      public async Task Successful(
        string email, string name, string systemName)
      {
        // arrange
        Entities.Practice entity = CreateRandomPracticeInDatabase();

        Practice expected = new()
        {
          Id = entity.Id,
          OdsCode = entity.OdsCode,
          Email = email,
          Name = name,
          SystemName = systemName,
          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
          IsActive = true
        };

        // act
        IPractice result = await _service.UpdateAsync(expected);

        // assert
        result.Should().BeOfType<Practice>();
        result.Should().BeEquivalentTo(expected, option => option
          .Excluding(p => p.Id)
          .Excluding(p => p.ModifiedAt));
        result.Id.Should().NotBe(Guid.Empty);
        result.ModifiedAt.Should().BeBefore(DateTimeOffset.Now);

        Entities.Practice actual = _context.Practices.FirstOrDefault();
        result.Should().BeEquivalentTo(result);
      }

      [Fact]
      public async Task SuccessfulWithExistingDeactivatedPractice()
      {
        // arrange
        Entities.Practice deactivatedPractice = CreateRandomPracticeInDatabase(
          isActive: false);

        IPractice expected = new Practice
        {
          Id = deactivatedPractice.Id,
          OdsCode = deactivatedPractice.OdsCode,
          Email = deactivatedPractice.Email,
          Name = deactivatedPractice.Name,
          SystemName = deactivatedPractice.SystemName,
          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
          IsActive = true
        };

        // act
        IPractice result = await _service.UpdateAsync(expected);

        // assert
        result.Should().BeOfType<Practice>();
        result.Should().BeEquivalentTo(expected, option => option
          .Excluding(p => p.ModifiedAt));
        result.Id.Should().NotBe(Guid.Empty);
        result.ModifiedAt.Should().BeBefore(DateTimeOffset.Now);

        Entities.Practice actual = _context.Practices.FirstOrDefault();
        actual.Should().BeEquivalentTo(result);
      }
    }
  }
}
