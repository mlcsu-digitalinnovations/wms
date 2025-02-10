using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Notify.Models.Responses;
using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.TextMessage.Api.Filters;
using static WmsHub.Common.Helpers.Constants.MessageTemplateConstants;

namespace WmsHub.TextMessage.Api.Controllers
{

  [ApiController]
  [Authorize]
  [ApiVersion("1.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[controller]")]
  public class CallbackController : BaseController
  {
    private readonly ITextOptions _options;
    private readonly IMapper _mapper;

    public CallbackController(ITextService textService,
      IMapper mapper, IOptions<TextOptions> options)
      : base(textService)
    {
      _mapper = mapper;
      _options = options.Value;
    }
    
    /// <summary>
    /// The Gov.uk/Notify service requires a Bearer Token authentication
    /// </summary>
    /// <param name="postRequest">CallbackPostRequest</param>
    /// <returns></returns>
    [HttpPost]
    // v1.1 specific action for GET api/values endpoint
    //[MapToApiVersion("1.1")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ServiceFilter(typeof(Filters.ApiAccessFilterAsync))]
    [ApiRequestObjectFilter(typeof(Models.Notify.CallbackPostRequest))]
    public async Task<IActionResult> Post(
      Models.Notify.CallbackPostRequest postRequest)
    {
      CallbackRequest request = (CallbackRequest)_mapper.Map(postRequest,
        typeof(Models.Notify.CallbackPostRequest),
        typeof(CallbackRequest)
        );

      try
      {
        if (request.IsCallback)
        {
          Models.Notify.CallbackPostResponse apiResponse =
          _mapper.Map<Models.Notify.CallbackPostResponse>(
            await Service.IsCallBackAsync(request));

          return BaseReturnResponse(
                apiResponse.ResponseStatus,
                apiResponse,
                apiResponse.GetErrorMessage());
        }
        else
        {
          //Then this is a message sent to the text number
          //Reply with message and log
          LogInformation($"Message ({request.Message}) received " +
            $"from {request.SourceNumber}");

          Guid templateId = _options.GetTemplateIdFor(
            TEMPLATE_NUMBERNOTMONITORED);

          ISmsMessage sms = new SmsMessage()
          {
            MobileNumber = request.SourceNumber,
            ClientReference = request.Reference,
            TemplateId = templateId.ToString()
          };

          SmsNotificationResponse result =
            await Service.SendSmsMessageAsync(sms);

          return Ok(result);
        }
      }
      catch (Exception ex)
      {
        LogException(ex, request);
        return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
      }
    }

    protected internal virtual TextService Service
    {
      get
      {
        TextService service = _service as TextService;
        _service.User = User;
        return service;
      }
    }
  }
}