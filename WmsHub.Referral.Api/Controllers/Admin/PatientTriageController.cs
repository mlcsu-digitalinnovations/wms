using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Referral.Api.Models;

namespace WmsHub.Referral.Api.Controllers.Admin
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/admin/[controller]")]
  [Route("admin/[Controller]")]
  public class PatientTriageController : BaseController
  {
    private readonly IMapper _mapper;
    public PatientTriageController(IPatientTriageService service,
      IMapper mapper) : base(
      service)
    {
      _mapper = mapper;
    }

    /// <summary>
    /// Gets a list of all PatientTriageItems
    /// </summary>
    /// <returns>The number of referrals prepared</returns>
    /// <response code="200">Referrals prepared</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpGet]
    public IActionResult Get()
    {
      try
      {
        PatientTriageItemsResponse response = Service.GetAllTriage();
        if (response.Status != StatusType.Valid)
        {
          return Problem(
            detail: response.GetErrorMessage(),
            statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(response);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    /// <summary>
    /// Updates a PatientTriage item
    /// </summary>
    /// <param name="request"></param>
    /// <response code="200">Referral updated</response>
    /// <response code="400">Missing/invalid values</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpPut]
    public async Task<IActionResult> Put([FromBody]PatientTriagePutRequest request)
    {
      try
      {
        PatientTriageUpdateRequest model = 
          _mapper.Map<PatientTriageUpdateRequest>(request);

        PatientTriageUpdateResponse response = 
          await Service.UpdatePatientTriage(model);

        if (response.Status == StatusType.Invalid)
        {
          return Problem(
            detail: response.GetErrorMessage(),
            statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(response);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    protected PatientTriageService Service
    {
      get
      {
        PatientTriageService service = _service as PatientTriageService;
        service.User = User;
        return service;
      }
    }
  }
}
