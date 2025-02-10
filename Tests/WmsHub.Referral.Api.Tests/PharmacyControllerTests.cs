using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Controllers;
using WmsHub.Referral.Api.Models;
using Xunit;

namespace WmsHub.Referral.Api.Tests
{
  
  [Collection("Service collection")]
  public class PharmacyControllerTests : ServiceTestsBase, IDisposable
  {
    private const string OWNER_PHARMACY = "Owner.Pharmacy";

    private static readonly Random _random = new();

    private static List<string> _validOdsCodeFirstPharmacyChars = new()
    { "FA", "FB", "FC", "FD","FE", "FG","FH","FI","FJ","FK","FL","FM",
        "FM","FN","FO","FP","FQ","FR","FS","FT","FU","FV","FW","FX","FY"};

    private static DatabaseContext _context;
    private static PharmacyController _controller;
    private readonly PharmacyController _controllerNoUser;
    private readonly PharmacyController _controllerUnauthorized;
    private readonly PharmacyService _service;
    private readonly ClaimsPrincipal _user;

    public PharmacyControllerTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _context = new DatabaseContext(_serviceFixture.Options);

      _user = GetClaimsPrincipal();
      _user.AddIdentity(new ClaimsIdentity(new List<Claim>()
      {
        new(ClaimTypes.Name, OWNER_PHARMACY)
      }));

      _service = new PharmacyService(
        _context,
        _serviceFixture.Mapper);

      _controller = new PharmacyController(
        _service,
        _serviceFixture.Mapper)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext() { User = _user }
        }
      };

      _controllerNoUser = new PharmacyController(
        _service,
        _serviceFixture.Mapper);

      _controllerUnauthorized = new PharmacyController(
        _service,
        _serviceFixture.Mapper)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext()
          {
            User = GetClaimsPrincipalWithId(TEST_USER_ID)
          }
        }
      };

      CleanUp();
    }

    public void Dispose()
    {
      CleanUp();
    }

    private void CleanUp()
    {
      // Delete all pharmacies entities
      _context.Pharmacies.RemoveRange(_context.Pharmacies);
      _context.SaveChanges();
    }

    public class CreateAsync : PharmacyControllerTests
    {
      public CreateAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
      { }

      [Theory]
      [InlineData("")]
      [InlineData(null)]
      public async Task Invalid(string odsCode)
      {
        // ARRANGE
        string expectedDetail = "The OdsCode field is required.";
        PharmacyPost expected = new()
        {
          Email = Generators.GenerateNhsEmail(),
          OdsCode = odsCode,
          TemplateVersion = "1.0"
        };

        // ACT
        IActionResult result = await _controller.CreateAsync(expected);

        // ASSERT
        ObjectResult outputResult = Assert.IsType<ObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        ProblemDetails problem = Assert.IsType<ProblemDetails>(outputResult.Value);
        problem.Detail.Should().Contain(expectedDetail);
      }

      [Theory]
      [MemberData(nameof(GetValidTemplateVersionsData))]
      public async Task ValidCreate(string templateVersion)
      {
        // ARRANGE
        DateTimeOffset expectedModifiedAt = DateTimeOffset.Now;
        PharmacyPost expected = new()
        {
          Email = Generators.GenerateNhsEmail(),
          OdsCode = Generators.GeneratePharmacyOdsCode(_random),
          TemplateVersion = templateVersion
        };
        // ACT
        IActionResult result = await _controller.CreateAsync(expected);

        // ASSERT
        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        PharmacyPost createdPharmacy = 
          Assert.IsType<PharmacyPost>(outputResult.Value);
        createdPharmacy.Should().BeEquivalentTo(expected);

        _context.Pharmacies.Count().Should().Be(1);
        Pharmacy entity = _context.Pharmacies.Single();
        entity.Should().BeEquivalentTo(expected);
        entity.IsActive.Should().BeTrue();
        entity.ModifiedAt.Should()
          .BeCloseTo(expectedModifiedAt, new TimeSpan(0,0,1));
        entity.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }

      [Theory]
      [MemberData(nameof(GetValidTemplateVersionsData))]
      public async Task InvalidOdsCreate(string templateVersion)
      {
        // ARRANGE
        string detail = "The OdsCode field must have a length of ";
        DateTimeOffset expectedModifiedAt = DateTimeOffset.Now;
        PharmacyPost expected = new()
        {
          Email = Generators.GenerateNhsEmail(),
          OdsCode = Generators.GeneratePharmacyOdsCode(_random) + "Br123",
          TemplateVersion = templateVersion
        };
        // ACT
        IActionResult result = await _controller.CreateAsync(expected);

        // ASSERT
        ObjectResult outputResult = Assert.IsType<ObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        ProblemDetails problem =
          Assert.IsType<ProblemDetails>(outputResult.Value);
        problem.Detail.Should().Contain(detail);

      }

      public static TheoryData<string> GetValidTemplateVersionsData()
      {
        TheoryData<string> data = [];
        data.Add("1.0");
        data.Add("1.5");
        data.Add("2.0");
        return data;
      }
    }

    public class GetAsync : PharmacyControllerTests
    {
      public GetAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
      { }

      [Fact]
      public async Task AllPharmaciesActive()
      {
        // ARRANGE
        List<Pharmacy> expectedPharmacies =
        [
          CreatePharmacy(odsCode: "M11111"),
          CreatePharmacy(odsCode: "M11112")
        ];
        _context.Pharmacies.AddRange(expectedPharmacies);
        _context.SaveChanges();

        // ACT
        IActionResult result = await _controller.GetAsync();

        // ASSERT
        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        List<PharmacyPut> createdPharmacies =
          Assert.IsType<List<PharmacyPut>>(outputResult.Value);
        createdPharmacies.Count().Should().Be(expectedPharmacies.Count());

        foreach (PharmacyPut createdPharmacy in createdPharmacies)
        {
          Pharmacy expectedPharmacy =
            expectedPharmacies.FirstOrDefault(p =>
              p.OdsCode == createdPharmacy.OdsCode);

          createdPharmacy.Should()
            .BeEquivalentTo(expectedPharmacy, option => option
              .Excluding(p => p.Id)
              .Excluding(p => p.IsActive)
              .Excluding(p => p.ModifiedAt)
              .Excluding(p => p.ModifiedByUserId));
        }
      }

      [Fact]
      public async Task AllPharmaciesNotActive()
      {
        // ARRANGE
        _context.Pharmacies.RemoveRange(_context.Pharmacies);
        _context.SaveChanges();

        List<Pharmacy> expectedPharmacies = new()
        {
          CreatePharmacy(odsCode: "M111111"),
          CreatePharmacy(odsCode: "M111112", isActive: false)
        };
        await _context.Pharmacies.AddRangeAsync(expectedPharmacies);
        await _context.SaveChangesAsync();

        // ACT
        IActionResult result = await _controller.GetAsync();

        // ASSERT
        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        List<PharmacyPut> createdPharmacies =
          Assert.IsType<List<PharmacyPut>>(outputResult.Value);
        createdPharmacies.Count().Should()
          .Be(expectedPharmacies.Count(p => p.IsActive));

        foreach (PharmacyPut createdPharmacy in createdPharmacies)
        {
          Pharmacy expectedPharmacy =
            expectedPharmacies.Single(p => p.OdsCode == createdPharmacy.OdsCode);

          createdPharmacy.Should()
            .BeEquivalentTo(expectedPharmacy, option => option
              .Excluding(p => p.Id)
              .Excluding(p => p.IsActive)
              .Excluding(p => p.ModifiedAt)
              .Excluding(p => p.ModifiedByUserId));
        }
      }

      [Fact]
      public async Task InternalServerError()
      {
        // ARRANGE -- All pharmacies already removed
        string expectedDetail = $"Test Exception: {DateTimeOffset.Now}";

        Mock<PharmacyService> mockPharmacyService = new(
          _context, _serviceFixture.Mapper);

        mockPharmacyService.Setup(x => x.GetAsync())
          .ThrowsAsync(new Exception(expectedDetail));

        // ACT
        IActionResult result = await new PharmacyController(
            mockPharmacyService.Object,
          _serviceFixture.Mapper)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext() { User = _user }
          }
        }
          .GetAsync();

        // ASSERT
        ObjectResult outputResult = Assert.IsType<ObjectResult>(result);
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        ProblemDetails problemDetails = 
          Assert.IsType<ProblemDetails>(outputResult.Value);
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);

      }

      [Fact]
      public async Task NoPharmacies()
      {
        // ARRANGE -- All Pharmacy already removed

        // ACT
        IActionResult result = await _controller.GetAsync();

        // ASSERT
        NoContentResult outputResult = Assert.IsType<NoContentResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
      }

      [Fact]
      public async Task Unauthorized()
      {
        // ARRANGE
        string expectedTitle = 
          "Access has not been granted for this endpoint.";
        List<PharmacyController> controllers = new()
          { _controllerNoUser, _controllerUnauthorized};

        // ACT
        foreach (PharmacyController controller in controllers)
        {
          IActionResult result = await controller.GetAsync();

          // ASSERT
          ObjectResult outputResult = Assert.IsType<ObjectResult>(result);
          outputResult.StatusCode.Should()
            .Be(StatusCodes.Status401Unauthorized);
          ProblemDetails problem = 
            Assert.IsType<ProblemDetails>(outputResult.Value);
          problem.Title.Should().Be(expectedTitle);
        }
      }
    }

    public class UpdateAsync : PharmacyControllerTests
    {
      const string EXISTING_PHARMACY_ODSCODE = "FG123";
      const string EXISTING_PHARMACY_ODSCODE_NOT_FOUND = "FP123";

      private static readonly Pharmacy _existingPharmacy =
        CreatePharmacy(odsCode: EXISTING_PHARMACY_ODSCODE);

      public UpdateAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
      {
        _context.Pharmacies.Add(_existingPharmacy);
        _context.SaveChanges();
      }

      [Theory]
      [MemberData(nameof(GetInvalidPharmacyUpdateData))]
      public async Task Invalid(
        string odsCode, string templateVersion, string detail)
      {
        // ARRANGE
        PharmacyPut expected = new()
        {
          Email = _existingPharmacy.Email,
          OdsCode = odsCode,
          TemplateVersion = templateVersion
        };

        // ACT
        IActionResult result = await _controller.UpdateAsync(
          expected.OdsCode, expected);

        // ASSERT
        ObjectResult outputResult = Assert.IsType<ObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        ProblemDetails problem =
          Assert.IsType<ProblemDetails>(outputResult.Value);
        problem.Detail.Should().Be(detail);
      }

      [Theory]
      [MemberData(nameof(GetValidTemplateVersionsData))]
      public async Task ValidUpdate(string templateVersion)
      {
        // ARRANGE
        DateTimeOffset expectedModifiedAt = DateTimeOffset.Now;
        PharmacyPut expected = new()
        {
          Email = Generators.GenerateNhsEmail(),
          OdsCode = _existingPharmacy.OdsCode,
          TemplateVersion = templateVersion
        };

        // ACT
        IActionResult result = await _controller.UpdateAsync(
          expected.OdsCode, expected);

        // ASSERT
        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        PharmacyPut updatedPharmacy =
          Assert.IsType<PharmacyPut>(outputResult.Value);
        updatedPharmacy.Should().BeEquivalentTo(expected);

        _context.Pharmacies.Count().Should().Be(1);
        Pharmacy entity = _context.Pharmacies.Single();
        entity.Should().BeEquivalentTo(expected);
        entity.IsActive.Should().BeTrue();
        entity.ModifiedAt.Should()
          .BeCloseTo(expectedModifiedAt, new TimeSpan(0, 0, 1));
        entity.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }

      public static TheoryData<string> GetValidTemplateVersionsData()
      {
        TheoryData<string> data = [];
        data.Add("1.0");
        data.Add("1.5");
        data.Add("2.0");
        return data;
      }

      public static TheoryData<string, string, string> GetInvalidPharmacyUpdateData()
      {
        TheoryData<string, string, string> data = [];
        data.Add(
          EXISTING_PHARMACY_ODSCODE_NOT_FOUND,
          "1.0",
          $"Unable to find a pharmacy with an OdsCode of {EXISTING_PHARMACY_ODSCODE_NOT_FOUND}.");
        return data;
      }
    }

    public static Pharmacy CreatePharmacy(
      string email = null,
      bool isActive = true,
      DateTimeOffset? modifiedAt = default,
      string modifiedByUserId = null,
      string name = null,
      string odsCode = null,
      string templateVersion = null)
    {
      return new()
      {
        Email = email ?? Generators.GenerateNhsEmail(),
        IsActive = isActive,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt.Value,
        ModifiedByUserId = modifiedByUserId == null
          ? Guid.NewGuid()
          : Guid.Parse(modifiedByUserId),
        OdsCode = odsCode ?? Generators.GenerateOdsCode(_random),
        TemplateVersion = templateVersion ?? "1.0"
      };
    }
  }
}
