using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.ElectiveCareReferral;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Models.ElectiveCareReferral;

namespace WmsHub.Referral.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Authorize(Policy = AuthPolicies.ElectiveCare.POLICY_NAME)]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
public class ElectiveCareUserManagementController : BaseController
{
  private readonly ILogger _log;
  private readonly IMapper _mapper;

  public ElectiveCareUserManagementController(
    IElectiveCareReferralService electiveCareReferralService,
    ILogger logger,
    IMapper mapper)
    : base(electiveCareReferralService)
  {
    _log = logger.ForContext<ElectiveCareUserManagementController>();
    _mapper = mapper;
  }

  [HttpPost]
  [DisableRequestSizeLimit]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> UploadFile(IFormFile file)
  {
    if (file == null || file.Length == 0)
    {
      return BadRequest("File has not been provided or empty.");
    }

    string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

    try
    {
      if (!Constants.ALLOWED_SPREADSHEET_EXTENSIONS.Contains(fileExtension))
      {
        throw new InvalidOperationException("Filetype not supported.");
      }

      UserManagementPostRequest request = new(file);

      string detail = BuildDetailMessage(request);

      if (!request.AllRows.Any() 
        || (!request.AllRows.Any(x => 
         x.ActionType == UserManagementPostRequest.ActionType.Delete 
         || x.ActionType == UserManagementPostRequest.ActionType.Create))
       )
      {
        detail += " There are no rows that contain users. Please correct " +
          "the errors and re-upload the file.";

        _log.Debug(detail);

        return Problem(
          detail: detail,
          statusCode: StatusCodes.Status400BadRequest);
      }

      IEnumerable<ElectiveCareUserData> data = _mapper
        .Map<IEnumerable<ElectiveCareUserData>>(
          request.AllRows
          .Where(r => 
            r.ActionType == UserManagementPostRequest.ActionType.Create 
            || r.ActionType == UserManagementPostRequest.ActionType.Delete)
          .ToList());

      ElectiveCareUserManagementResponse response =
        await Service.ManageUsersUsingUploadAsync(data);

      return Ok(response);
    }
    catch (Exception ex)
    {
      if (ex is MsGraphBearerTokenRequestFailureException)
      {
        _log.Error("Post failed: {ExceptionMessage}", ex.Message);
        return Problem("Request could not be processed, " +
          "unable to access the Microsoft Graph service.",
          statusCode: StatusCodes.Status500InternalServerError);
      }
      else
      {
        _log.Error(ex, "Post Failed.");
        return Problem(ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }
  }

  private static string BuildDetailMessage(
    UserManagementPostRequest request)
  {
    string detail = $"Found {request.AllRows.Count} row(s).";

    if (request.HasEmptyRows)
    {
      detail += $" Ignored {request.EmptyRowsCount} empty row(s).";
    }

    if (request.HasHeaderRow)
    {
      detail += $" Found a valid header in the first row.";
    }

    return detail;
  }

  private IElectiveCareReferralService Service
  {
    get
    {
      IElectiveCareReferralService service =
        _service as IElectiveCareReferralService;
      service.User = User;
      return service;
    }
  }
}
