using System;
using Microsoft.AspNetCore.Mvc;
using WmsHub.Business;
using WmsHub.Business.Entities.ErsMock;
using WmsHub.ErsMock.Api.Models.Task;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WmsHub.ErsMock.Api.Controllers;

[ApiController]
[Route("ers-api/STU3/v1/[controller]")]
public class TaskController : ControllerBase
{
  private const string EXPECTED_INTENT = "proposal";
  private const string EXPECTED_STATUS = "ready";
  private readonly DatabaseContext _databaseContext;

  public TaskController(DatabaseContext databaseContext)
  {
    _databaseContext = databaseContext;
  }

  [HttpGet()]
  public async Task<IActionResult> A029AvailableActionsForUserList(
    [FromQuery] string focus,
    [FromQuery] string intent,
    [FromQuery] string status)
  {
    try
    {
      if (intent != EXPECTED_INTENT)
      {
        return Problem(
          detail:
            $"The {nameof(intent)} query parameter must be {EXPECTED_INTENT}",
          statusCode: StatusCodes.Status400BadRequest);
      }

      if (status != EXPECTED_STATUS)
      {
        return Problem(
          detail:
            $"The {nameof(status)} query parameter must be {EXPECTED_STATUS}",
          statusCode: StatusCodes.Status400BadRequest);
      }

      Focus focusParameters = new(focus);
      if (focusParameters.Version != "1")
      {
        return Problem(
          detail: "The version segment of the query parameter focus must be 1",
          statusCode: StatusCodes.Status400BadRequest);
      }

      ErsMockReferral? ersMockReferral = await _databaseContext.ErsMockReferrals
        .FindAsync(focusParameters.Ubrn);

      if (ersMockReferral == null)
      {
        return Problem(
          detail: "Unable to find a referral with the UBRN: " +
            $"{focusParameters.Ubrn}",
          statusCode: StatusCodes.Status404NotFound);
      }

      AvailableActions availableActions = new()
      {
        Entry = new List<AvailableActionEntry>()
        {
          new AvailableActionEntry()
          {
            Resource = new AvailableActionResource()
            {
              Code = new AvailableActionCode()
              {
                Coding = new List<AvailableActionCodingItem>()
                {
                  new AvailableActionCodingItem()
                  {
                    Code = "RECORD_REVIEW_OUTCOME"
                  }
                }
              }
            }
          }
        }
      };

      return Ok(availableActions);
    }
    catch (ArgumentException ex)
    {
      return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status400BadRequest);
    }
    catch (Exception ex)
    {
      return Problem(
        detail: $"Unknown failure: {ex.Message}",
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }
}
