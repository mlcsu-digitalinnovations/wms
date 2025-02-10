using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using WmsHub.Business;
using WmsHub.Business.Entities.ErsMock;
using WmsHub.Common.Models;
using WmsHub.ErsMock.Api.Models.ReferralRequest;

namespace WmsHub.ErsMock.Api.Controllers;

[ApiController]
[Route("ers-api/STU3/v1/[controller]")]
public class ReferralRequestController : ControllerBase
{
  DatabaseContext _databaseContext;

  public ReferralRequestController(DatabaseContext databaseContext)
  {
    _databaseContext = databaseContext;
  }

  [HttpPost("$ers.fetchworklist")]
  public IActionResult A008RetrieveWorklist(FetchWorklistRequest request)
  {
    ErsWorkList ersWorklist = new();

    List<ErsWorkListEntry> entries = new();

    foreach (ErsMockReferral ersMockReferral in _databaseContext
      .ErsMockReferrals
      .Where(x => x.IsActive)
      .Where(x => x.IsTriaged != true)
      .Where(x => x.ServiceId == request.ServiceId)
      .ToList())
    {
      entries.Add(ErsWorkListEntryHelper.CreateErsWorkListEntry(
        ersMockReferral.NhsNumber,
        ersMockReferral.Ubrn));
    }

    ersWorklist.Entry = entries.ToArray();

    return Ok(ersWorklist);
  }

  [HttpPost("{ubrn}/$ers.generateCRI")]
  public IActionResult A007RetrieveClinicalInformation()
  {
    return NotFound();
  }

  [HttpGet("{ubrn}")]
  public async Task<IActionResult> A005RetrieveReferralRequest(
    [FromRoute] string ubrn)
  {
    if (ubrn == Constants.UbrnA005Returns401)
    {
      return Unauthorized();
    }

    ErsMockReferral? referral = await _databaseContext
      .ErsMockReferrals
      .Where(x => x.IsActive)
      .SingleOrDefaultAsync(x => x.Ubrn == ubrn);

    if (referral == null)
    {
      return NotFound();
    }
    else
    {
      ErsReferral ersReferral = new()
      {
        Id = $"{referral.Ubrn}",
        Meta = new()
        {
          VersionId = "1"
        }
      };

      if (referral.AttachmentId != null)
      {
        ersReferral.Attachments = new()
        {
          new ErsAttachment()
          {
            Creation = referral.Creation!.Value.Date,
            Id = referral.AttachmentId,
            Title = $"{referral.AttachmentId}.{referral.FileExtension}",
            Url = $"attachment/{referral.AttachmentId}"
          }
        };
      }

      return Ok(ersReferral);
    }
  }

  [HttpPost("{ubrn}/$ers.recordReviewOutcome")]
  public async Task<IActionResult> A028RecordReviewOutcome(
    [FromRoute] string ubrn,
    [FromBody] RecordReviewOutcomeRequest request)
  {
    ErsMockReferral? referral = await _databaseContext
      .ErsMockReferrals
      .FindAsync(ubrn);

    if (referral == null)
    {
      return NotFound();
    }
    else
    {
      if (request.IsValid())
      {
        referral.ReviewOutcome = request.ReviewOutcome;
        referral.IsTriaged = true;
        await _databaseContext.SaveChangesAsync();
        return Ok();
      }
      else
      {
        return BadRequest(request.Errors);
      }
    }
  }
}
