using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;
[Collection("Service collection")]
public class OrganisationServiceTests : ServiceTestsBase, IDisposable
{
  protected readonly DatabaseContext _context;
  protected readonly OrganisationService _service;

  public OrganisationServiceTests(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
    _context = new DatabaseContext(serviceFixture.Options);
    _service = new(_context)
    {
      User = GetClaimsPrincipal()
    };
  }

  public void Dispose()
  {
    CleanUp();
    GC.SuppressFinalize(this);
  }

  private void CleanUp()
  {
    _context.Organisations.RemoveRange(_context.Organisations);
    _context.Database.EnsureDeleted();
  }

  public class AddAsyncTests : OrganisationServiceTests
  {
    public AddAsyncTests(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ValidOdsCode_CreatesOrganisation()
    {
      // Arrange.
      Organisation model = new() { OdsCode = "ABC" };

      // Act.
      await _service.AddAsync(model);

      // Assert.
      _context.Organisations.Any(org => org.OdsCode == model.OdsCode).Should().BeTrue();
    }

    [Fact]
    public async Task DuplicateOdsCode_ThrowsInvalidOperationException()
    {
      // Arrange.
      string odsCode = "ABC";

      Entities.Organisation storedOrganisation = new()
      {
        OdsCode = odsCode,
        IsActive = true,
        Id = Guid.NewGuid()
      };

      _context.Organisations.Add(storedOrganisation);
      await _context.SaveChangesAsync();

      Organisation model = new() { OdsCode = odsCode };

      try
      {
        // Act.
        await _service.AddAsync(model);
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.Should().BeOfType<InvalidOperationException>();
          ex.Message.Should().Be($"An organisation with the ODS code {odsCode} already exists.");
        }
      }
    }

    [Fact]
    public async Task NullOrganisation_ThrowsArgumentNullException()
    {
      // Arrange.

      try
      {
        // Act.
        await _service.AddAsync(null);
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.Should().BeOfType<ArgumentNullException>();
          ex.Message.Should().Be("Value cannot be null. (Parameter 'organisation')");
        }
      }
    }
  }

  public class DeleteAsyncTests : OrganisationServiceTests
  {
    public DeleteAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ValidOdsCode_DeletesOrganisation()
    {
      // Arrange.
      string odsCode = "ABC";
      Entities.Organisation organisation = new()
      {
        Id = Guid.NewGuid(),
        OdsCode = odsCode,
        IsActive = true
      };

      _context.Organisations.Add(organisation);
      await _context.SaveChangesAsync();

      // Act.
      await _service.DeleteAsync(odsCode);

      // Assert.
      _context.Organisations.Single(org => org.OdsCode == odsCode).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task NoMatchingOdsCode_ThrowsInvalidOperationException()
    {
      // Arrange.
      string odsCode = "ABC";

      try
      {
        // Act.
        await _service.DeleteAsync(odsCode);
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.Should().BeOfType<InvalidOperationException>();
          ex.Message.Should().Be($"An organisation with the ODS code {odsCode} does not exist.");
        }
      }
    }

    [Fact]
    public async Task WhitespaceOdsCode_ThrowsArgumentNullOrWhiteSpaceException()
    {
      // Arrange.

      try
      {
        // Act.
        await _service.DeleteAsync("");
      }
      catch (Exception ex)
      {
        using (new AssertionScope())
        {
          ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>();
          ex.Message.Should().Be("Value cannot be null or white space. (Parameter 'odsCode')");
        }
      }
    }
  }

  public class GetAsyncTests : OrganisationServiceTests
  {
    public GetAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReturnsActiveOrganisationsInOdsCodeOrder()
    {
      // Arrange.
      Entities.Organisation activeOrganisation1 = new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = "DEF"
      };

      _context.Organisations.Add(activeOrganisation1);

      Entities.Organisation activeOrganisation2 = new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = "ABC"
      };

      _context.Organisations.Add(activeOrganisation2);

      Entities.Organisation inactiveOrganisation = new()
      {
        Id = Guid.NewGuid(),
        IsActive = false,
        OdsCode = "GHI"
      };

      _context.Organisations.Add(inactiveOrganisation);
      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<Organisation> organisations = await _service.GetAsync();

      // Assert.
      using (new AssertionScope())
      {
        organisations.Should().HaveCount(2);
        organisations.First().OdsCode.Should().Be(activeOrganisation2.OdsCode);
        organisations.Last().OdsCode.Should().Be(activeOrganisation1.OdsCode);
        organisations.Any(org => org.OdsCode == inactiveOrganisation.OdsCode).Should().BeFalse();
      }
    }
  }

  public class ResetOrganisationQuotasTests : OrganisationServiceTests
  {
    public ResetOrganisationQuotasTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ResetsAllActiveOrganisationQuotas()
    {
      // Arrange.
      Entities.Organisation activeOrganisation1 = new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = "DEF",
        QuotaRemaining = 10,
        QuotaTotal = 100
      };

      _context.Organisations.Add(activeOrganisation1);

      Entities.Organisation activeOrganisation2 = new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = "ABC",
        QuotaRemaining = 20,
        QuotaTotal = 200
      };

      _context.Organisations.Add(activeOrganisation2);

      Entities.Organisation inactiveOrganisation = new()
      {
        Id = Guid.NewGuid(),
        IsActive = false,
        OdsCode = "GHI",
        QuotaRemaining = 30,
        QuotaTotal = 300
      };

      _context.Organisations.Add(inactiveOrganisation);
      await _context.SaveChangesAsync();

      // Act.
      await _service.ResetOrganisationQuotas();

      // Assert.
      using (new AssertionScope())
      {
        _context.Organisations.Single(org => org.Id == activeOrganisation1.Id)
          .QuotaRemaining
          .Should()
          .Be(activeOrganisation1.QuotaTotal);
        _context.Organisations.Single(org => org.Id == activeOrganisation2.Id)
          .QuotaRemaining
          .Should()
          .Be(activeOrganisation2.QuotaTotal);
        _context.Organisations.Single(org => org.Id == inactiveOrganisation.Id)
          .QuotaRemaining
          .Should()
          .Be(inactiveOrganisation.QuotaRemaining);
      }
    }
  }

  public class UpdateAsyncTests : OrganisationServiceTests
  {
    public UpdateAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ValidOdsCode_UpdatesOrganisation()
    {
      // Arrange.
      Entities.Organisation storedOrganisation = new()
      {
        Id = Guid.NewGuid(),
        OdsCode = "ABC",
        QuotaTotal = 100,
        QuotaRemaining = 100
      };

      _context.Organisations.Add(storedOrganisation);
      await _context.SaveChangesAsync();

      Organisation update = new()
      {
        OdsCode = "ABC",
        QuotaTotal = 400,
        QuotaRemaining = 400
      };

      // Act.
      Organisation updatedOrganisation = await _service.UpdateAsync(update);

      // Assert.
      updatedOrganisation.Should().BeEquivalentTo(update);
    }

    [Fact]
    public async Task NoMatchingOdsCode_ThrowsInvalidOperationException()
    {
      // Arrange.
      string odsCode = "XYZ";

      Organisation update = new()
      {
        OdsCode = odsCode,
        QuotaTotal = 400,
        QuotaRemaining = 400
      };

      try
      {
        // Act.
        Organisation updatedOrganisation = await _service.UpdateAsync(update);
      }
      catch (Exception ex)
      {
        using (new AssertionScope())
        {
          ex.Should().BeOfType<InvalidOperationException>();
          ex.Message.Should().Be($"An organisation with the ODS code {odsCode} does not exist.");
        }
      }
    }

    [Fact]
    public async Task NullOrganisation_ThrowsArgumentNullException()
    {
      // Arrange.

      try
      {
        // Act.
        Organisation updatedOrganisation = await _service.UpdateAsync(null);
      }
      catch (Exception ex)
      {
        using (new AssertionScope())
        {
          ex.Should().BeOfType<ArgumentNullException>();
          ex.Message.Should().Be("Value cannot be null. (Parameter 'organisation')");
        }
      }
    }
  }
}
