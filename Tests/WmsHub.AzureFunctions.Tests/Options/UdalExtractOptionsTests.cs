using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using WmsHub.AzureFunctions.Options;
using WmsHub.Tests.Helper;
using static WmsHub.AzureFunctions.Options.UdalExtractOptions;

namespace WmsHub.AzureFunctions.Tests.Options;

public class UdalExtractOptionsTests : ABaseTests
{

  public class SectionKeyTests : UdalExtractOptionsTests
  {
    [Fact]
    public void Should_BeNameOfClass()
    {
      // Arrange.
      string expectedSectionKey = $"{nameof(UdalExtractOptions)}";

      // Act.
      string sectionKey = UdalExtractOptions.SectionKey;

      // Assert.
      sectionKey.Should().Be(expectedSectionKey);
    }
  }

  public class AgemPipeTests : UdalExtractOptionsTests
  {
    [Fact]    
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<UdalExtractOptions, RequiredAttribute>("AgemPipe").Should().BeTrue();
    }
  }

  public class AgemPipeOptionsTests : UdalExtractOptionsTests
  {
    public class CliPathTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<AgemPipeOptions, RequiredAttribute>("CliPath").Should().BeTrue();
      }
    }

    public class ColumnActionsTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_HaveDefaultValue()
      {
        // Arrange.
        AgemPipeOptions agemPipeOptions = new()
        { CliPath = "", ConfigName = "", ExtractFilePath = "", WorkingDirectory = "" };

        string expectedColumnActions = "0 pseudo";

        // Act.
        string columnActions = agemPipeOptions.ColumnActions;

        // Assert.
        columnActions.Should().Be(expectedColumnActions);
      }

      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<AgemPipeOptions, RequiredAttribute>("ColumnActions").Should().BeTrue();
      }
    }

    public class ConfigNameTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<AgemPipeOptions, RequiredAttribute>("ConfigName").Should().BeTrue();
      }
    }

    public class EncryptedFilenameTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_HaveDefaultValue()
      {
        // Arrange.
        AgemPipeOptions agemPipeOptions = new()
        { CliPath = "", ConfigName = "", ExtractFilePath = "", WorkingDirectory = "" };

        string expectedEncryptedFilenamePrefix = "dwmp_udal_enc_";
        DateTime expectedEncryptedFilenameDateTime = DateTime.Now;

        // Act.
        string encryptedFilename = agemPipeOptions.EncryptedFilename;

        // Assert.
        encryptedFilename[..expectedEncryptedFilenamePrefix.Length]
          .Should().Be(expectedEncryptedFilenamePrefix);

        DateTime.ParseExact(
          encryptedFilename[expectedEncryptedFilenamePrefix.Length..],
          "yyyyMMddHHmmss",
          CultureInfo.InvariantCulture)
          .Should().BeCloseTo(expectedEncryptedFilenameDateTime, new TimeSpan(0, 0, 5))
          .And.BeBefore(expectedEncryptedFilenameDateTime);
      }

      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<AgemPipeOptions, RequiredAttribute>("EncryptedFilename").Should().BeTrue();
      }
    }

    public class EncryptedFilePathTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_CombineExtractFilePathAndEncryptedFilename()
      {
        // Arrange.
        string encryptedFilename = "EncryptedFilenameTest";
        string extractFilePath = "c:\\test\\extractfilepath";
        string expectedEncryptedFilePath = $"{extractFilePath}\\{encryptedFilename}";

        AgemPipeOptions agemPipeOptions = new()
        {
          CliPath = "",
          ConfigName = "",
          EncryptedFilename = encryptedFilename,
          ExtractFilePath = extractFilePath,
          WorkingDirectory = ""
        };

        // Act.
        string encryptedFilePath = agemPipeOptions.EncryptedFilePath;

        // Assert.
        encryptedFilePath.Should().Be(expectedEncryptedFilePath);
      }
    }

    public class ExtractFilePathTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<AgemPipeOptions, RequiredAttribute>("ExtractFilePath").Should().BeTrue();
      }
    }

    public class FileToEncryptFilenameTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_HaveDefaultValue()
      {
        // Arrange.
        AgemPipeOptions agemPipeOptions = new()
        { CliPath = "", ConfigName = "", ExtractFilePath = "", WorkingDirectory = "" };

        string expectedFileToEncryptFilenamePrefix = "dwmp_udal_";
        DateTime expectedFileToEncryptFilenameDateTime = DateTime.Now;

        // Act.
        string FileToEncryptFilename = agemPipeOptions.FileToEncryptFilename;

        // Assert.
        FileToEncryptFilename[..expectedFileToEncryptFilenamePrefix.Length]
          .Should().Be(expectedFileToEncryptFilenamePrefix);

        DateTime.ParseExact(
          FileToEncryptFilename[expectedFileToEncryptFilenamePrefix.Length..],
          "yyyyMMddHHmmss",
          CultureInfo.InvariantCulture)
          .Should().BeCloseTo(expectedFileToEncryptFilenameDateTime, new TimeSpan(0, 0, 5))
          .And.BeBefore(expectedFileToEncryptFilenameDateTime);
      }

      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<AgemPipeOptions, RequiredAttribute>("FileToEncryptFilename").Should().BeTrue();
      }
    }

    public class FileToEncryptFilePathTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_CombineExtractFilePathAndFileToEncryptFilename()
      {
        // Arrange.
        string extractFilePath = "c:\\test\\extractfilepath";
        string fileToEncryptFilename = "FileToEncryptFilenameTest";
        string expectedFileToEncryptFilePath = $"{extractFilePath}\\{fileToEncryptFilename}";

        AgemPipeOptions agemPipeOptions = new()
        {
          CliPath = "",
          ConfigName = "",
          ExtractFilePath = extractFilePath,
          FileToEncryptFilename = fileToEncryptFilename,
          WorkingDirectory = ""
        };

        // Act.
        string fileToEncryptFilePath = agemPipeOptions.FileToEncryptFilePath;

        // Assert.
        fileToEncryptFilePath.Should().Be(expectedFileToEncryptFilePath);
      }
    }

    public class FileToEncryptKeepAfterEncryptionTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_HaveDefaultValue()
      {
        // Arrange.
        AgemPipeOptions agemPipeOptions = new()
        { CliPath = "", ConfigName = "", ExtractFilePath = "", WorkingDirectory = "" };

        bool expectedFileToEncryptKeepAfterEncryption = false;

        // Act.
        bool fileToEncryptKeepAfterEncryption = agemPipeOptions.FileToEncryptKeepAfterEncryption;

        // Assert.
        fileToEncryptKeepAfterEncryption.Should().Be(expectedFileToEncryptKeepAfterEncryption);
      }
    }

    public class OutputIndicatingSuccessTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_HaveDefaultValue()
      {
        // Arrange.
        AgemPipeOptions agemPipeOptions = new()
        { CliPath = "", ConfigName = "", ExtractFilePath = "", WorkingDirectory = "" };

        string expectedOutputIndicatingSuccess = "Processing\r\nComplete\r\n";

        // Act.
        string outputIndicatingSuccess = agemPipeOptions.OutputIndicatingSuccess;

        // Assert.
        outputIndicatingSuccess.Should().Be(expectedOutputIndicatingSuccess);
      }

      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<AgemPipeOptions, RequiredAttribute>("OutputIndicatingSuccess").Should().BeTrue();
      }
    }

    public class WorkingDirectoryTests : AgemPipeOptionsTests
    {
      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<AgemPipeOptions, RequiredAttribute>("WorkingDirectory").Should().BeTrue();
      }
    }
  }

  public class BiApiTests : UdalExtractOptionsTests
  {
    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<UdalExtractOptions, RequiredAttribute>("BiApi").Should().BeTrue();
    }
  }

  public class BiApiOptionsTests : UdalExtractOptionsTests
  {
    public class ApiKeyTests : BiApiOptionsTests
    {
      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<BiApiOptions, RequiredAttribute>("ApiKey").Should().BeTrue();
      }
    }

    public class FromTests : BiApiOptionsTests
    {
      [Fact]
      public void Should_HaveADefaultValue()
      {
        // Arrange.
        BiApiOptions biApiOptions = new()
        { ApiKey = "", Url = "" };

        DateTime? expectedFrom = DateTime.Now.Date.AddDays(-8);

        // Act.
        DateTime? from = biApiOptions.From;

        // Assert.
        from.Should().Be(expectedFrom);
      }
    }

    public class RequestUriTests : BiApiOptionsTests
    {
      public static TheoryData<DateTime?, DateTime?, string, string> FromToAndUrlExpectedData()
      {
        TheoryData<DateTime?, DateTime?, string, string> data = [];

        // both from and to dates provided
        data.Add(
          new DateTime(2001, 02, 03, 04, 05, 06),
          new DateTime(2007, 08, 09, 10, 11, 12),
          "https://requesturi.unit.test",
          "https://requesturi.unit.test/?fromDate=2001-02-03T04%3A05%3A06&toDate=2007-08-09T10%3A11%3A12");

        // only from date provided
        data.Add(
          new DateTime(2001, 02, 03, 04, 05, 06),
          null,
          "https://requesturi.unit.test",
          "https://requesturi.unit.test/?fromDate=2001-02-03T04%3A05%3A06");

        // only to date provided
        data.Add(
          null,
          new DateTime(2007, 08, 09, 10, 11, 12),
          "https://requesturi.unit.test",
          "https://requesturi.unit.test/?toDate=2007-08-09T10%3A11%3A12");

        // neither from or to date provided
        data.Add(
          null,
          null,
          "https://requesturi.unit.test",
          "https://requesturi.unit.test/");

        return data;
      }

      [Theory]
      [MemberData(nameof(FromToAndUrlExpectedData))]
      public void Should_BuildUsingFromToAndUrlProperties(
        DateTime? from,
        DateTime? to,
        string url,
        string expectedRequestUri)
      {
        // Arrange.
        BiApiOptions biApiOptions = new()
        { ApiKey = "", From = from, To = to, Url = url };

        // Act.
        Uri requestUri = biApiOptions.RequestUri;

        // Assert.
        requestUri.ToString().Should().Be(expectedRequestUri);
      }
    }

    public class ToTests : BiApiOptionsTests
    {
      [Fact]
      public void Should_HaveADefaultValue()
      {
        // Arrange.
        BiApiOptions biApiOptions = new()
        { ApiKey = "", Url = "" };

        DateTime? expectedTo = DateTime.Now.Date.AddDays(1);

        // Act.
        DateTime? to = biApiOptions.To;

        // Assert.
        to.Should().Be(expectedTo);
      }
    }

    public class UrlTests : BiApiOptionsTests
    {
      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<BiApiOptions, RequiredAttribute>("Url").Should().BeTrue();
      }
    }
  }

  public class MeshMailboxApiTests : UdalExtractOptionsTests
  {
    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<UdalExtractOptions, RequiredAttribute>("MeshMailboxApi").Should().BeTrue();
    }
  }

  public class MeshMailboxApiOptionsTests : UdalExtractOptionsTests
  {
    public class CertificateNameTests : BiApiOptionsTests
    {
      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<MeshMailboxApiOptions, RequiredAttribute>("CertificateName").Should().BeTrue();
      }
    }

    public class KeyVaultUrlTests : BiApiOptionsTests
    {
      [Fact]
      public void Should_HaveRequiredAttribute()
      {
        // Arrange. Act. and Assert.
        HasAttribute<MeshMailboxApiOptions, RequiredAttribute>("KeyVaultUrl").Should().BeTrue();
      }
    }
  }
}
