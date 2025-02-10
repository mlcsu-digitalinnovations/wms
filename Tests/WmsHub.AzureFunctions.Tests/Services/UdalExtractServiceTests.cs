using FluentAssertions;
using MELT;
using Mlcsu.Diu.Mustard.Apis.MeshMailbox.Interfaces;
using Mlcsu.Diu.Mustard.Apis.MeshMailbox.Models;
using Moq;
using RichardSzalay.MockHttp;
using System.Diagnostics;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using WmsHub.AzureFunctions.Factories;
using WmsHub.AzureFunctions.Models;
using WmsHub.AzureFunctions.Options;
using WmsHub.AzureFunctions.Services;
using static WmsHub.AzureFunctions.Options.UdalExtractOptions;

namespace WmsHub.AzureFunctions.Tests.Services;

public class UdalExtractServiceTests : IDisposable
{
  private const int FiveMinutes = 300000;
  private readonly Guid _expectedMessageId = Guid.Parse("00000000-0000-0000-0000-000000000001");
  private readonly ProcessStartInfo _expectedProcessStartInfo;
  private readonly MockFileSystem _mockFileSystem;
  private readonly MockHttpMessageHandler _mockHttpMessageHandler;
  private readonly Mock<IMeshMailboxService> _mockMeshMailboxService;
  private readonly Mock<IProcessFactory> _mockProcessFactory;
  private readonly UdalExtract _testUdalExtract;
  private readonly UdalExtractOptions _udalExtractOptions;
  private readonly UdalExtractService _udalExtractService;

  public UdalExtractServiceTests()
  {
    _mockFileSystem = new MockFileSystem();
    _mockHttpMessageHandler = new();
    _mockMeshMailboxService = new Mock<IMeshMailboxService>();
    _mockProcessFactory = new Mock<IProcessFactory>();

    _udalExtractOptions = new UdalExtractOptions
    {
      AgemPipe = new AgemPipeOptions
      {
        FileToEncryptFilename = "testfile.csv",
        EncryptedFilename = "testfile_enc.csv",
        ExtractFilePath = @"extract\file\path",
        ConfigName = "configName",
        CliPath = @"path\to\cli",
        WorkingDirectory = @"working\directory",
        OutputIndicatingSuccess = "Success"
      },
      BiApi = new BiApiOptions
      {
        ApiKey = "api-key",
        Url = "https://bi.api.options.url"
      },
      MeshMailboxApi = new MeshMailboxApiOptions
      {
        CertificateName = "CertificateName",
        KeyVaultUrl = "https://mesh.mailbox.api.options.key.vault.url"
      }
    };

    // Add the extract file path to the mock file system so its available to create the csv into.
    _mockFileSystem.AddDirectory(_udalExtractOptions.AgemPipe.ExtractFilePath);

    _expectedProcessStartInfo = new()
    {
      Arguments =
        $"-i {_udalExtractOptions.AgemPipe.FileToEncryptFilePath} " +
        $"-o {_udalExtractOptions.AgemPipe.EncryptedFilePath} " +
        $"-c \"{_udalExtractOptions.AgemPipe.ConfigName}\" " +
        $"-C {_udalExtractOptions.AgemPipe.ColumnActions}",
      CreateNoWindow = false,
      FileName = _udalExtractOptions.AgemPipe.CliPath,
      RedirectStandardOutput = true,
      UseShellExecute = false,
      WindowStyle = ProcessWindowStyle.Hidden,
      WorkingDirectory = _udalExtractOptions.AgemPipe.WorkingDirectory
    };

    _testUdalExtract = new()
    {
      Age = 30,
      CalculatedBmiAtRegistration = 22.5m,
      Coaching0007 = true,
      Coaching0814 = false,
      Coaching1521 = true,
      Coaching2228 = false,
      Coaching2935 = true,
      Coaching3642 = false,
      Coaching4349 = true,
      Coaching5056 = false,
      Coaching5763 = true,
      Coaching6470 = false,
      Coaching7177 = true,
      Coaching7884 = false,
      ConsentForFutureContactForEvaluation = true,
      DateCompletedProgramme = new DateTime(2024,10,13),
      DateOfBmiAtRegistration = new DateTime(2024, 1, 14),
      DateOfProviderContactedServiceUser = new DateTime(2024, 8, 15),
      DateOfProviderSelection = new DateTime(2024, 10, 16),
      DateOfReferral = new DateTime(2024, 10, 17),
      DatePlacedOnWaitingListForElectiveCare = new DateTime(2024, 10, 18),
      DateStartedProgramme = new DateTime(2024, 10, 19),
      DateToDelayUntil = new DateTime(2024, 10, 20),
      DeprivationQuintile = "IMD2",
      DocumentVersion = 1.0m,
      Ethnicity = "Asian",
      EthnicityGroup = "South Asian",
      EthnicitySubGroup = "Indian",
      GpRecordedWeight = 70.5m,
      GpSourceSystem = "System A",
      HasALearningDisability = false,
      HasAPhysicalDisability = false,
      HasDiabetesType1 = false,
      HasDiabetesType2 = true,
      HasHypertension = true,
      HasRegisteredSeriousMentalIllness = false,
      HeightCm = 175.3m,
      IsVulnerable = false,
      MethodOfContact = "Sms",
      ModifiedAt = new DateTime(2024, 10, 21),
      NhsNumber = "1234567890",
      NumberOfContacts = 5,
      OpcsCodesForElectiveCare = "XYZ123",
      ProviderEngagement0007 = true,
      ProviderEngagement0814 = false,
      ProviderEngagement1521 = true,
      ProviderEngagement2228 = false,
      ProviderEngagement2935 = true,
      ProviderEngagement3642 = false,
      ProviderEngagement4349 = true,
      ProviderEngagement5056 = false,
      ProviderEngagement5763 = true,
      ProviderEngagement6470 = false,
      ProviderEngagement7177 = true,
      ProviderEngagement7884 = false,
      ProviderName = "Healthcare Provider A",
      ProviderUbrn = "UBRN12345",
      ReferralSource = "GP",
      ReferringGpPracticeNumber = "GP123",
      ReferringOrganisationOdsCode = "ORG123",
      Sex = "Male",
      StaffRole = "Nurse",
      Status = "Active",
      TriagedCompletionLevel = 3,
      VulnerableDescription = "None",
      WeightMeasurement0007 = 70.0m,
      WeightMeasurement0814 = 70.2m,
      WeightMeasurement1521 = 70.4m,
      WeightMeasurement2228 = 70.6m,
      WeightMeasurement2935 = 70.8m,
      WeightMeasurement3642 = 71.0m,
      WeightMeasurement4349 = 71.2m,
      WeightMeasurement5056 = 71.4m,
      WeightMeasurement5763 = 71.6m,
      WeightMeasurement6470 = 71.8m,
      WeightMeasurement7177 = 72.0m,
      WeightMeasurement7884 = 72.2m,
      WeightMeasurement8500 = 72.4m
    };

    _udalExtractService = new UdalExtractService(
      _mockFileSystem,
      _mockHttpMessageHandler.ToHttpClient(),
      _mockMeshMailboxService.Object,
      _mockProcessFactory.Object,
      Microsoft.Extensions.Options.Options.Create(_udalExtractOptions));
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    _mockHttpMessageHandler.Dispose();
  }

  public class ProcessAsyncTests() : UdalExtractServiceTests
  {
    [Fact]
    public async Task Should_ProcessSuccessfully()
    {
      // Arrange.
      string expectedFileToEncryptText =
        "NhsNumber,Age,CalculatedBmiAtRegistration,Coaching00-07,Coaching08-14,Coaching15-21," +
        "Coaching22-28,Coaching29-35,Coaching36-42,Coaching43-49,Coaching50-56,Coaching57-63," +
        "Coaching64-70,Coaching71-77,Coaching78-84,ConsentForFutureContactForEvaluation," +
        "DateCompletedProgramme,DateOfBmiAtRegistration,DateOfProviderContactedServiceUser," +
        "DateOfProviderSelection,DateOfReferral,DatePlacedOnWaitingListForElectiveCare," +
        "DateStartedProgramme,DateToDelayUntil,DeprivationQuintile,DocumentVersion,Ethnicity," +
        "EthnicityGroup,EthnicitySubGroup,GpRecordedWeight,GpSourceSystem,HasALearningDisability," +
        "HasAPhysicalDisability,HasDiabetesType1,HasDiabetesType2,HasHypertension," +
        "HasRegisteredSeriousMentalIllness,HeightCm,IsVulnerable,MethodOfContact," +
        "NumberOfContacts,OPCSCodesForElectiveCare,ProviderEngagement00-07," +
        "ProviderEngagement08-14,ProviderEngagement15-21,ProviderEngagement22-28," +
        "ProviderEngagement29-35,ProviderEngagement36-42,ProviderEngagement43-49," +
        "ProviderEngagement50-56,ProviderEngagement57-63,ProviderEngagement64-70," +
        "ProviderEngagement71-77,ProviderEngagement78-84,ProviderName,ProviderUbrn," +
        "ReferralSource,ReferringGpPracticeNumber,ReferringOrganisationOdsCode,Sex,StaffRole," +
        "Status,TriagedCompletionLevel,VulnerableDescription,WeightMeasurement00-07," +
        "WeightMeasurement08-14,WeightMeasurement15-21,WeightMeasurement22-28," +
        "WeightMeasurement29-35,WeightMeasurement36-42,WeightMeasurement43-49," +
        "WeightMeasurement50-56,WeightMeasurement57-63,WeightMeasurement64-70," +
        "WeightMeasurement71-77,WeightMeasurement78-84,WeightMeasurement85" +
        "*1234567890,30,22.5,1,0,1,0,1,0,1,0,1,0,1,0,1,13/10/2024,14/01/2024,15/08/2024," +
        "16/10/2024,17/10/2024,18/10/2024,19/10/2024,20/10/2024,IMD2,1.0,Asian,South Asian,Indian" +
        ",70.5,System A,0,0,0,1,1,0,175.3,0,Sms,5,XYZ123,1,0,1,0,1,0,1,0,1,0,1,0," +
        "Healthcare Provider A,UBRN12345,GP,GP123,ORG123,Male,Nurse,Active,3,None,70.0,70.2,70.4," +
        "70.6,70.8,71.0,71.2,71.4,71.6,71.8,72.0,72.2,72.4*";

      string expectedResult = $"'{_udalExtractOptions.AgemPipe.EncryptedFilename}' containing 1 " +
        $"referrals sent successfully to MeshMailboxApi with message id: {_expectedMessageId}.";

      List<UdalExtract> udalExtractList = [_testUdalExtract];
      string responseMock = JsonSerializer.Serialize(udalExtractList);

      _mockHttpMessageHandler
        .When(HttpMethod.Get, _udalExtractOptions.BiApi.Url)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, responseMock);

      string fileToEncryptText = string.Empty;
      MockFileData mockEncryptedFile = new("MockEncryptedFile");
      Mock<IProcess> mockProcess = new();
      mockProcess
        .Setup(x => x.Start())
        .Returns(true)
        // this callback creates the mock encrypted file produced by the PIPE application.
        .Callback(() => _mockFileSystem.AddFile(
          _udalExtractOptions.AgemPipe.EncryptedFilePath,
          mockEncryptedFile));
      mockProcess
        .Setup(x => x.WaitForExit(FiveMinutes))
        .Returns(true)
        .Callback(() => fileToEncryptText = _mockFileSystem
          .File.ReadAllText(_udalExtractOptions.AgemPipe.FileToEncryptFilePath));
      using MemoryStream outputIndicatingSuccess = new(
        Encoding.UTF8.GetBytes(_udalExtractOptions.AgemPipe.OutputIndicatingSuccess));
      mockProcess
        .Setup(x => x.StandardOutput)
        .Returns(new StreamReader(outputIndicatingSuccess));

      _mockProcessFactory
        .Setup(x => x.Create())
        .Returns(mockProcess.Object);

      _mockMeshMailboxService
        .Setup(x => x.SendMessageWithDefaultsViaMesh(It.IsAny<SendMessageWithDefaultsRequest>()))
        .ReturnsAsync(new SendMessageResponse(new() { MessageId = _expectedMessageId.ToString() }));

      // Act.
      string result = await _udalExtractService.ProcessAsync();

      // Assert.
      result.Should().Be(expectedResult);
      fileToEncryptText.Should().Match(expectedFileToEncryptText);
      mockProcess.VerifySet(p => p.StartInfo = It.Is<ProcessStartInfo>(x =>
        x.Arguments == _expectedProcessStartInfo.Arguments
        && x.CreateNoWindow == _expectedProcessStartInfo.CreateNoWindow
        && x.FileName == _expectedProcessStartInfo.FileName
        && x.RedirectStandardOutput == _expectedProcessStartInfo.RedirectStandardOutput
        && x.UseShellExecute == _expectedProcessStartInfo.UseShellExecute
        && x.WindowStyle == _expectedProcessStartInfo.WindowStyle
        && x.WorkingDirectory == _expectedProcessStartInfo.WorkingDirectory));
      mockProcess.Verify(p => p.Start(), Times.Once);
      mockProcess.VerifyGet(p => p.StandardOutput, Times.Once);
      mockProcess.Verify(p => p.WaitForExit(FiveMinutes), Times.Once);
      _mockFileSystem.FileExists(_udalExtractOptions.AgemPipe.FileToEncryptFilePath)
        .Should().BeFalse();
      _mockFileSystem.FileExists(_udalExtractOptions.AgemPipe.EncryptedFilePath)
        .Should().BeFalse();
    }

    [Fact]
    public async Task Should_Throw_When_BiApiRequest_Returns_NotASuccessStatusCode()
    {
      // Arrange.
      _mockHttpMessageHandler
        .When(HttpMethod.Get, _udalExtractOptions.BiApi.Url)
        .Respond(HttpStatusCode.InternalServerError);

      // Act.
      Func<Task> act = _udalExtractService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<HttpRequestException>()
        .WithMessage($"*GET*{_udalExtractOptions.BiApi.RequestUri}*500*");
    }

    [Fact]
    public async Task Should_Throw_When_ExtractFilePathContainsFiles()
    {
      // Arrange.
      string expectedMessage =
        $"Found and deleted 1*{_udalExtractOptions.AgemPipe.ExtractFilePath}*Investigate*";

      MockFileData existingFileToBeDeleted = new("ExistingFile");
      string existingFileToBeDeletedFilePath = Path.Combine(
        _udalExtractOptions.AgemPipe.ExtractFilePath,
        "ExistingFile");
      _mockFileSystem.AddFile(existingFileToBeDeletedFilePath, existingFileToBeDeleted);

      // Act.
      Func<Task> act = _udalExtractService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Fact]
    public async Task Should_Throw_When_BiApiRequest_Returns_NoContent()
    {
      // Arrange.
      _mockHttpMessageHandler
        .When(HttpMethod.Get, _udalExtractOptions.BiApi.Url)
        .Respond(HttpStatusCode.NoContent);

      // Act.
      Func<Task> act = _udalExtractService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage($"*No data*{_udalExtractOptions.BiApi.From}*{_udalExtractOptions.BiApi.To}*");
    }

    [Fact]
    public async Task Should_Throw_When_PipeToolFailsToStart()
    {
      // Arrange.
      List<UdalExtract> udalExtractList = [new()];
      string responseMock = JsonSerializer.Serialize(udalExtractList);

      _mockHttpMessageHandler
        .When(HttpMethod.Get, _udalExtractOptions.BiApi.Url)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, responseMock);

      // This simulates the PIPE process failing to start.
      Mock<IProcess> mockProcess = new();
      mockProcess
        .Setup(x => x.Start())
        .Returns(false);

      _mockProcessFactory
        .Setup(x => x.Create())
        .Returns(mockProcess.Object);

      // Act.
      Func<Task> act = _udalExtractService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage($"PIPE tool failed to start*{_expectedProcessStartInfo.Arguments}*");
    }

    [Fact]
    public async Task Should_Throw_When_PipeToolWaitsForExitTooLong()
    {
      // Arrange.
      List<UdalExtract> udalExtractList = [new()];
      string responseMock = JsonSerializer.Serialize(udalExtractList);

      _mockHttpMessageHandler
        .When(HttpMethod.Get, _udalExtractOptions.BiApi.Url)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, responseMock);

      Mock<IProcess> mockProcess = new();
      mockProcess
        .Setup(x => x.Start())
        .Returns(true);
      // This simulates the PIPE taking too long to exit.
      mockProcess
        .Setup(x => x.WaitForExit(FiveMinutes))
        .Returns(false);

      _mockProcessFactory
        .Setup(x => x.Create())
        .Returns(mockProcess.Object);

      // Act.
      Func<Task> act = _udalExtractService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<TimeoutException>()
        .WithMessage($"PIPE tool failed to exit*{_expectedProcessStartInfo.Arguments}*");
    }

    [Fact]
    public async Task Should_Throw_When_PipeStandardOutputDoesNotIndicateSuccess()
    {
      // Arrange.
      List<UdalExtract> udalExtractList = [new()];
      string responseMock = JsonSerializer.Serialize(udalExtractList);

      _mockHttpMessageHandler
        .When(HttpMethod.Get, _udalExtractOptions.BiApi.Url)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, responseMock);

      Mock<IProcess> mockProcess = new();
      mockProcess
        .Setup(x => x.Start())
        .Returns(true);
      mockProcess
        .Setup(x => x.WaitForExit(FiveMinutes))
        .Returns(true);
      // Simulate the standard output to not match the expected
      string expectedStandardOutput =
        $"{_udalExtractOptions.AgemPipe.OutputIndicatingSuccess}_NoMatchWithStandard";
      StreamReader streamReader = new(
        new MemoryStream(Encoding.UTF8.GetBytes(expectedStandardOutput)));
      mockProcess
        .Setup(x => x.StandardOutput)
        .Returns(streamReader);

      _mockProcessFactory
        .Setup(x => x.Create())
        .Returns(mockProcess.Object);

      // Act.
      Func<Task> act = _udalExtractService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage($"PIPE tool completed with invalid state*{expectedStandardOutput}*when*" +
          $"{_udalExtractOptions.AgemPipe.OutputIndicatingSuccess}*");
    }

    [Fact]
    public async Task Should_Throw_When_FileToEncryptIsKept()
    {
      // Arrange.
      _udalExtractOptions.AgemPipe.FileToEncryptKeepAfterEncryption = true;

      string expectedMessage =
        $"Process halted*'{_udalExtractOptions.AgemPipe.FileToEncryptFilePath}' was kept*" +
        $"'{nameof(_udalExtractOptions.AgemPipe.FileToEncryptKeepAfterEncryption)}'*true*" +
        $"Review files and then delete from '{_udalExtractOptions.AgemPipe.EncryptedFilePath}'.";

      List<UdalExtract> udalExtractList = [new()];
      string responseMock = JsonSerializer.Serialize(udalExtractList);

      _mockHttpMessageHandler
        .When(HttpMethod.Get, _udalExtractOptions.BiApi.Url)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, responseMock);

      MockFileData mockEncryptedFile = new("MockEncryptedFile");
      Mock<IProcess> mockProcess = new();
      mockProcess
        .Setup(x => x.Start())
        .Returns(true)
        // this callback creates the mock encrypted file produced by the PIPE application.
        .Callback(() => _mockFileSystem.AddFile(
          _udalExtractOptions.AgemPipe.EncryptedFilePath,
          mockEncryptedFile));
      mockProcess
        .Setup(x => x.WaitForExit(FiveMinutes))
        .Returns(true);
      using MemoryStream outputIndicatingSuccess = new(
        Encoding.UTF8.GetBytes(_udalExtractOptions.AgemPipe.OutputIndicatingSuccess));
      mockProcess
        .Setup(x => x.StandardOutput)
        .Returns(new StreamReader(outputIndicatingSuccess));

      _mockProcessFactory
        .Setup(x => x.Create())
        .Returns(mockProcess.Object);

      // Act.
      Func<Task> act = _udalExtractService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Fact]
    public async Task Should_Throw_When_PipeDoesNotCreateEncryptedFile()
    {
      // The test does not simulate the creation of the encrypted file so it should always throw.

      // Arrange.
      List<UdalExtract> udalExtractList = [new()];
      string responseMock = JsonSerializer.Serialize(udalExtractList);

      _mockHttpMessageHandler
        .When(HttpMethod.Get, _udalExtractOptions.BiApi.Url)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, responseMock);

      Mock<IProcess> mockProcess = new();
      mockProcess
        .Setup(x => x.Start())
        .Returns(true);
      mockProcess
        .Setup(x => x.WaitForExit(FiveMinutes))
        .Returns(true);
      using MemoryStream outputIndicatingSuccess = new(
        Encoding.UTF8.GetBytes(_udalExtractOptions.AgemPipe.OutputIndicatingSuccess));
      mockProcess
        .Setup(x => x.StandardOutput)
        .Returns(new StreamReader(outputIndicatingSuccess));

      _mockProcessFactory
        .Setup(x => x.Create())
        .Returns(mockProcess.Object);

      // Act.
      Func<Task> act = _udalExtractService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage($"PIPE tool failed to create encrypted file*" +
          $"{_udalExtractOptions.AgemPipe.EncryptedFilePath}*");
    }

    [Fact]
    public async Task Should_Throw_When_UdalExtractListIsNull()
    {
      // Arrange.
      string responseMock = "null";

      _mockHttpMessageHandler
        .When(HttpMethod.Get, _udalExtractOptions.BiApi.Url)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, responseMock);

      // Act.
      Func<Task> act = _udalExtractService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage($"Failed*_udalExtractList is null*");
    }
  }
}
