using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using WmsHub.AzureFunctions.Options;
using WmsHub.Tests.Helper;

namespace WmsHub.AzureFunctions.Tests.Options;
public class SqlMaintenanceOptionsTests : ABaseTests
{

  public class SectionKeyTests : SqlMaintenanceOptionsTests
  {
    [Fact]
    public void Should_BeNameOfClass()
    {
      // Arrange.
      string expectedSectionKey = $"{nameof(SqlMaintenanceOptions)}";

      // Act.
      string sectionKey = SqlMaintenanceOptions.SectionKey;

      // Assert.
      sectionKey.Should().Be(expectedSectionKey);
    }
  }

  public class ConnectionStringTests : SqlMaintenanceOptionsTests
  {

    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<SqlMaintenanceOptions, RequiredAttribute>("ConnectionString")
        .Should().BeTrue();
    }
  }

  public class StoredProcedureNameTests : SqlMaintenanceOptionsTests
  {
    [Fact]
    public void Should_HaveDefaultValue()
    {
      // Arrange.
      string expectedStoredProcedureName = "dbo.usp_AzureSQLMaintenance";
      SqlMaintenanceOptions options = new() { ConnectionString = "" };

      // Act.
      string storedProcedureName = options.StoredProcedureName;

      // Assert.
      storedProcedureName.Should().Be(expectedStoredProcedureName);
    }

    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<SqlMaintenanceOptions, RequiredAttribute>("StoredProcedureName")
        .Should().BeTrue();
    }
  }

  public class SuccessResultTests : SqlMaintenanceOptionsTests
  {
    [Fact]
    public void Should_HaveDefaultValue()
    {
      // Arrange.
      int expectedSuccessResult = -1;
      SqlMaintenanceOptions options = new() { ConnectionString = "" };

      // Act.
      int SuccessResult = options.SuccessResult;

      // Assert.
      SuccessResult.Should().Be(expectedSuccessResult);
    }

    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<SqlMaintenanceOptions, RequiredAttribute>("SuccessResult")
        .Should().BeTrue();
    }
  }
}
