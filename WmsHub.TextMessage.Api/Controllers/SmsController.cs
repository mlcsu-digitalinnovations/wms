using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Notify.Exceptions;
using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;

namespace WmsHub.TextMessage.Api.Controllers
{
  [ApiController]
  [Authorize(AuthenticationSchemes = "ApiKey")]
  [ApiVersion("1.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[controller]")]
  public class SmsController : BaseController
  {
    private readonly TextOptions _options;
    private readonly IMapper _mapper;

    [ExcludeFromCodeCoverage]
    public SmsController(ITextService textService,
      IOptions<TextOptions> options,
      IMapper mapper) : base(textService)
    {
      _options = options.Value;
      _mapper = mapper;
    }

    [HttpGet("prepare")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPrepare()
    {
      try
      {
        int count = await Service.PrepareMessagesToSend();

        return Ok($"{count} text messages prepared.");
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(detail: "There was a problem preparing the " +
          "text messages. Check logs for more details");
      }
    }

    [HttpGet("checksend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCheckSend()
    {
      try
      {
        IEnumerable<ISmsMessage> smsMessages =
          await Service.GetMessagesToSendAsync();

        return Ok($"{smsMessages.Count()} text messages ready to send.");
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(detail: "There was a problem checking the " +
          "text messages. Check logs for more details");
      }
    }

    [HttpGet("send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get([FromQuery] int limit = 50)
    {
      try
      {
        IEnumerable<ISmsMessage> smsMessages =
          await Service.GetMessagesToSendAsync(limit);

        _options.ValidateNumbersAgainstWhiteList(
          smsMessages.Select(s => s.MobileNumber).ToArray());

        int count = await SendMessages(smsMessages);

        return Ok(
          $"{count} message(s) sent out of {smsMessages.Count()} requested");
      }
      catch (NumberWhiteListException ex)
      {
        LogInformation(ex.Message);
        return Problem(detail: ex.Message);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(detail: "There was a problem sending the " +
          "message. Check logs for more details");
      }
    }

    private async Task<int> SendMessages(IEnumerable<ISmsMessage> smsMessages)
    {
      int count = 0;
      foreach (ISmsMessage smsMessage in smsMessages)
      {
        smsMessage.Personalisation.Add(
          "link", $"{GetLink(smsMessage)}?u={smsMessage.Base36DateSent}");

        string outcome = Constants.TEXT_MESSAGE_FAILED;
        try
        {
          SmsNotificationResponse result = await Service
            .SendSmsMessageAsync(smsMessage);

          if (result == null)
          {
            throw new NotifyClientException(
              "Unknown error: SendSmsMessageAsync returned null");
          }

          count++;

          //Currently not saving the SMS Id to Text Message
          smsMessage.SmsId = result.id;
          smsMessage.Reference = result.reference;
          smsMessage.Sent = DateTimeOffset.Now;

          outcome = Constants.TEXT_MESSAGE_SENT;
        }
        catch (Exception ex)
        {
          // try catch block to handle exception in TextMessageService
          // and report them in the log  here without breaking for loop
          LogException(ex);
          outcome = Constants.TEXT_MESSAGE_FAILED;
        }
        finally
        {
          await Service.UpdateMessageRequestAsync(smsMessage, outcome);
        }
      }

      return count;
    }

    private string GetLink(ISmsMessage smsMessage)
    {
      if (smsMessage.ReferralSource == 
        ReferralSource.GeneralReferral.ToString())
      {
        return _options.GeneralReferralNotifyLink;
      }
      else
      {
        return _options.NotifyLink;
      }
    }

    /// <summary>
    /// Kicks off the process of sending one text message for the given 
    /// referral ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(Guid id)
    {
      try
      {
        await Service.PrepareMessagesToSend(id);
        ISmsMessage smsMessage = await Service
          .GetMessageByReferralIdToSendAsync(id);

        if (string.IsNullOrEmpty(smsMessage.MobileNumber))
        {
          throw new ReferralInvalidStatusException("Invalid message");
        }

        _options.ValidateNumbersAgainstWhiteList(new List<string>()
          {smsMessage.MobileNumber});

        DateTimeOffset dateSent = DateTimeOffset.Now;
        smsMessage.Base36DateSent = Base36Converter
          .ConvertDateTimeOffsetToBase36(dateSent);

        string link = _options.NotifyLink.EndsWith("/")
          ? _options.NotifyLink
          : $"{_options.NotifyLink}/";

        smsMessage.Personalisation.Add(
          "link", $"{link}?u={smsMessage.Base36DateSent}");

        try
        {
          Notify.Models.Responses.SmsNotificationResponse result =
            await Service.SendSmsMessageAsync(smsMessage);
          //Currently not saving the SMS Id to Text Message
          smsMessage.SmsId = result.id;
          smsMessage.Reference = result.reference;
          smsMessage.Sent = dateSent.DateTime;
          await Service.UpdateMessageRequestAsync(smsMessage);
        }
        catch (Exception ex)
        {
          LogException(ex);
        }
        //200 status code
        return new OkObjectResult($"1 messages sent out of 1 requested");
      }
      catch (BadHttpRequestException bre)
      {
        LogException(bre);
        return Problem(detail: bre.Message, title: "Current detail",
          statusCode: StatusCodes.Status404NotFound);
      }
      catch (InvalidOperationException iox)
      {
        LogException(iox);
        return Problem(detail: iox.Message, title: "Current detail",
          statusCode: StatusCodes.Status409Conflict);
      }
      catch (ReferralInvalidStatusException vex)
      {
        LogException(vex);
        return Problem(detail: vex.Message, title: "Current detail",
          statusCode: StatusCodes.Status500InternalServerError);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(detail: ex.Message, title: "Current detail",
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }


    /// <summary>
    /// For saving the message that is to be sent
    /// </summary>
    /// <param name="request">Referral.Id and Mobile number (+44)</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(TextMessageRequest request)
    {
      #region Validation
      var validatedModel = request.IsValid<TextMessageRequest>();

      if (!validatedModel.IsValid)
      {
        return BaseBadRequestObjectResult(
          $"Model is not valid:{validatedModel.Error}");
      }
      #endregion Validation

      bool result = await Service.AddNewMessageAsync(request);

      if (!result)
      {
        return BaseBadRequestObjectResult("New message not added");
      }

      return new OkObjectResult(result);

    }

    private TextService Service
    {
      get
      {
        TextService textService = _service as TextService;
        if (User != null)
        {
          textService.User = User;
        }

        return textService;
      }
    }
  }
}
