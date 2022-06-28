using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.BusinessIntelligence.Api.Models;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.BusinessIntelligence.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[Controller]")]
  public class ReferralReprocessing : BaseController
  {
    private readonly IMapper _mapper;
    public ReferralReprocessing(IBusinessIntelligenceService busIntelliService,
      IMapper mapper)
      : base(busIntelliService)
    {
      _mapper = mapper;
    }

    /// <summary>
    /// Lists referrals reprocessing information.
    /// </summary>
    /// <remarks>If the optional filter parameters are not provided then data 
    /// is returned for the previous 31 days only. If only one filter parameter
    /// is used then the other parameter will default to 31 days before or 
    /// after the provided parameter.</remarks>
    /// <response code="200">Request successful.</response>
    /// <response code="204">No data returned.</response>
    /// <response code="400">Bad request.</response>
    /// <response code="401">Invalid authentication.</response>
    /// <response code="500">Internal server error.</response>
    /// <response code="503">Service unavailable, please try again.</response> 
    /// <param name="fromDate">(Optional) filter list to include only referrals
    /// with a referral date equal to and after this date and time.</param>
    /// <param name="toDate">(Optional) filter list to include only referrals
    /// with a referral date equal to and before this date and time.</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<ReprocessedReferral>>>
      Get(
      [FromQuery] DateTimeOffset? fromDate,
      [FromQuery] DateTimeOffset? toDate)
    {
      GetAzureSocketIp();

      //filter date check
      if (fromDate.HasValue && toDate.HasValue)
      {
        if (fromDate > toDate)
        {
          return Problem(
            detail: $"'from' date {fromDate} cannot be "
                    + $"later than 'to' date {toDate}.",
            statusCode: StatusCodes.Status400BadRequest);
        }
      }
      else if (fromDate.HasValue && !toDate.HasValue)
      {
        toDate = fromDate.Value.AddDays(31);
      }
      else if (!fromDate.HasValue && toDate.HasValue)
      {
        fromDate = toDate.Value.AddDays(-31);
      }
      else
      {
        toDate = DateTimeOffset.Now;
        fromDate = toDate.Value.AddDays(-31);
      }

      try
      {
        IEnumerable<Business.Models.ReprocessedReferral> models =
          await Service.GetAnonymisedReprocessedReferralsBySubmissionDate(
            fromDate,
            toDate);

        IEnumerable<ReprocessedReferral> reprocessedReferrals = 
          _mapper.Map<IEnumerable<ReprocessedReferral>>(models);

        return Ok(reprocessedReferrals);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(detail: ex.Message,
          statusCode: (int)HttpStatusCode.InternalServerError);
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
}
