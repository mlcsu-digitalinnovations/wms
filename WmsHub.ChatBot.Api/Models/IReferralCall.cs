using System;

namespace WmsHub.ChatBot.Api.Models
{
  public interface IReferralCall
  {
    Guid? Id { get; set; }
    string Outcome { get; set; }
  }
}