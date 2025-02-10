using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.BusinessIntelligence.Api.Models;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Api.Models;
using BusinessModels = WmsHub.Business.Models.BusinessIntelligence;

namespace WmsHub.BusinessIntelligence.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = AuthPolicies.RmcAuthPolicy.POLICYNAME)]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
public class RmcController : BaseController
{
  private readonly IMapper _mapper;

  public RmcController(IBusinessIntelligenceService Service,
    IMapper mapper)
    : base(Service)
  {
    _mapper = mapper;
  }

  /// <summary>
  /// List RMC User logs.
  /// </summary>
  /// <remarks>If the optional filter parameters are not provided then data 
  /// is returned for the previous 31 days only. If only one filter parameter
  /// is used then the other parameter will default to 31 days before or 
  /// after the provided parameter.</remarks>
  /// <response code="200">Request successful. Ownername, Action,
  /// ActionDateTime, Status and Status Reason.</response>
  /// <response code="204">No data returned.</response>
  /// <response code="400">Bad request.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="500">Internal server error.</response>
  /// <response code="503">Service unavailable, please try again.</response> 
  /// <param name="fromDate">(Optional) filter list to include user logs
  /// made at and after this date and time.</param>
  /// <param name="toDate">(Optional) filter list to include user logs
  /// made on and before this date and time.</param>
  /// <param name="offset">(Optional) number of days to include after fromDate
  /// or before toDate if both fromDate and toDate are not provided.</param>
  [HttpGet("useractionlog")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Produces("application/json")]
  public async Task<ActionResult<IEnumerable<BiRmcUserInformation>>> Get(
    [FromQuery] DateTimeOffset? fromDate,
    [FromQuery] DateTimeOffset? toDate,
    [FromQuery] int offset = 31)
  {
    try
    {
      DateRange dateRange = GetDateRange(fromDate, toDate, offset);

      IEnumerable<BusinessModels.BiRmcUserInformation> models =
        await Service.GetRmcUsersInformation(dateRange.From, dateRange.To);

      IEnumerable<BiRmcUserInformation> apiModels = 
        _mapper.Map<IEnumerable<BiRmcUserInformation>>(models);

      return apiModels.Any() ? Ok(apiModels) : NoContent();

    }
    catch (DateRangeNotValidException dex)
    {
      LogException(dex);
      return Problem(
        detail: dex.Message,
        statusCode: (int)HttpStatusCode.BadRequest);
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
    }
  }

  private BusinessIntelligenceService Service
  {
    get
    {
      var service = _service as BusinessIntelligenceService;
      service.User = User;
      return service;
    }
  }

}