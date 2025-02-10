using FluentAssertions;
using Moq;
using System.Data;
using WmsHub.AzureFunctions.Options;
using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions.Tests.Services;

public class SqlMaintenanceServiceTests
{
  private const string MockConnectionString =
    "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;";
  private const string MockStoredProcedureName = "MockStoredProcedure";
  private const int SuccessResult = -1;
  private const int UnsuccessfulResult = 0;

  private readonly SqlMaintenanceOptions _options = new()
  {
    ConnectionString = MockConnectionString,
    StoredProcedureName = MockStoredProcedureName,
    SuccessResult = SuccessResult
  };

  public class ProcessAsync : SqlMaintenanceServiceTests
  {

    [Fact]
    public async Task When_StoredProcedureReturnsExpectedValue_ReturnsSuccessMessage()
    {
      // Arrange.
      Mock<IDbCommand> mockCommand = new();
      mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(SuccessResult);

      Mock<IDatabaseContext> mockDbContext = new();
      mockDbContext
        .Setup(ctx => ctx.OpenConnectionAsync(MockConnectionString))
        .Returns(Task.CompletedTask);
      mockDbContext
        .Setup(ctx => ctx.CreateCommand(
          MockStoredProcedureName, 
          CommandType.StoredProcedure, 
          10800))
        .Returns(mockCommand.Object);

      SqlMaintenanceService service = new(
        mockDbContext.Object,
        Microsoft.Extensions.Options.Options.Create(_options));

      // Act.
      string result = await service.ProcessAsync();

      // Assert.
      result.Should().Be($"StoredProcedure {MockStoredProcedureName} completed successfully. " +
        "Review dbo.AzureSQLMaintenanceLog for details.");
      mockCommand.Verify(cmd => cmd.ExecuteNonQuery(), Times.Once);
      mockDbContext.Verify(ctx => ctx.OpenConnectionAsync(MockConnectionString), Times.Once);
    }

    [Fact]
    public async Task When_StoredProcedureReturnsUnsuccessfulResult_ThrowsInvalidOperationException()
    {
      // Arrange.
      Mock<IDbCommand> mockCommand = new();
      mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(UnsuccessfulResult);

      Mock<IDatabaseContext> mockDbContext = new();
      mockDbContext
        .Setup(ctx => ctx.OpenConnectionAsync(MockConnectionString))
        .Returns(Task.CompletedTask);
      mockDbContext
        .Setup(ctx => ctx.CreateCommand(
          MockStoredProcedureName, 
          CommandType.StoredProcedure, 
          10800))
        .Returns(mockCommand.Object);

      SqlMaintenanceService service = new(
        mockDbContext.Object,
        Microsoft.Extensions.Options.Options.Create(_options));

      // Act.
      Func<Task> act = service.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<InvalidOperationException>()
          .WithMessage($"StoredProcedure {MockStoredProcedureName} returned unexpected result 0. " +
            $"Review dbo.AzureSQLMaintenanceLog for details.");

      mockCommand.Verify(cmd => cmd.ExecuteNonQuery(), Times.Once);
      mockDbContext.Verify(ctx => ctx.OpenConnectionAsync(MockConnectionString), Times.Once);
    }
  }
}
