using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace IdentityAgentApi.Tests
{
  public class IdentityAgentTests
  {

    public IdentityAgentTests(ITestOutputHelper output)
    {
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Console()
        .WriteTo.Debug()
        .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Information)
        .CreateLogger();
    }

    // DO NOT DELETE

    // ONLY ENABLE ON A MACHINE WITH THE HSCIC IDENTITY AGENT INSTALLED
    // AND A VALID SMARTCARD INSTALLED

    //[Fact]
    //public void GetTicket_Test()
    //{
    //  // arrage
    //  var gaTicket = new GaTicket(Log.Logger);

    //  // act
    //  var ticket = gaTicket.GetTicket();

    //  // assert
    //  Assert.IsType<string>(ticket);
    //  Assert.NotEqual(string.Empty, ticket);
    //  Log.Information($"Ticket: {ticket}");
    //}
  }
}
