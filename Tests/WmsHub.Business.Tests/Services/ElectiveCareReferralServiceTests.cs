using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Execution;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.ElectiveCareReferral;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Apis.Ods;
using WmsHub.Common.Apis.Ods.Fhir;
using WmsHub.Common.Apis.Ods.PostcodesIo;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;
[Collection("Service collection")]
public class ElectiveCareReferralServiceTests : ServiceTestsBase
{
  private const string OPSCODE = "TST1";
  private const string OPCSCODES = "ABC,ZXY";
  private readonly DatabaseContext _context;
  private ElectiveCareReferralService _electiveCareReferralService;
  private readonly ElectiveCareReferralOptions _options = new()
  {
    EnableEnglishOnlyPostcodes = true,
    IgnorePostcodeValidation = true,
    Issuer = "TestIssuer",
    OpcsCodes = OPCSCODES,
    PrincipalName = "TestPrincipal",
  };
  private Mock<IEthnicityService> _mockEthnicityService = new();
  private Mock<IMSGraphService> _mockMSGraphService = new();
  private Mock<IMessageService> _mockMessageService = new();
  private Mock<IOdsFhirService> _mockOdsFhirService = new();
  private Mock<IOdsOrganisationService> _mockOdsOrganisationService = new();
  private Mock<IOptions<ElectiveCareReferralOptions>> _mockOptions = new();
  private Mock<IPostcodesIoService> _mockPostcodesIoService = new();
  private Mock<IReferralService> _mockReferralService = new();

  public ElectiveCareReferralServiceTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper) 
    : base(serviceFixture, testOutputHelper)
  {
    _mockOptions.Setup(x => x.Value).Returns(_options);
    _context = new DatabaseContext(_serviceFixture.Options);
  }

  public class ProcessTrustDataAsyncTests :
    ElectiveCareReferralServiceTests,
    IDisposable
  {
    public ProcessTrustDataAsyncTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _electiveCareReferralService = new(
        _context,
        _mockEthnicityService.Object,
        _mockMSGraphService.Object,
        _log,
        _serviceFixture.Mapper,
        _mockMessageService.Object,
        _mockOdsFhirService.Object,
        _mockOdsOrganisationService.Object,
        _mockOptions.Object,
        _mockPostcodesIoService.Object,
        _mockReferralService.Object
        )
      {
        User = GetClaimsPrincipal()
      };

    }

    public void Dispose()
    {
      _electiveCareReferralService = null;
      _context.ElectiveCarePostErrors.RemoveRange(
        _context.ElectiveCarePostErrors);
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Organisations.RemoveRange(_context.Organisations);
      _context.Database.EnsureDeleted();
    }

    [Fact]
    public async Task TrustDataIsNullThrowArgumentNullException()
    {
      // Arrange.
      string expectedErrorMessage = "Value cannot be null. " +
        "(Parameter 'trustData')";

      // Act.
      Func<Task> act = async () =>
      {
        await _electiveCareReferralService.ProcessTrustDataAsync(null, OPSCODE, Guid.Empty);
      };

      // Assert.
      await act.Should().ThrowAsync<ArgumentNullException>()
        .WithMessage(expectedErrorMessage);

    }

    [Fact]
    public async Task TrustDataIsEmpty_ReturnsProcessTrustData_Errors()
    {
      // Arrange.
      int key = 0;
      string expectedErrorMessage = "There are no data rows to process.";
      List<ElectiveCareReferralTrustData> testTrustData = new();

      // Act.
      ProcessTrustDataResult result = await _electiveCareReferralService.ProcessTrustDataAsync(
        testTrustData,
        OPSCODE,
        Guid.Empty);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().ContainKey(key);
        result.Errors[key].Should().Contain(expectedErrorMessage);
      }
    }

    [Fact]
    public async Task TrustData_InvalidRow_AddError()
    {
      // Arrange.
      List<ElectiveCareReferralTrustData> testTrustData = new()
      {
        GenerateElectiveCareReferralTrustData
        (
          dateOfBirth:  Under18,
          dateOfTrustReportedBmi: FutureDate,
          whenPlacedOnWaitingList: PlacedOnWaitList,
          opcsCode: "TEST",
          trustReportBmi: 27.0M
        )
      };
      Guid trustUserId = Guid.NewGuid();
      int expectedErrorCount = 5;

      // Act.
      ProcessTrustDataResult result = await _electiveCareReferralService.ProcessTrustDataAsync(
        testTrustData,
        OPSCODE,
        trustUserId);

      // Assert.
      using (new AssertionScope())
      {
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().ContainKey(0);
        result.Errors[0].Should().HaveCount(expectedErrorCount);
        _context.ElectiveCarePostErrors.Should().HaveCount(expectedErrorCount);
        _context.ElectiveCarePostErrors.Count(t => t.TrustUserId == trustUserId)
          .Should().Be(expectedErrorCount);
      }
    }

    [Fact]
    public async Task TrustData_Invalid_Row_Errors_1()
    {
      // Arrange.
      string[] expectedErrors = [
        "The field 'Date of Birth' must equate to an age between 18 and 110.",
        "The field 'Date of Birth' cannot be in the future.",
        "The field 'Date of Trust Reported BMI' cannot be in the future.",
        "The field 'Date Placed On Waiting List' cannot be more than 3 years ago.",
        "The field 'Trust Reported BMI' must be between 27.5 and 90.",
        "The field 'OPCS surgery code(s)' does not contain a valid OPCS code."
      ];

      List<ElectiveCareReferralTrustData> trustData = [
        GenerateElectiveCareReferralTrustData(
          dateOfBirth: DateTime.UtcNow.AddDays(2),
          dateOfTrustReportedBmi: FutureDate,
          whenPlacedOnWaitingList: PlacedOnWaitList,
          opcsCode: "TEST",
          trustReportBmi: 27.0M)
      ];

      Guid trustUserId = Guid.NewGuid();

      // Act.
      ProcessTrustDataResult result = await _electiveCareReferralService.ProcessTrustDataAsync(
        trustData,
        OPSCODE,
        trustUserId);

      // Assert.
      result.Errors.Should().HaveCount(1).And.ContainKey(0);
      result.Errors[0].Should().HaveCount(expectedErrors.Length);
      _context.ElectiveCarePostErrors.Should().HaveCount(expectedErrors.Length);
      _context.ElectiveCarePostErrors.Count(t => t.TrustUserId == trustUserId).Should()
        .Be(expectedErrors.Length);
      for (int i = 0; i < expectedErrors.Length; i++)
      {
        string expectedError = expectedErrors[i];
        result.Errors[0][i].Should().Be(expectedError);
      }
    }

    [Fact]
    public async Task TrustData_Invalid_Row_Errors_2()
    {
      // Arrange.
      string[] expectedErrors = [
        "The FamilyName field is required.",
        "The GivenName field is required.",
        "The field 'Mobile' does not contain enough digits to be a valid UK mobile number.",
        "The SexAtBirth field is required.",
        "The field 'Trust Reported BMI' must be between 27.5 and 90.",
        "The field 'OPCS surgery code(s)' does not contain a valid OPCS code."
      ];

      ElectiveCareReferralTrustData trustDataItem = GenerateElectiveCareReferralTrustData(
        dateOfBirth: DateTime.UtcNow.AddYears(-40),
        whenPlacedOnWaitingList: -400,
        mobile: "+4407777555222",
        opcsCode: "TEST",
        trustReportBmi: 27.0M);

      trustDataItem.FamilyName = "";
      trustDataItem.GivenName = "";
      trustDataItem.SexAtBirth = null;

      List<ElectiveCareReferralTrustData> trustData = [trustDataItem];

      Guid trustUserId = Guid.NewGuid();

      // Act.
      ProcessTrustDataResult result = await _electiveCareReferralService.ProcessTrustDataAsync(
        trustData,
        OPSCODE,
        trustUserId);

      // Assert.
      result.Errors.Should().HaveCount(1).And.ContainKey(0);
      result.Errors[0].Should().HaveCount(expectedErrors.Length);
      _context.ElectiveCarePostErrors.Should().HaveCount(expectedErrors.Length);
      _context.ElectiveCarePostErrors.Count(t => t.TrustUserId == trustUserId).Should()
        .Be(expectedErrors.Length);
      for (int i = 0; i < expectedErrors.Length; i++)
      {
        string expectedError = expectedErrors[i];
        result.Errors[0][i].Should().Be(expectedError);
      }
    }

    [Fact]
    public async Task TrustData_Invalid_Row_Errors_3()
    {
      // Arrange.
      string[] expectedErrors = [
        "The Mobile field is required.",
        "The NhsNumber field is required.",
        "The OpcsCodes field is required.",
        "The Postcode field is required.",
        "The TrustOdsCode field is required.",
        "The field 'Trust Reported BMI' must be between 27.5 and 90.",
        "The field 'OPCS surgery code(s)' does not contain a valid OPCS code."
      ];

      ElectiveCareReferralTrustData trustDataItem = GenerateElectiveCareReferralTrustData(
        dateOfBirth: DateTime.UtcNow.AddYears(-40),
        whenPlacedOnWaitingList: -400,
        opcsCode: "TEST",
        trustReportBmi: 27.0M);

      trustDataItem.OpcsCodes = null;
      trustDataItem.Mobile = "";
      trustDataItem.NhsNumber = null;
      trustDataItem.Postcode = null;
      trustDataItem.TrustOdsCode = null;

      List<ElectiveCareReferralTrustData> trustData = [trustDataItem];

      Guid trustUserId = Guid.NewGuid();

      // Act.
      ProcessTrustDataResult result = await _electiveCareReferralService.ProcessTrustDataAsync(
        trustData,
        OPSCODE,
        trustUserId);

      // Assert.
      result.Errors.Should().HaveCount(1).And.ContainKey(0);
      result.Errors[0].Should().HaveCount(expectedErrors.Length);
      _context.ElectiveCarePostErrors.Should().HaveCount(expectedErrors.Length);
      _context.ElectiveCarePostErrors.Count(t => t.TrustUserId == trustUserId).Should()
        .Be(expectedErrors.Length);
      for (int i = 0; i < expectedErrors.Length; i++)
      {
        string expectedError = expectedErrors[i];
        result.Errors[0][i].Should().Be(expectedError);
      }
    }

    [Theory]
    [InlineData(" 12 34 567  890  ", "The field 'NHS Number' is invalid.")]
    [InlineData("   ", "The NhsNumber field is required.")]
    public async Task InvalidOrMissingNhsNumberWithWhiteSpaceRejected(
      string nhsNumber, string expectedError)
    {
      // Arrange.
      List<ElectiveCareReferralTrustData> data =
        new()
        {
          GenerateElectiveCareReferralTrustData(
          dateOfBirth: DateTimeOffset.UtcNow.AddYears(-19),
          nhsNumber: nhsNumber)
        };

      // Act.
      ProcessTrustDataResult result = await _electiveCareReferralService.ProcessTrustDataAsync(
        data,
        OPSCODE,
        Guid.NewGuid());

      // Assert.
      using (new AssertionScope())
      {
        result.Errors.Should().HaveCount(1);
        result.Errors[0][0].Should().Be(expectedError);
        result.IsValid.Should().BeFalse();
        result.NoOfReferralsCreated.Should().Be(0);
      }
    }

    [Fact]
    public async Task ValidNhsNumberWithWhiteSpaceTrimmedAndAccepted()
    {
      // Arrange.
      const string VALID_NHS_NUMBER_NO_WHITESPACE = "9994469991";
      const string VALID_NHS_NUMBER_WITH_WHITESPACE = " 99 94 469  991   ";

      List<ElectiveCareReferralTrustData> data = new()
      {
        GenerateElectiveCareReferralTrustData(
          dateOfBirth: DateTimeOffset.UtcNow.AddYears(-19),
          nhsNumber: VALID_NHS_NUMBER_WITH_WHITESPACE)
      };

      Organisation organisation = new()
      {
        Id = Guid.NewGuid(),
        IsActive = true,
        OdsCode = OPSCODE,
        ModifiedAt = DateTimeOffset.UtcNow,
        ModifiedByUserId = Guid.NewGuid(),
        QuotaRemaining = 1,
        QuotaTotal = 1
      };

      _context.Organisations.Add(organisation);
      await _context.SaveChangesAsync();

      // Act.
      ProcessTrustDataResult result = await _electiveCareReferralService.ProcessTrustDataAsync(
        data,
        OPSCODE,
        Guid.NewGuid());

      // Assert.
      using (new AssertionScope())
      {
        result.Errors.Should().HaveCount(0);
        result.IsValid.Should().BeTrue();
        result.NoOfReferralsCreated.Should().Be(1);
        _context.Referrals.SingleOrDefault(r =>
          r.NhsNumber.Equals(VALID_NHS_NUMBER_NO_WHITESPACE))
          .Should()
          .NotBeNull();
      }
    }

    private static DateTimeOffset FutureDate =>
      DateTimeOffset.UtcNow.AddDays(10);
    private static DateTimeOffset Over110 =>
      DateTimeOffset.UtcNow.AddYears(-111);
    private static int PlacedOnWaitList => -(365 * 4);
    private static DateTimeOffset Under18 =>
      DateTimeOffset.UtcNow.AddYears(-16);


    private static ElectiveCareReferralTrustData
      GenerateElectiveCareReferralTrustData(
      DateTimeOffset? dateOfBirth,
      DateTimeOffset? dateOfTrustReportedBmi = null,
      string ethnicity = null,
      string familyName = null,
      string givenName = null,
      string mobile = null,
      string nhsNumber = null,
      string opcsCode = null,
      string postCode = null,
      int rowNumber = 0,
      string serviceUserEthnicity = null,
      string serviceUserEthnicityGroup = null,
      string sexAtBirth = null,
      string sourceEthnicity = null,
      string spellIdentifier = null,
      bool? surgeryInLessThanEighteenWeeks = false,
      string trustOdsCode = null,
      decimal? trustReportBmi = null,
      int whenPlacedOnWaitingList = 0)
    {
      ElectiveCareReferralTrustData trustData = new();

      EthnicityGrouping ethnicityGroup = Generators.GenerateEthnicityGrouping(
        new Random(),
        ethnicity);

      trustData.DateOfBirth = dateOfBirth == null
        ? Generators.GenerateDateOfBirth(new Random()).Value
        : dateOfBirth.Value;
      trustData.DateOfTrustReportedBmi = dateOfTrustReportedBmi;
      trustData.DatePlacedOnWaitingList = DateTimeOffset.UtcNow.AddDays(
        whenPlacedOnWaitingList);
      trustData.Ethnicity = ethnicity ?? ethnicityGroup.Ethnicity;
      trustData.FamilyName = familyName
        ?? Generators.GenerateName(new Random(), 10);
      trustData.GivenName = givenName
        ?? Generators.GenerateName(new Random(), 10);
      trustData.Mobile = mobile ?? Generators.GenerateMobile(new Random());
      trustData.NhsNumber = nhsNumber ?? Generators.GenerateNhsNumber(
        new Random());
      trustData.OpcsCodes = opcsCode ?? OPCSCODES;
      trustData.Postcode = postCode ?? Generators.GeneratePostcode(
        new Random());
      trustData.RowNumber = rowNumber;
      trustData.ServiceUserEthnicity = serviceUserEthnicity
        ?? ethnicityGroup.ServiceUserEthnicity;
      trustData.ServiceUserEthnicityGroup = serviceUserEthnicityGroup
        ?? ethnicityGroup.ServiceUserEthnicityGroup;
      trustData.SexAtBirth = sexAtBirth ?? Generators.GenerateSex(new Random());
      trustData.SourceEthnicity = sourceEthnicity;
      trustData.SpellIdentifier = spellIdentifier;
      trustData.SurgeryInLessThanEighteenWeeks = surgeryInLessThanEighteenWeeks;
      trustData.TrustOdsCode = trustOdsCode ?? OPSCODE;
      trustData.TrustReportedBmi = trustReportBmi == null
        ? 31.5M
        : trustReportBmi.Value;
      return trustData;
    }
  }
}
