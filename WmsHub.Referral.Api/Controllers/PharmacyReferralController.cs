using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Referral.Api.Models;

namespace WmsHub.Referral.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[Controller]")]
  public class PharmacyReferralController : BaseController
  {
    private readonly IMapper _mapper;
    private PharmacyReferralOptions _options;

    public PharmacyReferralController(IReferralService referralService,
      IOptions<PharmacyReferralOptions> options,
      IMapper mapper)
      : base(referralService)
    {
      _options = options.Value;
      _mapper = mapper;
    }

    /// <summary>
    /// Get List of Ethnicities
    /// </summary>
    /// <response code="200">Returns the list of ethnicities</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("Ethnicity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEthnicities()
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "PharmacyReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        IEnumerable<Models.Ethnicity> ethnicities =
          _mapper.Map<IEnumerable<Models.Ethnicity>>(
            await Service.GetEthnicitiesAsync(ReferralSource.Pharmacy));

        return Ok(ethnicities);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
      }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
      Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("application/json")]
    public async Task<IActionResult> Post([FromBody]
      PharmacyReferralPostRequest pharmacyReferralPostRequest)
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "PharmacyReferral.Service")
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");

        pharmacyReferralPostRequest.InjectionRemover();

        // HACK update ethnicity for non-disclosure
        string ethnicityToChange =
          "The patient does not want to disclose their ethnicity";
        string ethnicityToChangeTo =
          "I do not wish to Disclose my Ethnicity";
        if (pharmacyReferralPostRequest
            .ServiceUserEthnicity == ethnicityToChange &&
          pharmacyReferralPostRequest
            .ServiceUserEthnicityGroup == ethnicityToChange)
        {
          pharmacyReferralPostRequest
            .ServiceUserEthnicity = ethnicityToChangeTo;
          pharmacyReferralPostRequest
            .ServiceUserEthnicityGroup = ethnicityToChangeTo;
        }

        IPharmacyReferralCreate referralCreate =
          _mapper.Map<PharmacyReferralCreate>(pharmacyReferralPostRequest);

        referralCreate.ReferringPharmacyEmailIsWhiteListed =
          _options.IsEmailInWhitelist(
            pharmacyReferralPostRequest.ReferringPharmacyEmail, false);

        referralCreate.ReferringPharmacyEmailIsValid = await Service
          .PharmacyEmailListedAsync(referralCreate.ReferringPharmacyEmail);

        referralCreate.EthnicityAndServiceUserEthnicityValid = 
          await Service.EthnicityToServiceUserEthnicityMatch(
            referralCreate.Ethnicity, referralCreate.ServiceUserEthnicity);

        referralCreate.EthnicityAndGroupNameValid =
          await Service.EthnicityToGroupNameMatch(
            referralCreate.Ethnicity, 
            referralCreate.ServiceUserEthnicityGroup);

        ValidateModelResult result = Validators.ValidateModel(referralCreate);
        if (!result.IsValid)
        {
          throw new PharmacyReferralValidationException(result.Results);
        }

        IReferral referral =
          await Service.CreatePharmacyReferral(referralCreate);

        return Ok(referral);
      }
      catch (Exception ex)
      {
        if (ex is PharmacyReferralValidationException)
        {
          LogInformation(ex.Message);
          return ValidationProblem(new ValidationProblemDetails(
            (ex as PharmacyReferralValidationException).ValidationResults));
        }
        else if (ex is ReferralNotUniqueException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status409Conflict);
        }
        else if (ex is EmailWhiteListException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status403Forbidden);
        }
        else if (ex is NoProviderChoicesFoundException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status204NoContent);
        }
        else
        {
          LogException(ex, pharmacyReferralPostRequest);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    [HttpGet]
    [Route("GenerateKey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetKey(string email,
      int expireMinutes = 10)
    {
      Random random = new Random();
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "PharmacyReferral.Service")
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");        

        //Validate Email is NHS
        if (string.IsNullOrWhiteSpace(email))
        {
          throw new EmailNotProvidedException("Email must be provided.");
        }

        if (!email.EndsWith("@nhs.net"))
        {
          throw new EmailWrongDomainException(
            "Only emails from the domain @nhs.net are allowed.");
        }

        _options.IsEmailInWhitelist(email);

        PharmacistKeyCodeCreate create = new PharmacistKeyCodeCreate
        {
          ReferringPharmacyEmail = email,
          KeyCode = Generators.GenerateKeyCode(random),
          ExpireMinutes = expireMinutes
        };

        IPharmacistKeyCodeGenerationResponse response =
          await Service.GetPharmacistKeyCodeAsync(create);

        if (response.Errors.Any())
          throw new PharmacistCreateException(response.GetErrorMessage());

        return Ok(response);
      }
      catch (Exception ex)
      {
        if (ex is PharmacyReferralValidationException)
        {
          LogInformation(ex.Message);
          return ValidationProblem(new ValidationProblemDetails(
            (ex as PharmacyReferralValidationException).ValidationResults));
        }
        else if (ex is PharmacistCreateException ||
                 ex is EmailNotProvidedException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
        }
        else if (ex is EmailWrongDomainException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status422UnprocessableEntity);
        }
        else if (ex is EmailWhiteListException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status403Forbidden);
        }
        else
        {
          LogException(ex, email);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }

    }

    [HttpGet]
    [Route("ValidateKey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateKey(string email, string keyCode)
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "PharmacyReferral.Service")
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");

        _options.IsEmailInWhitelist(email);

        PharmacistKeyCodeCreate create = new PharmacistKeyCodeCreate
        {
          ReferringPharmacyEmail = email,
          KeyCode = keyCode,
          ExpireMinutes = 10
        };

        IPharmacistKeyCodeValidationResponse response =
          await Service.ValidatePharmacistKeyCodeAsync(create);

        return Ok(response);
      }
      catch (Exception ex)
      {
        if (ex is PharmacyReferralValidationException)
        {
          LogInformation(ex.Message);
          return ValidationProblem(new ValidationProblemDetails(
            (ex as PharmacyReferralValidationException).ValidationResults));
        }
        else if (ex is EmailNotProvidedException ||
          ex is EmailWrongDomainException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
        }
        else if (ex is EmailWhiteListException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status403Forbidden);
        }
        else if (ex is PharmacyKeyCodeExpiredException ||
          ex is PharmacyKeyCodeIncorrectException ||
          ex is PharmacyKeyCodeTooManyAttemptsException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status422UnprocessableEntity);
        }
        else
        {
          LogException(ex, email);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    [HttpPost]
    [Route("NhsNumberInUse")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
      Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("application/json")]
    public async Task<IActionResult> IsNhsNumberInUse(
      [FromBody] IsNhsNumberInUseRequest request)
    {
      try
      {
        if (User != null &&
            User.FindFirst(ClaimTypes.Name).Value != "PharmacyReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        request.InjectionRemover();

        try
        {
          await Service.CheckReferralCanBeCreatedWithNhsNumberAsync(
            request.NhsNumber);
        }
        catch (ReferralNotUniqueException ex)
        {
          Log.Debug(ex.Message, request.NhsNumber);
          return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
        catch (InvalidOperationException ex)
        {
          Log.Debug(ex.Message, request.NhsNumber);
          return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
        }

        return Ok();
      }
      catch (Exception ex)
      {
        if (ex is SelfReferralValidationException)
        {
          LogInformation(ex.Message);
          return ValidationProblem(new ValidationProblemDetails(
            (ex as SelfReferralValidationException).ValidationResults));
        }
        else
        {
          LogException(ex, request);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    protected internal IReferralService Service
    {
      get
      {
        IReferralService service = _service as IReferralService;
        service.User = User;
        return service;
      }
    }
  }
}
