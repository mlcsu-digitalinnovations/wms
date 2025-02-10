using FluentAssertions;
using Microsoft.Data.SqlClient;
using WmsHub.AzureFunctions.Factories;

namespace WmsHub.AzureFunctions.Tests.Factories;

public class SqlConnectionFactoryTests
{
  public class CreateTests : SqlConnectionFactoryTests
  {
    [Fact]
    public void Should_CreateSqlConnection_When_ValidConnectionStringIsProvided()
    {
      // Arrange.
      string connectionString = "Data Source=server;Initial Catalog=db;Integrated Security=True";
      SqlConnectionFactory factory = new();

      // Act.
      SqlConnection sqlConnection = factory.Create(connectionString);

      // Assert.
      sqlConnection.Should().NotBeNull();
      sqlConnection.ConnectionString.Should().Be(connectionString);
    }
  }
}
