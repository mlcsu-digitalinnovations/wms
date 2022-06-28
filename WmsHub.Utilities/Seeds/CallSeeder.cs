using System;
using WmsHub.Business.Entities;

namespace WmsHub.Utilities.Seeds
{
  public class CallSeeder : SeederBase<Call>
  {
    public static readonly Guid CHATBOT_API_USER_ID =
       new Guid("eafc7655-89b7-42a3-bdf7-c57c72cd1d41");
  }
}