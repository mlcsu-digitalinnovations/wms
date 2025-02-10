using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Services.Interfaces;
using WmsHub.BusinessIntelligence.Api.Models;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Api.Models;
using BusinessModels = WmsHub.Business.Models.BusinessIntelligence;

namespace WmsHub.BusinessIntelligence.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = AuthPolicies.DefaultAuthPolicy.POLICYNAME)]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
[SwaggerTag("Questionnaire related methods.")]
public class QuestionnaireController : BaseController
{
  private readonly IMapper _mapper;

  public QuestionnaireController(
    IBusinessIntelligenceService service,
    IMapper mapper)
    : base(service)
  {
    _mapper = mapper 
      ?? throw new ArgumentNullException($"{nameof(mapper)} is null");
  }

  /// <summary>
  /// Lists all questionnaires with optional filtering by referral date.
  /// </summary>
  /// <remarks>
  /// If the optional filter parameters are not provided then data 
  /// is returned for the previous 31 days only. If only one filter parameter
  /// is used then the other parameter will default to 31 days before or 
  /// after the provided parameter.
  /// </remarks>
  /// <response code="200"></response>
  /// <response code="204">No data returned.</response>
  /// <response code="400">Bad request.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="500">Internal server error.</response>
  /// <response code="503">Service unavailable, please try again.</response> 
  /// <param name="fromDate">
  /// (Optional) filter list to include completed questionnaire
  /// at and after this date and time.
  /// </param>
  /// <param name="toDate">
  /// (Optional) filter list to include completed questionnaire
  /// on and before this date and time.
  /// </param>
  /// <param name="offset">
  /// (Optional) number of days to include after fromDate
  /// or before toDate if both fromDate and toDate are not provided.
  /// </param>
  [HttpGet]
  [SwaggerResponse(200, 
    "Lists all questionnaires with optional filtering by referral date.",
    typeof(IEnumerable<BiQuestionnaire>))]
  [ProducesResponseType(StatusCodes.Status200OK,
    Type = typeof(BiQuestionnaire))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [Produces("application/json")]
  public async Task<IActionResult> GetQuestionnaireAsync(
    [FromQuery] DateTimeOffset? fromDate,
    [FromQuery] DateTimeOffset? toDate,
    [FromQuery] int offset = 31)
  {
    try
    {
      DateRange dateRange = GetDateRange(fromDate, toDate, offset);

      IEnumerable<BusinessModels.BiQuestionnaire> models =
        await Service.GetQuestionnaires(dateRange.From, dateRange.To);

      IEnumerable<BiQuestionnaire> apiModels =
        _mapper.Map<IEnumerable<BiQuestionnaire>>(models);

      return apiModels.Any() ? Ok(apiModels) : NoContent();
    }
    catch (DateRangeNotValidException dex)
    {
      LogException(dex);
      return Problem(
        detail: dex.Message,
        statusCode: StatusCodes.Status400BadRequest);
    }
    catch (Exception ex)
    {
      LogException(ex);

      return Problem(
        ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  private IBusinessIntelligenceService Service
  {
    get
    {
      return _service as IBusinessIntelligenceService;
    }
  }
}
