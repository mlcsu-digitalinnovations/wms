using System;
using System.Text;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Entities
{
  public class TextMessage : TextMessageBase, ITextMessage
  {
    public TextMessage() { }

    public TextMessage(ICallbackRequest request)
    {
      var valid = Guid.TryParse(request.Id, out var id);
      if (!valid)
      {
        throw new ArgumentOutOfRangeException(
          $"Id of '{request.Id}' is not a valid Guid");
      }
      Id = id;
      IsActive = true;
      ModifiedAt = DateTimeOffset.Now;
      //Modified by will require a user ID for the system - As this is a
      // webapi, it may be a default Guid
      //ModifiedByUserId = request.ModifiedByUserId;
      if (Guid.TryParse(request.Reference, out var refId)) ReferralId = refId;

      Sent = request.CreatedAt;
      Received = request.SentAt;
      Outcome = request.Status.ToString();
      Number = request.To;
    }

    public TextMessage(TextMessageRequest request)
    {
			DateTimeOffset dateSent = DateTimeOffset.Now;

			Sent = dateSent.DateTime;
			Base36DateSent = Base36Converter.ConvertDateTimeOffsetToBase36(dateSent);
      Number = request.MobileNumber.ConvertToUkMobileNumber(false);
      IsActive = true;
    }

    public virtual Referral Referral { get; set; }

    public virtual string Update(ICallbackRequest model)
    {
      var sb = new StringBuilder();
      // Reference must match or throw exception
      Guid.TryParse(model.Reference, out Guid id);
      if (Id != id) throw new ArgumentException("Update IDs do not match");

      if (model.SentAt != null && Received != model.SentAt)
      {
        sb.AppendLine($"Received updated from {Received} to {model.SentAt}; ");
        Received = model.SentAt.GetValueOrDefault();
      }

      if (Outcome != model.Status)
      {
        sb.AppendLine($"Outcome updated from {Outcome} to {model.Status}; ");
        Outcome = model.Status;
      }

      return sb.ToString();
    }

    //public virtual string Update(ISmsMessage model)
    //{
    //  var sb = new StringBuilder();
    //  // Id must match or throw exception
    //  if (Id != model.LinkedTextMessage.Value) 
    //    throw new ArgumentException("Update IDs do not match");

    //  Outcome = "SENT";

    //  if (model.Sent != null && Sent != model.Sent)
    //  {
    //    sb.AppendLine($"Sent updated from {Sent} to {model.Sent}; ");
    //    Sent = DateTime.SpecifyKind(model.Sent.Value, DateTimeKind.Utc);
    //  }

    //  Base36DateSent = model.Base36DateSent;

    //  return sb.ToString();
    //}
  }
}