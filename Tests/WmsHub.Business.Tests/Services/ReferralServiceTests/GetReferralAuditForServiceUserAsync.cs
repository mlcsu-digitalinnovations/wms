using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;
public partial class ReferralServiceTests : ServiceTestsBase
{
  public class GetReferralAuditForServiceUserAsync :
    ReferralServiceTests,
    IDisposable
  {
    public GetReferralAuditForServiceUserAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
    {
      CleanUp();
    }

    public new void Dispose()
    {
      CleanUp();
      GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ModifiedById_GuidEmpty_Username_Is_Unknown()
    {
      // Arrange.
      Entities.UserStore user = RandomEntityCreator.CreateUserStore();
      _context.UsersStore.Add(user);
      await _context.SaveChangesAsync();

      Guid referralId = Guid.NewGuid();
      List<ReferralStatus> statuses = new()
      {
        ReferralStatus.New,
        ReferralStatus.TextMessage1,
        ReferralStatus.RmcCall,
        ReferralStatus.ChatBotCall1
      };

      Entities.TextMessage textMessage = RandomEntityCreator
        .CreateRandomTextMessage(referralId: referralId);
      _context.TextMessages.Add(textMessage);
      await _context.SaveChangesAsync();

      int auditIdEmpty =
        await AddAuditAsync(Guid.Empty, referralId, statuses[0]);
      int auditIdServiceUser =
        await AddAuditAsync(textMessage.Id, referralId, statuses[1]);
      int auditIdRmcUser =
        await AddAuditAsync(user.Id, referralId, statuses[2]);
      int auditIdAutomated =
        await AddAuditAsync(Guid.NewGuid(), referralId, statuses[3]);

      // Act.
      List<ReferralAudit> response =
        await _service.GetReferralAuditForServiceUserAsync(referralId);

      // Assert.
      using (new AssertionScope())
      {
        ReferralAudit auditEmpty = response
          .SingleOrDefault(t => t.AuditId == auditIdEmpty);
        ReferralAudit auditServiceUser = response
          .SingleOrDefault(t => t.AuditId == auditIdServiceUser);
        ReferralAudit auditRmcUser = response
          .SingleOrDefault(t => t.AuditId == auditIdRmcUser);
        ReferralAudit auditAutomated = response
          .SingleOrDefault(t => t.AuditId == auditIdAutomated);

        auditEmpty.Username.Should().Be(Constants.WebUi.RMC_UNKNOWN);
        auditServiceUser.Username.Should()
          .Be(Constants.WebUi.RMC_SERVICE_USER);
        auditRmcUser.Username.Should().Be(user.OwnerName);
        auditAutomated.Username.Should().Be(Constants.WebUi.RMC_AUTOMATED);
      }
    }

    private void CleanUp()
    {
      _context.ReferralsAudit.RemoveRange(_context.ReferralsAudit);
      _context.SaveChanges();
    }

    private async Task<int> AddAuditAsync(
      Guid modifiedByUserId,
      Guid referralId,
      ReferralStatus status)
    {
      Entities.ReferralAudit audit = RandomEntityCreator
        .CreateRandomReferralAudit(
          id: referralId,
          modifiedByUserId: modifiedByUserId,
          status: status
      );

      if (modifiedByUserId != Guid.Empty)
      {
        Entities.UserStore user = await _context.UsersStore
          .SingleOrDefaultAsync(s => s.Id == modifiedByUserId);

        if (user != null)
        {
          audit.User = user;
        }
      }

      _context.ReferralsAudit.Add(audit);
      await _context.SaveChangesAsync();

      return audit.AuditId;
    }
  }
}
