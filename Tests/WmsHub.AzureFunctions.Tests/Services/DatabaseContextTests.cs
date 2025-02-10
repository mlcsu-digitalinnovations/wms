using FluentAssertions;
using Microsoft.Data.SqlClient;
using Moq;
using System.Data;
using WmsHub.AzureFunctions.Factories;
using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions.Tests.Services;

public class DatabaseContextTests
{
  public class OpenConnectionAsyncTests : DatabaseContextTests
  {
    [Fact]
    public async Task Should_OpenConnection_When_ValidConnectionStringIsProvided()
    {
      // Arrange.
      string connectionString = "Data Source=server;Initial Catalog=db;Integrated Security=True";
      SqlConnection sqlConnection = new(connectionString);
      Mock<ISqlConnectionFactory> mockSqlConnectionFactory = new();
      mockSqlConnectionFactory
        .Setup(f => f.Create(It.IsAny<string>()))
        .Returns(sqlConnection);
      mockSqlConnectionFactory
        .Setup(f => f.OpenAsync(It.IsAny<SqlConnection>()))
        .Returns(Task.CompletedTask);

      DatabaseContext dbContext = new(mockSqlConnectionFactory.Object);

      // Act.
      await dbContext.OpenConnectionAsync(connectionString);

      // Assert.
      mockSqlConnectionFactory.Verify(f => f.Create(connectionString), Times.Once);
      mockSqlConnectionFactory.Verify(f => f.OpenAsync(sqlConnection), Times.Once);
    }
  }

  public class CreateCommandTests : DatabaseContextTests
  {
    [Fact]
    public async Task Should_CreateCommand_When_ConnectionIsOpen()
    {
      // Arrange.
      string connectionString = "Data Source=server;Initial Catalog=db;Integrated Security=True";
      SqlConnection expectedSqlConnection = new(connectionString);
      
      Mock<ISqlConnectionFactory> mockSqlConnectionFactory = new();
      mockSqlConnectionFactory
        .Setup(f => f.Create(It.IsAny<string>()))
        .Returns(expectedSqlConnection);
      mockSqlConnectionFactory
        .Setup(f => f.OpenAsync(It.IsAny<SqlConnection>()))
        .Returns(Task.CompletedTask);

      DatabaseContext dbContext = new(mockSqlConnectionFactory.Object);
      await dbContext.OpenConnectionAsync(connectionString);

      string expectedCommandText = "SELECT * FROM Users";
      CommandType expectedCommandType = CommandType.Text;
      int expectedCommandTimeout = 10800;

      // Act.
      IDbCommand command = dbContext.CreateCommand(expectedCommandText, expectedCommandType);

      // Assert.
      command.CommandText.Should().Be(expectedCommandText);
      command.CommandType.Should().Be(expectedCommandType);
      command.CommandTimeout.Should().Be(expectedCommandTimeout);
      command.Connection.Should().Be(expectedSqlConnection);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_ConnectionIsNotOpen()
    {
      // Arrange.
      Mock<ISqlConnectionFactory> mockSqlConnectionFactory = new();
      DatabaseContext dbContext = new(mockSqlConnectionFactory.Object);
      string commandText = "SELECT * FROM Users";
      CommandType expectedCommandType = CommandType.Text;

      // Act.
      Func<IDbCommand> act = () => dbContext.CreateCommand(commandText, expectedCommandType);

      // Assert.
      act.Should().Throw<InvalidOperationException>().WithMessage("Connection is not open.");
    }
  }
}
