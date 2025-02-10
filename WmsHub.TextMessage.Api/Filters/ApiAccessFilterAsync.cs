using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.Notify;
using WmsHub.TextMessage.Api.Models.Notify;
using WmsHub.TextMessage.Api.Models.Profiles;

namespace WmsHub.TextMessage.Api.Filters
{
  [ExcludeFromCodeCoverage]
  public class ApiAccessFilterAsync : IAsyncActionFilter
  {

    public async Task OnActionExecutionAsync(ActionExecutingContext context,
      ActionExecutionDelegate next)
    {
      var param = context.ActionArguments.SingleOrDefault(
        p => p.Value is CallbackPostRequest);
      if (param.Value == null)
      {
        context.Result = new BadRequestObjectResult("Object is null");
        return;
      }

      if (!context.ModelState.IsValid)
      {
        context.Result = new BadRequestObjectResult(context.ModelState);
      }

      var resultContext = await next();
    }
  }

  public class ApiRequestObjectFilterAttribute : ActionFilterAttribute
  {
    private readonly Type _type;
    private readonly string _name;
    protected IMapper Mapper { get; set; }

    public ApiRequestObjectFilterAttribute():
      this(typeof(CallbackPostRequest))
    {
    }

      [ExcludeFromCodeCoverage]
    public ApiRequestObjectFilterAttribute(Type type)
    {
      _type = type;
      _name = _type.Name;

      CallbackRequestProfile callbackProfile =
        new CallbackRequestProfile();
      MapperConfiguration configuration =
        new MapperConfiguration(cfg => cfg.AddProfile(callbackProfile));
      Mapper = new Mapper(configuration);
    }

    /// <summary>
    /// The callback request has two actions, callback and a message receiver.
    /// A test needs to be carried out to determine if it is a Callback then 
    ///   the validation is against the correct fields
    /// If it is a message, then validate the message properties
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
      // TODO: ApiRequestObjectFilterAttribute acting as though its static
      // _scaffold gets filled up with errors. Moved here to create with each 
      // request.
      BadRequestObjectScaffold scaffold = new BadRequestObjectScaffold();

      KeyValuePair<string, object> param = 
        context.ActionArguments.SingleOrDefault(p => p.Value is object);
      bool result = ValidateRequest(param, scaffold);

      if (!result)
      {
        scaffold.Message =
          $"{_name} has the following {scaffold.Errors.Count } errors";
        string json = JsonConvert.SerializeObject(scaffold);
        context.Result = new BadRequestObjectResult(json);
        scaffold.Errors.DefaultIfEmpty();
        return;
      }

      base.OnActionExecuting(context);
    }

    protected internal virtual bool ValidateRequest(
      KeyValuePair<string, object> param,
      BadRequestObjectScaffold scaffold)
    {
      try
      {
        if (_type == typeof(CallbackPostRequest))
        {
          CallbackRequest callback =
           Mapper.Map<CallbackPostRequest, CallbackRequest>(
              param.Value as CallbackPostRequest);

          if (callback.IsCallback)
          {
            if (string.IsNullOrWhiteSpace(callback.Reference))
              scaffold.Errors.Add($"Reference must be supplied");

            if (string.IsNullOrWhiteSpace(callback.To))
              scaffold.Errors.Add($"Message 'TO' must be supplied");

            if (!string.IsNullOrWhiteSpace(callback.To)
              && !(callback.To.StartsWith("07")
              || callback.To.StartsWith("+447")))
              scaffold.Errors.Add(
                $"To: {callback.To} must be a mobile number");

            if (callback.NotificationTypeValue == CallbackNotification.Email)
              scaffold.Errors.Add(
                "Only SMS Notification Type Callback allowed");
          }
          else
          {
            if (string.IsNullOrWhiteSpace(callback.SourceNumber))
              scaffold.Errors.Add(
                $"Message 'Source Number' must be supplied");

            if (!string.IsNullOrWhiteSpace(callback.SourceNumber)
              && (!callback.SourceNumber.StartsWith("07")
              || !callback.SourceNumber.StartsWith("+447")))
              scaffold.Errors.Add(
                $"To: {callback.To} must be a mobile number");

            if (string.IsNullOrWhiteSpace(callback.Message))
              scaffold.Errors.Add(
           "Must contain a valid message string of less than 160 characters");

            if (callback.DateReceived == null)
              scaffold.Errors.Add("A valid UTC date must be supplied");
          }
        }
        else if (_type == typeof(SmsMessage))
        {
          SmsMessage smsMessage = param.Value as SmsMessage;
        }
        else
        {
          throw new ArgumentOutOfRangeException(
            $"Object type {_type} not Callback or SmsMessage");
        }

      }
      catch (Exception ex)
      {
        scaffold.Errors.Add($"Exception: {ex.Message}");
      }
      return !scaffold.Errors.Any();
    }
  }
}
