using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
  public class CourseCompletionController : BaseController
  {
    private readonly IMapper _mapper;
    public CourseCompletionController(IPatientTriageService service,
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
    public async Task<IActionResult> Get()
    {
      try
      {
        CourseCompletionResponse response = 
          await Service.GetAllCourseCompletionAsync();
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
    public async Task<IActionResult> Put(
      [FromBody] CourseCompletionRequest request)
    {
      try
      {
        CourseCompletion model =
          _mapper.Map<CourseCompletion>(request);

        CourseCompletionResponse response =
          await Service.UpdateCourseCompletionAsync(model);

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
