using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Referral.Api.Models;

namespace WmsHub.Referral.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
public class PharmacyController : BaseController
{
  private readonly IMapper _mapper;

  public PharmacyController(IPharmacyService pharmacyService, IMapper mapper)
    : base(pharmacyService)
  {
    _mapper = mapper;
  }

  /// <summary>
  /// Gets all pharmacies that have registered a system
  /// </summary>
  /// <returns>All pharmacies that have registered a system</returns>
  [HttpGet]
  [ProducesResponseType(
    StatusCodes.Status200OK, Type = typeof(IEnumerable<PharmacyPut>))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  public async Task<IActionResult> GetAsync()
  {
    try
    {
      if (User == null || User.FindFirstValue(ClaimTypes.Name) !=
        ApiKeyProvider.OWNER_PHARMACY)
      {
        return Problem(
          title: "Access has not been granted for this endpoint.",
          statusCode: StatusCodes.Status401Unauthorized);
      }

      List<PharmacyPut> models = _mapper.Map<List<PharmacyPut>>(
        await Service.GetAsync());

      if (!models.Any())
      {
        return NoContent();
      }

      return Ok(models);
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
  /// Get a pharmacy object via its ODS code.
  /// </summary>
  /// <param name="odsCode">The ODS code to search for.</param>
  /// <returns>A pharmacy object.</returns>
  [HttpGet("GetOdsCode")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PharmacyPut))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  public async Task<IActionResult> GetByOdsCodeAsync(string odsCode)
  {
    try
    {
      if (User.FindFirstValue(ClaimTypes.Name) !=
          ApiKeyProvider.OWNER_PHARMACY)
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

      PharmacyPut model = _mapper
        .Map<PharmacyPut>(await Service.GetByObsCodeAsync(odsCode));

      if (model == null)
      {
        return NoContent();
      }

      return Ok(model);
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
  /// Creates a pharmacy
  /// </summary>
  /// <returns>A pharmacy object</returns>
  [HttpPost("Create")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = 
    typeof(PharmacyPost))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  public async Task<IActionResult> CreateAsync(PharmacyPost createModel)
  {
    try
    {
      if (User.FindFirstValue(ClaimTypes.Name) !=
          ApiKeyProvider.OWNER_PHARMACY)
      {
        return Problem(
          title: "Access has not been granted for this endpoint.",
          statusCode: StatusCodes.Status401Unauthorized);
      }

      Business.Models.Pharmacy model = _mapper
        .Map<Business.Models.Pharmacy>(createModel);

      Business.Models.IPharmacy modelCreated = await Service
        .CreateAsync(model);

      createModel = _mapper.Map<PharmacyPost>(modelCreated);

      return Ok(createModel);
    }
    catch (Exception ex)
    {
      if (ex is PharmacyExistsException
          || ex is PharmacyInvalidException)
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
  /// Updates a pharmacy
  /// </summary>
  /// <returns>A pharmacy object</returns>
  [HttpPut("Update/{odsCode}")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PharmacyPut))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  public async Task<IActionResult> UpdateAsync(
    string odsCode, PharmacyPut updateModel)
  {
    try
    {
      if (User.FindFirstValue(ClaimTypes.Name) !=
          ApiKeyProvider.OWNER_PHARMACY)
      {
        return Problem(
          title: "Access has not been granted for this endpoint.",
          statusCode: StatusCodes.Status401Unauthorized);
      }

      Business.Models.Pharmacy model = _mapper
        .Map<Business.Models.Pharmacy>(updateModel);
      model.OdsCode = odsCode;

      Business.Models.IPharmacy modelUpdated = await Service
        .UpdateAsync(model);

      updateModel = _mapper.Map<PharmacyPut>(modelUpdated);

      return Ok(updateModel);
    }
    catch (Exception ex)
    {
      if (ex is PharmacyNotFoundException
          || ex is PharmacyInvalidException)
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

  private PharmacyService Service
  {
    get
    {
      PharmacyService service = _service as PharmacyService;
      service.User = User;
      return service;
    }
  }
}