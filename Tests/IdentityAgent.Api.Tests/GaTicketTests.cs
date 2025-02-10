using FluentAssertions;
using IdentityAgentApi;
using Xunit;

namespace IdentityAgent.Api.Tests;
public class GaTicketTests
{
  public class ReturnCodeAsStringTests : GaTicketTests
  {
    [Fact]
    public void KnownReturnCode_ReturnCodeMessage()
    {
      // Arrange.
      int unknownReturnCode = 0;
      string expectedMessage = $"Return Code: 'TCK_API_SUCCESS'.";

      // Act.
      string result = GaTicket.ReturnCodeAsString(unknownReturnCode);

      // Assert.
      result.Should().Be(expectedMessage);
    }

    [Fact]
    public void UnknownReturnCode_NotProcessedMessage()
    {
      // Arrange.
      int unknownReturnCode = -1;
      string expectedMessage = $"Not Processed: Unknown Return Code '{unknownReturnCode}'.";

      // Act.
      string result = GaTicket.ReturnCodeAsString(unknownReturnCode);

      // Assert.
      result.Should().Be(expectedMessage);
    }
  }
}
