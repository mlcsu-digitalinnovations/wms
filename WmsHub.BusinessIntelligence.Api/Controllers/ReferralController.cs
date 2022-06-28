
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace WmsHub.BusinessIntelligence.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[Controller]")]
  public class ReferralController : BaseController
  {
    private readonly IMapper _mapper;
    public ReferralController(IBusinessIntelligenceService busIntelliService,
      IMapper mapper)
      : base(busIntelliService)
    {
      _mapper = mapper;
    }

    /// <summary>
    /// Lists all referrals with optional filtering by referral date.
    /// </summary>
    /// <remarks>All referrals returned are anonymised. If the optional filter 
    /// parameters are not provided then data is returned for the previous 
    /// 31 days only. If only one filter parameter is used then the other 
    /// parameter will default to 31 days before or after the provided 
    /// parameter.</remarks>
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
    public async Task<ActionResult<IEnumerable<Models.AnonymisedReferral>>>
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

      BusinessIntelligenceService busIntService = Service;
      List<Models.AnonymisedReferral> AnonymisedReferralviewModel;

      try
      {
        IEnumerable<Business.Models.AnonymisedReferral> anonReferralDtos =
          await busIntService.GetAnonymisedReferrals(fromDate, toDate);

        if (anonReferralDtos.Count() == 0)
          return NoContent();

        AnonymisedReferralviewModel = (List<Models.AnonymisedReferral>)
          _mapper.Map(
            anonReferralDtos,
            typeof(IEnumerable<Business.Models.AnonymisedReferral>),
            typeof(IEnumerable<Models.AnonymisedReferral>)
        );
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
      }

      return Ok(AnonymisedReferralviewModel);
    }


    ///// <summary>
    ///// Lists all providers, their submission errors and referral awaiting 
    ///// acceptance stats.
    ///// </summary>
    ///// <remarks>If the optional filter parameters are not provided then data 
    ///// is returned for the previous 31 days only. If only one filter parameter
    ///// is used then the other parameter will default to 31 days before or 
    ///// after the provided parameter.</remarks>
    ///// <response code="200">Request successful.</response>
    ///// <response code="204">No data returned.</response>
    ///// <response code="400">Bad request.</response>
    ///// <response code="401">Invalid authentication.</response>
    ///// <response code="500">Internal server error.</response>
    ///// <response code="503">Service unavailable, please try again.</response> 
    ///// <param name="ubrn">this date and time.</param>
    // ND - THIS IS BEING REMOVED UNTIL FURTHER NOTICE
    //[HttpGet("History")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    //[Produces("application/json")]
    //public async 
    //  Task<ActionResult<IEnumerable<Models.AnonymisedReferralHistory>>>
    //  Get(string ubrn)
    //{
    //  GetAzureSocketIp();

    //  //filter date check
    //  if (string.IsNullOrWhiteSpace(ubrn))
    //  {
    //      return Problem(
    //        detail: $"'UBRN must be provided",
    //        statusCode: StatusCodes.Status400BadRequest); 
    //  }

    //  BusinessIntelligenceService busIntService = Service;
    //  List<Models.AnonymisedReferralHistory> models;

    //  try
    //  {
    //    IEnumerable<Business.Models.AnonymisedReferral> dtos =
    //      await busIntService.GetAnonymisedReferralsForUbrn(ubrn);

    //    if (dtos.Count() == 0)
    //      return NoContent();

    //    models = (List<Models.AnonymisedReferralHistory>)
    //      _mapper.Map(
    //        dtos,
    //        typeof(IEnumerable<Business.Models.AnonymisedReferralHistory>),
    //        typeof(IEnumerable<Models.AnonymisedReferralHistory>)
    //      );
    //  }
    //  catch (Exception ex)
    //  {
    //    LogException(ex);
    //    return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
    //  }

    //  return Ok(models);
    //}

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

