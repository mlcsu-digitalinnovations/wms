using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WmsHub.Business.Helpers;

namespace WmsHub.Business.Models.BusinessIntelligence;

public class BiRmcUserInformation
{
  private Dictionary<string, string> Params =>
    HttpUtility.ParseQueryString(Request.Query).AllKeys.ToDictionary(
      k => k, k => HttpUtility.ParseQueryString(Request.Query)[k]);

  public BiRmcUserInformation(
    string action,
    string ownerName,
    string request,
    DateTimeOffset requestAt,
    Guid userId
   )
  {
    Action = action;
    ActionDateTime = requestAt;
    Request = string.IsNullOrWhiteSpace(request)
      ? null
      : new Uri(request.Replace("|", "?"));
    UserId = userId;
    Username = ownerName;
  }

  public string Action { get; private set; }
  public DateTimeOffset ActionDateTime { get; private set; }
  public string DelayReason =>
    Action == "ConfirmDelay" ? Params["DelayReason"] : null;
  public Uri Request { get; private set; }
  public string StatusReason => Action switch
  {
    "AddToRmcCallList" => Params["StatusReason"],
    "RejectToEreferrals" => Params["StatusReason"],
    "UnableToContact" => Params["StatusReason"],
    _ => null,
  };
  public string Ubrn => QueryStringHelpers.TryFindUbrn(Request);
  public Guid UserId { get; private set; }
  public string Username { get; private set; }
}
