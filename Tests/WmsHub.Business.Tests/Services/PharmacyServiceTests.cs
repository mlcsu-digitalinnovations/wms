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
  public class PharmacyServiceTests : ServiceTestsBase, IDisposable
  {
    protected readonly DatabaseContext _context;
    protected readonly PharmacyService _service;

    public PharmacyServiceTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _context = new DatabaseContext(_serviceFixture.Options);
      _service = new PharmacyService(
        _context,
        _serviceFixture.Mapper)
      {
        User = GetClaimsPrincipal()
      };

      CleanUp();
    }

    protected Entities.Pharmacy CreateRandomPharmacyInDatabase(
      bool isActive = true, string modifiedByUserId = TEST_USER_ID)
    {
      Entities.Pharmacy practice = RandomEntityCreator.CreateRandomPharmacy(
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
      _context.Pharmacies.RemoveRange(_context.Pharmacies);
      _context.SaveChanges();
      _context.Pharmacies.Count().Should().Be(0);
    }

    public class PharmacyServiceConstructor : PharmacyServiceTests
    {
      public PharmacyServiceConstructor(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public void PharmacyServiceInstantiate()
      {
        // assert
        _service.Should().NotBeNull();
      }
    }

    public class CreateAsync : PharmacyServiceTests
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
      public async Task PharmacyExistsException()
      {
        // arrange
        Entities.Pharmacy practiceEntity = CreateRandomPharmacyInDatabase();

        Pharmacy practiceModel = RandomModelCreator.CreateRandomPharmacy(
          odsCode: practiceEntity.OdsCode);

        // act
        await Assert.ThrowsAsync<PharmacyExistsException>(
          async () => await _service.CreateAsync(practiceModel));
      }

      [Theory]
      // invalid ods code
      [InlineData("", "r.sample@nhs.net", "1.0")]
      [InlineData(null, "r.sample@nhs.net", "1.0")]
      // invalid email address when provided
      [InlineData("FG123", "",  "1.5")]
      [InlineData("FG123", "d.com",  "2.0")]
      [InlineData("FG123", "@d.com",  "2.0")]
      [InlineData("FG123", "a.a.com",  "1.5")]
      // invalid template name
      [InlineData("FG123", "r.sample@nhs.net",  "")]
      [InlineData("FG123", "r.sample@nhs.net",  null)]
      public async Task PharmacyInvalidException(
        string odsCode, string email, string version)
      {
        // arrange
        Pharmacy practiceModel = new Pharmacy
        {
          OdsCode = odsCode,
          Email = email,
          TemplateVersion = version,
        };

        // act
        await Assert.ThrowsAsync<PharmacyInvalidException>(
          async () => await _service.CreateAsync(practiceModel));
      }

      [Theory]
      // all properties
      [InlineData("FG123", "r.sample@nhs.net",  "1.0")]
      [InlineData("FG123", "r.sample@nhs.net",  "1.5")]
      [InlineData("FG123", "r.sample@nhs.net",  "2.0")]
      public async Task Successful(
        string odsCode, string email, string version)
      {
        // arrange
        IPharmacy expected = new Pharmacy
        {
          OdsCode = odsCode,
          Email = email,
          TemplateVersion = version,
          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
          IsActive = true
        };

        // act
        IPharmacy result = await _service.CreateAsync(expected);

        // assert        
        result.Should().BeOfType<Pharmacy>();
        result.Should().BeEquivalentTo(expected, option => option
          .Excluding(p => p.Id)
          .Excluding(p => p.ModifiedAt));
        result.Id.Should().NotBe(Guid.Empty);
        result.ModifiedAt.Should().BeBefore(DateTimeOffset.Now);

        Entities.Pharmacy actual = _context.Pharmacies.FirstOrDefault();
        actual.Should().BeEquivalentTo(result);
      }

      [Fact]
      public async Task SuccessfulWithExistingDeactivatedPharmacy()
      {
        // arrange
        Entities.Pharmacy deactivatedPharmacy = CreateRandomPharmacyInDatabase(
          isActive: false);

        IPharmacy expected = new Pharmacy
        {
          Id = deactivatedPharmacy.Id,
          OdsCode = deactivatedPharmacy.OdsCode,
          Email = deactivatedPharmacy.Email,
          TemplateVersion = deactivatedPharmacy.TemplateVersion,
          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
          IsActive = true
        };

        // act
        IPharmacy result = await _service.CreateAsync(expected);

        // assert
        result.Should().BeOfType<Pharmacy>();
        result.Should().BeEquivalentTo(expected, option => option
          .Excluding(p => p.ModifiedAt));
        result.Id.Should().NotBe(Guid.Empty);
        result.ModifiedAt.Should().BeBefore(DateTimeOffset.Now);

        Entities.Pharmacy actual = _context.Pharmacies.FirstOrDefault();
        actual.Should().BeEquivalentTo(result);
      }
    }

    public class GetAsync : PharmacyServiceTests
    {
      public GetAsync(ServiceFixture serviceFixture)
        : base(serviceFixture)
      { }

      [Fact]
      public async Task WhereAllPharmaciesAreActive()
      {
        // arrange
        var expected1 = CreateRandomPharmacyInDatabase();
        var expected2 = CreateRandomPharmacyInDatabase();

        // act
        IEnumerable<IPharmacy> pharmacies = await _service.GetAsync();

        pharmacies.Count().Should().Be(2);
        IPharmacy pharmacy1 = 
          pharmacies.Single(p => p.OdsCode == expected1.OdsCode);
        pharmacy1.Should().BeEquivalentTo(expected1);
        IPharmacy pharmacy2 = 
          pharmacies.Single(p => p.OdsCode == expected2.OdsCode);
        pharmacy2.Should().BeEquivalentTo(expected2);
      }

      [Fact]
      public async Task WhereNotAllPharmaciesAreActive()
      {
        // arrange
        var expected = CreateRandomPharmacyInDatabase();
        CreateRandomPharmacyInDatabase(isActive: false);

        // act
        IEnumerable<IPharmacy> practices = await _service.GetAsync();

        practices.Count().Should().Be(1);
        var practice1 = practices.Single(p => p.OdsCode == expected.OdsCode);
        practice1.Should().BeEquivalentTo(expected);
      }

    }

    public class GetByObdCodeAsync : PharmacyServiceTests
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
        Entities.Pharmacy entity = RandomEntityCreator.CreateRandomPharmacy(
          modifiedByUserId: Guid.Parse(TEST_USER_ID));
        _context.Add(entity);
        _context.SaveChanges();

        Pharmacy expected = _serviceFixture.Mapper.Map<Pharmacy>(entity);

        // act
        Pharmacy actual = await _service.GetByObsCodeAsync(entity.OdsCode);

        // assert
        actual.Should().BeOfType<Pharmacy>();
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
        CreateRandomPharmacyInDatabase();

        // act
        Pharmacy actual = await _service
          .GetByObsCodeAsync("NotAPharmacyOdsCode");

        // assert
        actual.Should().BeNull();
      }
    }

    public class UpdateAsync : PharmacyServiceTests
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
      public async Task PharmacyNotFoundException()
      {
        // arrange
        Pharmacy practiceModel = RandomModelCreator.CreateRandomPharmacy();

        // act
        await Assert.ThrowsAsync<PharmacyNotFoundException>(
          async () => await _service.UpdateAsync(practiceModel));
      }

      [Theory]
      // invalid ods code
      [InlineData("", "r.sample@nhs.net",  "1.0")]
      [InlineData(null, "r.sample@nhs.net",  "1.5")]
      // invalid email address when provided
      [InlineData("FG123", "",  "1.5")]
      [InlineData("FG123", "d.com",  "2.0")]
      [InlineData("FG123", "@d.com",  "2.0")]
      [InlineData("FG123", "a.a.com",  "1.5")]
      // invalid TemplateVersion
      [InlineData("FG123", "r.sample@nhs.net",  "")]
      [InlineData("FG123", "r.sample@nhs.net",  null)]
      public async Task PharmacyInvalidException(
        string odsCode, string email, string version)
      {
        // arrange
        Pharmacy practiceModel = new()
        {
          OdsCode = odsCode,
          Email = email,
          TemplateVersion = version,
        };

        // act
        await Assert.ThrowsAsync<PharmacyInvalidException>(
          async () => await _service.UpdateAsync(practiceModel));
      }

      [Theory]
      // all properties
      [InlineData("r.sample@nhs.net",  "1.0")]
      [InlineData("r.sample@nhs.net",  "1.5")]
      [InlineData("r.sample@nhs.net",  "2.0")]
      public async Task Successful(
        string email,  string version)
      {
        // arrange
        Entities.Pharmacy entity = CreateRandomPharmacyInDatabase();

        Pharmacy expected = new()
        {
          Id = entity.Id,
          OdsCode = entity.OdsCode,
          Email = email,
          TemplateVersion = version,
          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
          IsActive = true
        };

        // act
        IPharmacy result = await _service.UpdateAsync(expected);

        // assert
        result.Should().BeOfType<Pharmacy>();
        result.Should().BeEquivalentTo(expected, option => option
          .Excluding(p => p.Id)
          .Excluding(p => p.ModifiedAt));
        result.Id.Should().NotBe(Guid.Empty);
        result.ModifiedAt.Should().BeBefore(DateTimeOffset.Now);

        Entities.Pharmacy actual = _context.Pharmacies.FirstOrDefault();
        result.Should().BeEquivalentTo(result);
      }

      [Fact]
      public async Task SuccessfulWithExistingDeactivatedPharmacy()
      {
        // arrange
        Entities.Pharmacy deactivatedPharmacy = CreateRandomPharmacyInDatabase(
          isActive: false);

        IPharmacy expected = new Pharmacy
        {
          Id = deactivatedPharmacy.Id,
          OdsCode = deactivatedPharmacy.OdsCode,
          Email = deactivatedPharmacy.Email,
          TemplateVersion = deactivatedPharmacy.TemplateVersion,
          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
          IsActive = true
        };

        // act
        IPharmacy result = await _service.UpdateAsync(expected);

        // assert
        result.Should().BeOfType<Pharmacy>();
        result.Should().BeEquivalentTo(expected, option => option
          .Excluding(p => p.ModifiedAt));
        result.Id.Should().NotBe(Guid.Empty);
        result.ModifiedAt.Should().BeBefore(DateTimeOffset.Now);

        Entities.Pharmacy actual = _context.Pharmacies.FirstOrDefault();
        actual.Should().BeEquivalentTo(result);
      }
    }
  }
}
