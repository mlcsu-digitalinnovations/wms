using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Extensions;
using WmsHub.Referral.Api.Models;

namespace WmsHub.Referral.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
public class PracticeController : BaseController
{
  private readonly IMapper _mapper;

  public PracticeController(IPracticeService practiceService, IMapper mapper)
    : base(practiceService)
  {
    _mapper = mapper;
  }

  /// <summary>
  /// Gets all practices that have registered a system
  /// </summary>
  /// <returns>All practices that have registered a system</returns>
  [HttpGet]
  [ProducesResponseType(
    StatusCodes.Status200OK, Type = typeof(IEnumerable<Practice>))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Route("system")]
  public async Task<IActionResult> GetAsync()
  {
    try
    {
      if (User == null || User.FindFirstValue(ClaimTypes.Name) !=
        ApiKeyProvider.OWNER_PRACTICE)
      {
        return Problem(
          title: "Access has not been granted for this endpoint.",
          statusCode: StatusCodes.Status401Unauthorized);
      }

      List<Practice> practice = _mapper.Map<List<Practice>>(
        await Service.GetAsync());

      if (practice.Count == 0)
      {
        return NoContent();
      }
      
      return Ok(practice);
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
  /// Get a practice object via its ODS code.
  /// </summary>
  /// <param name="odsCode">The ODS code to search for.</param>
  /// <returns>A practice object.</returns>
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Practice))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Route("system/{odsCode}")]
  public async Task<IActionResult> GetByOdsCodeAsync(string odsCode)
  {
    try
    {
      if (User.FindFirstValue(ClaimTypes.Name) !=
        ApiKeyProvider.OWNER_PRACTICE)
      {
        return Problem(
          title: "Access has not been granted for this endpoint.",
          statusCode: StatusCodes.Status401Unauthorized);
      }

      if (string.IsNullOrWhiteSpace(odsCode))
      {
        return Problem(
          title: "The provided odsCode cannot be null or white space.",
          statusCode: StatusCodes.Status400BadRequest);
      }

      Practice practice = _mapper
        .Map<Practice>(await Service.GetByObsCodeAsync(odsCode));

      if (practice == null)
      {
        return NoContent();
      }

      return Ok(practice);
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
  /// Creates a practice
  /// </summary>
  /// <returns>A practice object</returns>
  [HttpPost]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Practice))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Route("system")]
  public async Task<IActionResult> CreateAsync(Practice practice)
  {
    try
    {
      if (User.FindFirstValue(ClaimTypes.Name) !=
        ApiKeyProvider.OWNER_PRACTICE)
      {
        return Problem(
          title: "Access has not been granted for this endpoint.",
          statusCode: StatusCodes.Status401Unauthorized);
      }

      practice.InjectionRemover();

      Business.Models.Practice practiceToCreate = _mapper
        .Map<Business.Models.Practice>(practice);

      Business.Models.IPractice practiceCreated = await Service
        .CreateAsync(practiceToCreate);

      practice = _mapper.Map<Practice>(practiceCreated);

      return Ok(practice);
    }
    catch (Exception ex)
    {
      if (ex is PracticeExistsException
        || ex is PracticeInvalidException)
      {
        LogInformation(ex.Message);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }
      else
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }
  }

  /// <summary>
  /// Updates a practice
  /// </summary>
  /// <returns>A practice object</returns>
  [HttpPut]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Practice))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Route("system/{odsCode}")]
  public async Task<IActionResult> UpdateAsync(
    string odsCode, Practice practice)
  {
    try
    {
      if (User.FindFirstValue(ClaimTypes.Name) !=
        ApiKeyProvider.OWNER_PRACTICE)
      {
        return Problem(
          title: "Access has not been granted for this endpoint.",
          statusCode: StatusCodes.Status401Unauthorized);
      }

      practice.InjectionRemover();

      Business.Models.Practice practiceToUpdate = _mapper
        .Map<Business.Models.Practice>(practice);
      practiceToUpdate.OdsCode = odsCode;

      Business.Models.IPractice practiceUpdated = await Service
        .UpdateAsync(practiceToUpdate);

      practice = _mapper.Map<Practice>(practiceUpdated);

      return Ok(practice);
    }
    catch (Exception ex)
    {
      if (ex is PracticeNotFoundException
        || ex is PracticeInvalidException)
      {
        LogInformation(ex.Message);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }
      else
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }
  }

  private PracticeService Service
  {
    get
    {
      PracticeService service = _service as PracticeService;
      service.User = User;
      return service;
    }
  }
}