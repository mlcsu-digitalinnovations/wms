using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Controllers;
using WmsHub.Referral.Api.Models;
using Xunit;

namespace WmsHub.Referral.Api.Tests
{
  [Collection("Service collection")]
  public class PracticeControllerTests : ServiceTestsBase, IDisposable
  {
    private const string OWNER_PRACTICE = "Owner.Practice";

    private readonly static Random _random = new Random();

    private static string[] _validPracticeSystemNames =
        Enum.GetNames(typeof(Business.Enums.PracticeSystemName));

    private static List<string> _validOdsCodeFirstChars = new List<string>()
          { "A","B","C","D","E","F","G","H",
            "J","K","L","M","N",
            "P","Q","R","S","T","U","V","W",
            "Y","ALD","GUE","JER"};

    private readonly DatabaseContext _context;
    private readonly PracticeController _controller;
    private readonly PracticeController _controllerNoUser;
    private readonly PracticeController _controllerUnauthorized;
    private readonly PracticeService _service;
    private readonly ClaimsPrincipal _user;

    public PracticeControllerTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _context = new DatabaseContext(_serviceFixture.Options);

      _user = GetClaimsPrincipal();
      _user.AddIdentity(new ClaimsIdentity(new List<Claim>()
      {
        new Claim(ClaimTypes.Name, OWNER_PRACTICE)
      }));

      _service = new PracticeService(
        _context,
        _serviceFixture.Mapper);

      _controller = new PracticeController(
        _service,
        _serviceFixture.Mapper)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext() { User = _user }
        }
      };

      _controllerNoUser = new PracticeController(
        _service,
        _serviceFixture.Mapper);

      _controllerUnauthorized = new PracticeController(
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
      // Delete all practice entities
      _context.Practices.RemoveRange(_context.Practices);
      _context.SaveChanges();
    }

    public class CreateAsync : PracticeControllerTests
    {
      public CreateAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
      { }

      [Theory]
      [MemberData(nameof(GetInvalidPracticeCreationData))]
      public async Task Invalid(
        string odsCode, string systemName, string detail)
      {
        // Arrange.
        Practice expected = new Practice()
        {
          Email = null,
          Name = null,
          OdsCode = odsCode,
          SystemName = systemName
        };

        // Act.
        IActionResult result = await _controller.CreateAsync(expected);

        // Assert.
        var outputResult = Assert.IsType<ObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var problem = Assert.IsType<ProblemDetails>(outputResult.Value);
        problem.Detail.Should().Be(detail);
      }

      [Theory]
      [MemberData(nameof(GetValidPracticeCreationData))]
      public async Task Valid(string odsCode, string systemName)
      {
        // Arrange.
        DateTimeOffset expectedModifiedAt = DateTimeOffset.Now;
        Practice expected = new()
        {
          Email = Generators.GenerateEmail(),
          Name = Generators.GenerateName(_random, 10),
          OdsCode = odsCode,
          SystemName = systemName
        };

        // Act.
        IActionResult result = await _controller.CreateAsync(expected);

        // Assert.
        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        Practice createdPractice = Assert.IsType<Practice>(outputResult.Value);
        createdPractice.Should().BeEquivalentTo(expected);

        _context.Practices.Count().Should().Be(1);
        Business.Entities.Practice practiceEntity = _context.Practices.Single();
        practiceEntity.Should().BeEquivalentTo(expected);
        practiceEntity.IsActive.Should().BeTrue();
        practiceEntity.ModifiedAt.Should()
          .BeCloseTo(expectedModifiedAt, new TimeSpan(0, 0, 1));
        practiceEntity.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }

      public static TheoryData<string, string> GetValidPracticeCreationData()
      {
        TheoryData<string, string> data = [];
        for (int i = 0; i < _validOdsCodeFirstChars.Count; i++)
        {
          data.Add(
            $"{_validOdsCodeFirstChars[i]}12345".Substring(0, 6),
            _validPracticeSystemNames[i % _validPracticeSystemNames.Length]);
        };
        return data;
      }

      public static IEnumerable<object[]> GetInvalidPracticeCreationData()
      {
        List<object[]> invalidData = new List<object[]>();
        invalidData.AddRange(GetInvalidOdsCodeData());
        invalidData.AddRange(GetInvalidSystemNameData());
        return invalidData;
      }
    }

    public class GetAsync : PracticeControllerTests
    {
      public GetAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
      { }

      [Fact]
      public async Task AllPracticesActive()
      {
        // Arrange.
        var expectedPractices = new List<Business.Entities.Practice>()
        { 
          CreatePractice(odsCode: "M11111"),
          CreatePractice(odsCode: "M11112") 
        };
        _context.Practices.AddRange(expectedPractices);
        _context.SaveChanges();

        // Act.
        IActionResult result = await _controller.GetAsync();

        // Assert.
        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        var createdPractices =
          Assert.IsType<List<Practice>>(outputResult.Value);
        createdPractices.Count().Should().Be(expectedPractices.Count());

        foreach (Practice createdPractice in createdPractices)
        {
          var expectedPractice =
            expectedPractices.FirstOrDefault(p =>
              p.OdsCode == createdPractice.OdsCode);

          createdPractice.Should()
            .BeEquivalentTo(expectedPractice, option => option
              .Excluding(p => p.Id)
              .Excluding(p => p.IsActive)
              .Excluding(p => p.ModifiedAt)
              .Excluding(p => p.ModifiedByUserId));
        }
      }

      [Fact]
      public async Task AllPracticesNotActive()
      {
        // Arrange.
        _context.Practices.RemoveRange(_context.Practices);
        _context.SaveChanges();

        var expectedPractices = new List<Business.Entities.Practice>()
        {
          CreatePractice(odsCode: "M111111"),
          CreatePractice(odsCode: "M111112", isActive: false)
        };
        _context.Practices.AddRange(expectedPractices);
        _context.SaveChanges();

        // Act.
        IActionResult result = await _controller.GetAsync();

        // Assert.
        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        var createdPractices =
          Assert.IsType<List<Practice>>(outputResult.Value);
        createdPractices.Count().Should()
          .Be(expectedPractices.Count(p => p.IsActive));

        foreach (Practice createdPractice in createdPractices)
        {
          var expectedPractice =
            expectedPractices.Single(p => p.OdsCode == createdPractice.OdsCode);

          createdPractice.Should()
            .BeEquivalentTo(expectedPractice, option => option
              .Excluding(p => p.Id)
              .Excluding(p => p.IsActive)
              .Excluding(p => p.ModifiedAt)
              .Excluding(p => p.ModifiedByUserId));
        }
      }

      [Fact]
      public async Task InternalServerError()
      {
        // Arrange. -- All practices already removed
        string expectedDetail = $"Test Exception: {DateTimeOffset.Now}";

        var mockPracticeService = new Mock<PracticeService>(
          _context, _serviceFixture.Mapper);

        mockPracticeService.Setup(x => x.GetAsync())
          .ThrowsAsync(new Exception(expectedDetail));

        // Act.
        IActionResult result = await new PracticeController(
          mockPracticeService.Object,
          _serviceFixture.Mapper)
        {
          ControllerContext = new ControllerContext
          {
            HttpContext = new DefaultHttpContext() { User = _user }
          }
        }
          .GetAsync();

        // Assert.
        var outputResult = Assert.IsType<ObjectResult>(result);
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        var problemDetails = Assert.IsType<ProblemDetails>(outputResult.Value);
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);

      }

      [Fact]
      public async Task NoPractices()
      {
        // Arrange. -- All practices already removed

        // Act.
        IActionResult result = await _controller.GetAsync();

        // Assert.
        NoContentResult outputResult = Assert.IsType<NoContentResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
      }

      [Fact]
      public async Task Unauthorized()
      {
        // Arrange.
        string expectedTitle = "Access has not been granted for this endpoint.";
        List<PracticeController> controllers = new List<PracticeController>()
          { _controllerNoUser, _controllerUnauthorized};

        // Act.
        foreach (PracticeController controller in controllers)
        {
          IActionResult result = await controller.GetAsync();

          // Assert.
          var outputResult = Assert.IsType<ObjectResult>(result);
          outputResult.StatusCode.Should()
            .Be(StatusCodes.Status401Unauthorized);
          var problem = Assert.IsType<ProblemDetails>(outputResult.Value);
          problem.Title.Should().Be(expectedTitle);
        }
      }
    }

    public class UpdateAsync : PracticeControllerTests
    {
      const string EXISTING_PRACTICE_ODSCODE = "A99999";
      const string EXISTING_PRACTICE_ODSCODE_NOT_FOUND = "B99999";

      private static Business.Entities.Practice _existingPractice =
        CreatePractice(odsCode: EXISTING_PRACTICE_ODSCODE);

      public UpdateAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
      {
        _context.Practices.Add(_existingPractice);
        _context.SaveChanges();
      }

      [Theory]
      [MemberData(nameof(GetInvalidPracticeUpdateData))]
      public async Task Invalid(
        string odsCode, string systemName, string detail)
      {
        // Arrange.
        Practice expected = new Practice()
        {
          Email = _existingPractice.Email,
          Name = _existingPractice.Name,
          OdsCode = odsCode,
          SystemName = systemName
        };

        // Act.
        IActionResult result = await _controller.UpdateAsync(
          expected.OdsCode, expected);

        // Assert.
        var outputResult = Assert.IsType<ObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var problem = Assert.IsType<ProblemDetails>(outputResult.Value);
        problem.Detail.Should().Be(detail);
      }

      

      [Theory]
      [MemberData(nameof(GetValidPracticeUpdateData))]
      public async Task Valid(string systemName)
      {
        // Arrange.
        Random rnd = new();
        DateTimeOffset expectedModifiedAt = DateTimeOffset.Now;
        Practice expected = new()
        {
          Email = Generators.GenerateEmail(),
          Name = Generators.GenerateName(rnd, 8),
          OdsCode = _existingPractice.OdsCode,
          SystemName = systemName
        };

        // Act.
        IActionResult result = await _controller.UpdateAsync(expected.OdsCode, expected);

        // Assert.
        OkObjectResult outputResult = Assert.IsType<OkObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        Practice updatedPractice = Assert.IsType<Practice>(outputResult.Value);
        updatedPractice.Should().BeEquivalentTo(expected);

        _context.Practices.Count().Should().Be(1);
        Business.Entities.Practice practiceEntity = _context.Practices.Single();
        practiceEntity.Should().BeEquivalentTo(expected);
        practiceEntity.IsActive.Should().BeTrue();
        practiceEntity.ModifiedAt.Should()
          .BeCloseTo(expectedModifiedAt, new TimeSpan(0, 0, 1));
        practiceEntity.ModifiedByUserId.Should().Be(TEST_USER_ID);
      }

      public static TheoryData<string> GetValidPracticeUpdateData()
      {
        TheoryData<string> data = [];
        data.AddRange(_validPracticeSystemNames);
        return data;
      }

      public static IEnumerable<object[]> GetInvalidPracticeUpdateData()
      {
        List<object[]> invalidData = new List<object[]>();
        invalidData.AddRange(
          GetInvalidSystemNameData(_existingPractice.OdsCode));
        invalidData.Add(new object[]
        {
          EXISTING_PRACTICE_ODSCODE_NOT_FOUND,
          _validPracticeSystemNames[0],
          "Unable to find a practice with an OdsCode of " +
          $"{EXISTING_PRACTICE_ODSCODE_NOT_FOUND}."
        });
        return invalidData;
      }
    }

    public static Business.Entities.Practice CreatePractice(
      string email = null,
      bool isActive = true,
      DateTimeOffset? modifiedAt = default,
      string modifiedByUserId = null,
      string name = null,
      string odsCode = null,
      string systemName = null)
    {
      return new Business.Entities.Practice()
      {
        Email = email ?? Generators.GenerateEmail(),
        IsActive = isActive,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt.Value,
        ModifiedByUserId = modifiedByUserId == null
          ? Guid.NewGuid()
          : Guid.Parse(modifiedByUserId),
        Name = name ?? Generators.GenerateName(_random, 10),
        OdsCode = odsCode ?? Generators.GenerateOdsCode(_random),
        SystemName = systemName ??
          _validPracticeSystemNames[
            _random.Next(_validPracticeSystemNames.Length)]
      };
    }

    public static IEnumerable<object[]> GetInvalidSystemNameData(
      string odsCode = null)
    {
      const string INVALID_SYSTEMNAME = "InvalidSystemName";
      string validOdsCode = odsCode ?? $"{_validOdsCodeFirstChars[0]}12345";

      string required = "The SystemName field is required.";

      string invalid = $"The SystemName field '{INVALID_SYSTEMNAME}' must " +
        "be one of the following values [" +
        $"{string.Join(',', _validPracticeSystemNames)}].";


      List<object[]> invalidData = new List<object[]>() {
          new object[] { validOdsCode, null, required },
          new object[] { validOdsCode, "", required },

          new object[] { validOdsCode, INVALID_SYSTEMNAME, invalid },
        };

      return invalidData;
    }

    public static IEnumerable<object[]> GetInvalidOdsCodeData()
    {
      string length = "The OdsCode field must have a length of 6 " +
        "characters.";
      string oneLetter5Num = "The OdsCode field that starts with 1 letter " +
        "must end with 5 numbers.";
      string required = "The OdsCode field is required.";
      string startWith1 = "The OdsCode field must start with a capital " +
        "letter of A-H, J-N, P-W or Y.";
      string startWith3 = "The OdsCode field must start with 3 capital " +
        "letters of ALD, GUE or JER.";
      string threeLetter3Num = "The OdsCode field that starts with 3 " +
        "letters must end with 3 numbers.";

      string systemName = _validPracticeSystemNames[0];

      List<object[]> invalidData = new List<object[]>() {
          new object[] { "I12345", systemName, startWith1 },
          new object[] { "O12345", systemName, startWith1 },
          new object[] { "X12345", systemName, startWith1 },
          new object[] { "Z12345", systemName, startWith1 },

          new object[] { "A1234", systemName, length },
          new object[] { "A123456", systemName, length },

          new object[] { "AAA123", systemName, startWith3 },

          new object[] { "A1234A", systemName, oneLetter5Num },

          new object[] { "JER12A", systemName, threeLetter3Num },

          new object[] { null, systemName, required },
          new object[] { "", systemName, required }
        };

      return invalidData;
    }
  }
}
