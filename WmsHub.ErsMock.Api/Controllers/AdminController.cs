using Microsoft.AspNetCore.Mvc;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Business.Entities.ErsMock;
using WmsHub.Business.Helpers;

namespace WmsHub.ErsMock.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase
{
  private const string NhsNumberIneligible = "9991379991";
  private const string NhsNumberIneligiblePreviouslyCancelled = "9995749998";
  private const string NhsNumberMissing = "9999089992";
  private const string NhsNumberNhsNumberMismatch = "9996679993";
  private const string NhsNumberValid = "9991769994";
  private const string NhsNumberValidSexNotKnown = "9993930237";
  private const string NhsNumberValidSexNotSpecified = "9998226473";

  private const string NhsNumberUpdateIneligible = "9995909995"; 
  private const string NhsNumberUpdateMissing = "9999499996";
  private const string NhsNumberUpdateNhsNumberMismatch = "9994919997";

  private const string ServiceId1 = "6708611";
  private const string ServiceId2 = "6708596";

  private const string UbrnIneligible = "670861100001";
  private const string UbrnIneligiblePreviouslyCancelled = "670861100008";
  private const string UbrnMissing = "670861100002";
  private const string UbrnNhsNumberMismatch = "670861100003";
  private const string UbrnValid = "670861100004";
  private const string UbrnValidSexNotKnown = "670861100009";
  private const string UbrnValidSexNotSpecified = "670861100010";

  private const string UbrnUpdateIneligible = "670861100005";  
  private const string UbrnUpdateMissing = "670861100006";
  private const string UbrnUpdateNhsNumberMismatch = "670861100007";

  private readonly DatabaseContext _databaseContext;

  public AdminController(DatabaseContext databaseContext)
  {
    _databaseContext = databaseContext;
  }

  [HttpPost("ResetTestReferrals")]
  public async Task<IActionResult> ResetTestReferrals()
  {
    _databaseContext.ErsMockReferrals.RemoveRange(_databaseContext.ErsMockReferrals);
    _databaseContext.Referrals.RemoveRange(_databaseContext.Referrals);

    await _databaseContext.SaveChangesAsync();

    _databaseContext.ErsMockReferrals.Add(new()
    {
      AttachmentId = GetAttachmentId(UbrnIneligible),
      Creation = DateTimeOffset.Now.AddDays(-2),
      Description = "Create Ineligible",
      FileExtension = "pdf",
      IsActive = true,
      IsTriaged = false,
      NhsNumber = NhsNumberIneligible,
      ReviewOutcome = null,
      ServiceId = ServiceId2,
      Ubrn = UbrnIneligible
    });

    ErsMockReferral createIneligiblePreviouslyCancelled = new()
    {
      AttachmentId = GetAttachmentId(UbrnIneligiblePreviouslyCancelled),
      Creation = DateTimeOffset.Now.AddDays(-2),
      Description = "Create Ineligible Previously Cancelled",
      FileExtension = "pdf",
      IsActive = true,
      IsTriaged = false,
      NhsNumber = NhsNumberIneligiblePreviouslyCancelled,
      ReviewOutcome = null,
      ServiceId = ServiceId2,
      Ubrn = UbrnIneligiblePreviouslyCancelled
    };
    _databaseContext.ErsMockReferrals.Add(createIneligiblePreviouslyCancelled);

    // create an existing referral that is in a state ready to be updated.
    _databaseContext.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
      referralAttachmentId: createIneligiblePreviouslyCancelled.AttachmentId,
      mostRecentAttachmentDate: createIneligiblePreviouslyCancelled.Creation.Value.AddDays(-1),
      nhsNumber: createIneligiblePreviouslyCancelled.NhsNumber,
      serviceId: createIneligiblePreviouslyCancelled.ServiceId,
      status: Business.Enums.ReferralStatus.CancelledByEreferrals,
      statusReason: "Previous cancelled waiting to be created again with ineligible",
      ubrn: createIneligiblePreviouslyCancelled.Ubrn));

    _databaseContext.ErsMockReferrals.Add(new()
    {
      AttachmentId = null,
      Creation = null,
      Description = "Create Missing",
      FileExtension = "pdf",
      IsActive = true,
      IsTriaged = false,
      NhsNumber = NhsNumberMissing,
      ReviewOutcome = null,
      ServiceId = ServiceId2,
      Ubrn = UbrnMissing
    });

    _databaseContext.ErsMockReferrals.Add(new()
    {
      AttachmentId = GetAttachmentId(UbrnNhsNumberMismatch),
      Creation = DateTimeOffset.Now.AddDays(-2),
      Description = "Create NHS Number Mismatch",
      FileExtension = "pdf",
      IsActive = true,
      IsTriaged = false,
      NhsNumber = NhsNumberNhsNumberMismatch,
      ReviewOutcome = null,
      ServiceId = ServiceId1,
      Ubrn = UbrnNhsNumberMismatch
    });

    _databaseContext.ErsMockReferrals.Add(new()
    {
      AttachmentId = GetAttachmentId(UbrnValid),
      Creation = DateTimeOffset.Now.AddDays(-2),
      Description = "Create Valid",
      FileExtension = "pdf",
      IsActive = true,
      IsTriaged = false,
      NhsNumber = NhsNumberValid,
      ReviewOutcome = null,
      ServiceId = ServiceId1,
      Ubrn = UbrnValid
    });

    _databaseContext.ErsMockReferrals.Add(new()
    {
      AttachmentId = GetAttachmentId(UbrnValidSexNotKnown),
      Creation = DateTimeOffset.Now.AddDays(-2),
      Description = "Create Valid Sex Not Known",
      FileExtension = "pdf",
      IsActive = true,
      IsTriaged = false,
      NhsNumber = NhsNumberValidSexNotKnown,
      ReviewOutcome = null,
      ServiceId = ServiceId1,
      Ubrn = UbrnValidSexNotKnown
    });

    _databaseContext.ErsMockReferrals.Add(new()
    {
      AttachmentId = GetAttachmentId(UbrnValidSexNotSpecified),
      Creation = DateTimeOffset.Now.AddDays(-2),
      Description = "Create Valid Sex Not Specified",
      FileExtension = "pdf",
      IsActive = true,
      IsTriaged = false,
      NhsNumber = NhsNumberValidSexNotSpecified,
      ReviewOutcome = null,
      ServiceId = ServiceId1,
      Ubrn = UbrnValidSexNotSpecified
    });

    ErsMockReferral updateIneligible = new()
    {
      AttachmentId = GetAttachmentId(UbrnUpdateIneligible),
      Creation = DateTimeOffset.Now.AddDays(-2),
      Description = "Update Ineligible",
      FileExtension = "pdf",
      IsActive = true,
      IsTriaged = false,
      NhsNumber = NhsNumberUpdateIneligible,
      ReviewOutcome = null,
      ServiceId = ServiceId2,
      Ubrn = UbrnUpdateIneligible
    };
    _databaseContext.ErsMockReferrals.Add(updateIneligible);

    // create an existing referral that is in a state ready to be updated.
    _databaseContext.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
      referralAttachmentId: updateIneligible.AttachmentId,
      mostRecentAttachmentDate: updateIneligible.Creation.Value.AddDays(-1),
      nhsNumber: updateIneligible.NhsNumber,
      serviceId: updateIneligible.ServiceId,
      status: Business.Enums.ReferralStatus.RejectedToEreferrals,
      statusReason: "Ineligible waiting to be updated",
      ubrn: updateIneligible.Ubrn));

    ErsMockReferral updateMissing = new()
    {
      AttachmentId = GetAttachmentId(UbrnUpdateMissing),
      Creation = DateTimeOffset.Now.AddDays(-2),
      Description = "Update Missing",
      FileExtension = "pdf",
      IsActive = true,
      IsTriaged = false,
      NhsNumber = NhsNumberUpdateMissing,
      ReviewOutcome = null,
      ServiceId = ServiceId2,
      Ubrn = UbrnUpdateMissing
    };
    _databaseContext.ErsMockReferrals.Add(updateMissing);

    // create an existing referral that is in a state ready to be updated.
    Referral missingEntity = RandomEntityCreator.CreateRandomReferral(
      referralAttachmentId: null,
      mostRecentAttachmentDate: null,
      nhsNumber: null,
      serviceId: updateMissing.ServiceId,
      status: Business.Enums.ReferralStatus.RejectedToEreferrals,
      statusReason: "Missing waiting to be updated",
      ubrn: updateMissing.Ubrn);
    missingEntity.MostRecentAttachmentDate = null;
    _databaseContext.Referrals.Add(missingEntity);

    ErsMockReferral updateNhsNumberMatchMatch = new()
    {
      AttachmentId = GetAttachmentId(UbrnUpdateNhsNumberMismatch),
      Creation = DateTimeOffset.Now.AddDays(-1),
      Description = "Update referral letter NHS number does not match eRS work list",
      FileExtension = "pdf",
      IsActive = true,
      IsTriaged = false,
      NhsNumber = NhsNumberUpdateNhsNumberMismatch,
      ReviewOutcome = null,
      ServiceId = ServiceId2,
      Ubrn = UbrnUpdateNhsNumberMismatch
    };
    _databaseContext.ErsMockReferrals.Add(updateNhsNumberMatchMatch);

    // create an existing referral that is in a state ready to be updated.
    _databaseContext.Referrals.Add(RandomEntityCreator.CreateRandomReferral(
      referralAttachmentId: updateNhsNumberMatchMatch.AttachmentId,
      mostRecentAttachmentDate: updateNhsNumberMatchMatch.Creation.Value.AddDays(-2),
      nhsNumber: updateNhsNumberMatchMatch.NhsNumber,
      serviceId: updateNhsNumberMatchMatch.ServiceId,
      status: Business.Enums.ReferralStatus.RejectedToEreferrals,
      statusReason: "NHS number mismatch waiting to be updated",
      ubrn: updateNhsNumberMatchMatch.Ubrn));

    await _databaseContext.SaveChangesAsync();

    return Ok();
  }

  private static string GetAttachmentId(string ubrn)
  {
    return $"00000000-0000-0000-0000-{ubrn}";
  }
}
