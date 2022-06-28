using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ChatBotService;
using WmsHub.Business.Services;
using WmsHub.ChatBot.Api.Clients;
using WmsHub.ChatBot.Api.Models;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Exceptions;

namespace WmsHub.ChatBot.Api.Controllers
{

  [ApiController]
  [ApiVersion("1.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[controller]")]
  public class ReferralCallController : BaseController
  {
    private readonly IMapper _mapper;
    private readonly IArcusClientHelper _clientHelper;
    protected  ArcusOptions _options;

    public ReferralCallController(
      IArcusClientHelper clientHelper,
      IChatBotService chatBotService,
      IOptions<ArcusOptions> options,
      IMapper mapper)
       : base(chatBotService)
    {
      _clientHelper = clientHelper;
      _mapper = mapper;
      _options = options.Value;
    }

    /// <summary>
    /// Updates a specific referral with the call outcome
    /// </summary>
    /// <param name="referralCall"></param>
    /// <response code="200">Referral updated with call outcome</response>
    /// <response code="400">Missing/invalid values</response>
    /// <response code="401">Invalid API key</response>
    /// <response code="404">Id not found</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Post([FromBody] ReferralCall referralCall)
    {
      try
      {
        UpdateReferralWithCallRequest request =
          _mapper.Map<UpdateReferralWithCallRequest>(referralCall);

        UpdateReferralWithCallResponse response =
          await Service.UpdateReferralWithCall(request);

        if (response.ResponseStatus != StatusType.Valid)
        {
          string error = response.GetErrorMessage();
          LogInformation(error);
          return BadRequest(error);
        }
        return Ok();
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
      }
    }


    /// <summary>
    /// Updates a specific referral if a single referral identified 
    /// to ChatBotTransfer
    /// </summary>
    /// <param name="transferRequest"></param>
    /// <response code="200">Referral updated with 
    /// outcome - TransferringToRmc</response>
    /// <response code="400">Missing/invalid values</response>
    /// <response code="401">Invalid API key</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpPost]
    [Consumes("application/json")]
    [Route("ChatBotTransfer")]
    public async Task<IActionResult>PostChatBotTransfer(
      TransferRequest transferRequest)
    {
      try
      {
        UpdateReferralTransferRequest request =
          _mapper.Map<UpdateReferralTransferRequest>(transferRequest);

        UpdateReferralTransferResponse response =
          await Service.UpdateReferralTransferRequestAsync(request);

        if (response.ResponseStatus != StatusType.Valid)
        {
          string error = response.GetErrorMessage();
          LogInformation(error);
          return BadRequest(error);
        }
        return Ok();
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(statusCode: (int)HttpStatusCode.InternalServerError,
          detail: ex.Message);
      }
    }

    /// <summary>
    /// Start of the calee collection
    /// <returns>IActionResult</returns>
    /// </summary>
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
      try
      {
        PrepareCallsForTodayResponse prepareResponse =
          await Service.PrepareCallsAsync();

        //Retrieve list of calls
        GetReferralCallListResponse response =
          await Service.GetReferralCallList(null);

        List<string> numbersToValidate = new List<string>();
        numbersToValidate.AddRange(
          response?.Arcus?.Callees.Select(c => c.PrimaryPhone));
        numbersToValidate.AddRange(
          response?.Arcus?.Callees
            .Where(c => !string.IsNullOrWhiteSpace(c.SecondaryPhone))
            .Select(c => c.SecondaryPhone));

        _options.ValidateNumbersAgainstWhiteList(numbersToValidate);

        if (response.Status == StatusType.Invalid)
          return Problem(response.GetErrorMessage(),
          statusCode: (int)HttpStatusCode.BadRequest);

        if (response.Arcus.Callees.Any())
        {
          //Make the call to argus
          HttpResponseMessage clientResponse =
            await _clientHelper.BatchPost(response.Arcus);
          //_logger.Debug("Arcus response: {@response}", response);

          var message = await clientResponse.Content.ReadAsStringAsync();
          //_logger.Debug("Arcus message: {@message}", message);

          if (clientResponse.StatusCode != HttpStatusCode.Created)
            throw new ArgumentException(message);

          await Service.UpdateReferralCallListSent(response.Arcus.Callees);
        }

        return Ok($"{prepareResponse.CallsPrepared} call(s) prepared and " +
          $"{response.Arcus.NumberOfCallsToMake} call(s) added to contact " +
          $"flow {_options.ContactFlowName}. There were " +
          $"{response.DuplicateCount} duplicate and " +
          $"{response.InvalidNumberCount} invalid number(s) processed.");
      }
      catch (NumberWhiteListException ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: (int)HttpStatusCode.InternalServerError);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
      }
    }

    protected virtual ChatBotService Service
    {
      get
      {
        ChatBotService service = _service as ChatBotService;
        service.User = User;
        return service;
      }
    }
  }


}