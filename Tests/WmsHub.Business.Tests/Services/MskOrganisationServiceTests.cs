#pragma warning disable IDE0058
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;
[Collection("Service collection")]
public class MskOrganisationServiceTests : ServiceTestsBase, IDisposable
{
  protected const string VALID_ODS_CODE = "ABCDE";

  protected readonly DatabaseContext _context;
  protected readonly MskOrganisationService _service;

  public MskOrganisationServiceTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
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
    _context.MskOrganisations.RemoveRange(_context.MskOrganisations);
    _context.Database.EnsureDeleted();
  }

  public class AddAsyncTests : MskOrganisationServiceTests
  {
    public AddAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ValidModel_CreatesMskOrganisation()
    {
      // Arrange.
      MskOrganisation model = new()
      {
        OdsCode = VALID_ODS_CODE,
        SendDischargeLetters = true,
        SiteName = "SiteName"
      };

      // Act.
      MskOrganisation returnedModel = await _service.AddAsync(model);

      // Assert.
      using (new AssertionScope())
      {
        returnedModel.Should().BeEquivalentTo(model);
        _context.MskOrganisations
          .SingleOrDefault(mo => mo.OdsCode == model.OdsCode)
          .Should().BeEquivalentTo(model);
      }
    }

    [Theory]
    [InlineData(" ", " ", 2)]
    [InlineData("ABCDEF", "Valid Site Name", 1)]
    public async Task InvalidModel_ThrowsValidationException(
      string odsCode,
      string siteName,
      int numberOfValidationResults)
    {
      // Arrange.
      MskOrganisation model = new()
      {
        OdsCode = odsCode,
        SendDischargeLetters = true,
        SiteName = siteName
      };

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
          ex.GetType().Should().Be(typeof(MskOrganisationValidationException));
          MskOrganisationValidationException mskOrganisationValidationException 
            = ex as MskOrganisationValidationException;
          mskOrganisationValidationException.ValidationResults
            .Should()
            .HaveCount(numberOfValidationResults);
          _context.MskOrganisations
            .Where(mo => mo.OdsCode == model.OdsCode)
            .Should()
            .HaveCount(0);
        }
      }
    }

    [Fact]
    public async Task DuplicateOdsCode_OrganisationNotCreated()
    {
      // Arrange.
      MskOrganisation model = new()
      {
        OdsCode = VALID_ODS_CODE,
        SendDischargeLetters = true,
        SiteName = "SiteName"
      };
      await _service.AddAsync(model);

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
          ex.GetType().Should().Be(typeof(InvalidOperationException));
          ex.Message.Should().Be($"An organisation with the ODS code " +
            $"{model.OdsCode} already exists.");
          _context.MskOrganisations
            .Where(mo => mo.OdsCode == model.OdsCode)
            .Should()
            .HaveCount(1);
        }
      }
    }
  }

  public class DeleteAsyncTests : MskOrganisationServiceTests
  {
    public DeleteAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ExistingEntity_IsActiveSetToFalse()
    {
      // Arrange.
      _context.MskOrganisations.Add(new()
      {
        OdsCode = VALID_ODS_CODE,
        SendDischargeLetters = true,
        IsActive = true,
        SiteName = "SiteName"
      });
      await _context.SaveChangesAsync();

      // Act.
      string result = await _service.DeleteAsync(VALID_ODS_CODE);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().Be($"Referral with ODS code {VALID_ODS_CODE} " +
          $"deleted.");
        _context.MskOrganisations.Single(mo => mo.OdsCode == VALID_ODS_CODE)
          .IsActive.Should().BeFalse();
      }
    }

    [Theory]
    [InlineData("FGHIJ")]
    [InlineData("KLMNO")]
    public async Task NoMskOrganisationWithOdsCode_ThrowsException(
      string odsCode)
    {
      // Arrange.
      _context.MskOrganisations.Add(new()
      {
        OdsCode = "FGHIJ",
        SendDischargeLetters = true,
        IsActive = false,
        SiteName = "SiteName"
      });
      await _context.SaveChangesAsync();

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
          ex.GetType().Should().Be(typeof(MskOrganisationNotFoundException));
          ex.Message.Should().Be("An organisation with the ODS code " +
            $"{odsCode} does not exist.");
        }
      }
    }
  }

  public class ExistsAsyncTests : MskOrganisationServiceTests
  {
    public ExistsAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper) 
    { }

    [Fact]
    public async Task OrganisationPresent_ReturnsTrue()
    {
      // Arrange.
      _context.MskOrganisations.Add(new()
      {
        OdsCode = VALID_ODS_CODE,
        SendDischargeLetters = true,
        IsActive = true,
        SiteName = "SiteName"
      });
      await _context.SaveChangesAsync();

      // Act.
      bool response = await _service.ExistsAsync(VALID_ODS_CODE);

      // Assert.
      response.Should().BeTrue();
    }

    [Fact]
    public async Task OrganisationNotPresent_ReturnsFalse()
    {
      // Arrange.
      _context.MskOrganisations.Add(new()
      {
        OdsCode = VALID_ODS_CODE,
        SendDischargeLetters = true,
        IsActive = false,
        SiteName = "SiteName"
      });
      await _context.SaveChangesAsync();

      // Act.
      bool response = await _service.ExistsAsync("ZYXWV");

      // Assert.
      response.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task InvalidOdsCode_ThrowsException(string odsCode)
    {
      // Arrange.

      try
      {
        // Act.
        bool response = await _service.ExistsAsync(odsCode);
      }
      catch (Exception ex)
      {
        // Assert.
        ex.Should().BeOfType(typeof(ArgumentNullOrWhiteSpaceException));
      }
    }
  }

  public class UpdateAsyncTests : MskOrganisationServiceTests
  {
    public UpdateAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ExistingEntity_Updated()
    {
      // Arrange.
      _context.MskOrganisations.Add(new()
      {
        OdsCode = VALID_ODS_CODE,
        SendDischargeLetters = true,
        IsActive = true,
        SiteName = "SiteName"
      });
      await _context.SaveChangesAsync();

      MskOrganisation submittedUpdate = new()
      {
        OdsCode = VALID_ODS_CODE,
        SendDischargeLetters = false,
        SiteName = "NewSiteName"
      };

      // Act.
      MskOrganisation updatedOrganisation =
        await _service.UpdateAsync(submittedUpdate);

      // Assert.
      using (new AssertionScope())
      {
        updatedOrganisation.Should().BeEquivalentTo(submittedUpdate);
        _context.MskOrganisations
          .Single(mo => mo.OdsCode == VALID_ODS_CODE)
          .SendDischargeLetters
          .Should().BeFalse();
      }
    }

    [Theory]
    [InlineData(" ", " ", 2)]
    [InlineData("ABCDEF", "Valid Site Name", 1)]
    public async Task InvalidModel_ThrowsValidationException(
      string odsCode,
      string siteName,
      int numberOfValidationResults)
    {
      // Arrange.

      Entities.MskOrganisation existingOrganisation = new()
      {
        OdsCode = VALID_ODS_CODE,
        SendDischargeLetters = true,
        IsActive = true,
        SiteName = "SiteName"
      };
      _context.MskOrganisations.Add(existingOrganisation);
      await _context.SaveChangesAsync();

      MskOrganisation submittedUpdate = new()
      {
        OdsCode = odsCode,
        SendDischargeLetters = false,
        SiteName = siteName
      };

      try
      {
        // Act.
        await _service.UpdateAsync(submittedUpdate);
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.GetType().Should().Be(typeof(MskOrganisationValidationException));
          MskOrganisationValidationException mskOrganisationValidationException
            = ex as MskOrganisationValidationException;
          mskOrganisationValidationException.ValidationResults
            .Should()
            .HaveCount(numberOfValidationResults);
          _context.MskOrganisations
            .Where(mo => mo.OdsCode == VALID_ODS_CODE)
            .Single()
            .Should()
            .BeEquivalentTo(existingOrganisation);
        }
      }
    }

    [Theory]
    [InlineData("FGHIJ")]
    [InlineData("KLMNO")]
    public async Task NoMskOrganisationWithOdsCode_ThrowsException(
      string odsCode)
    {
      // Arrange.
      _context.MskOrganisations.Add(new()
      {
        OdsCode = "FGHIJ",
        SendDischargeLetters = true,
        IsActive = false,
        SiteName = "SiteName"
      });
      await _context.SaveChangesAsync();

      MskOrganisation submittedUpdate = new()
      {
        OdsCode = odsCode,
        SendDischargeLetters = false,
        SiteName = "NewSiteName"
      };

      try
      {
        // Act.
        await _service.UpdateAsync(submittedUpdate);
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.GetType().Should().Be(typeof(MskOrganisationNotFoundException));
          ex.Message.Should().Be("An organisation with the ODS code " +
            $"{odsCode} does not exist.");
        }
      }
    }
  }

  public class GetAsyncTests : MskOrganisationServiceTests
  {
    public GetAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task ReturnsArrayOfMskOrganisations()
    {
      // Arrange.
      _context.MskOrganisations.Add(new()
      {
        OdsCode = "FGHIJ",
        SendDischargeLetters = true,
        IsActive = true,
        SiteName = "SiteName"
      });
      _context.MskOrganisations.Add(new()
      {
        OdsCode = "ABCDE",
        SendDischargeLetters = false,
        IsActive = true,
        SiteName = "SiteName"
      });
      _context.MskOrganisations.Add(new()
      {
        OdsCode = "KLMNO",
        SendDischargeLetters = true,
        IsActive = false,
        SiteName = "SiteName"
      });
      await _context.SaveChangesAsync();

      // Act.
      IEnumerable<MskOrganisation> output = await _service.GetAsync();

      // Assert.
      using (new AssertionScope())
      {
        output.Should().HaveCount(2);
        output.ElementAt(0).OdsCode.Should().Be("ABCDE");
        output.ElementAt(1).OdsCode.Should().Be("FGHIJ");
      }
    }
  }
}
