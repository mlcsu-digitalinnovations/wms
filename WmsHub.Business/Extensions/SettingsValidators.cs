using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ChatBotService;

namespace WmsHub.Business.Extensions
{
  public static class SettingsValidators
  {
    public static void Validate<T>(this T value) where T : class
    {
      if (typeof(T) == typeof(ArcusOptions))
      {
        if ((int) typeof(T).GetProperty("ReturnLimit").GetValue(value) == 0)
        {
          throw new ArgumentOutOfRangeException(
            "Expected environment variable, " +
            "WmsHub_ChatBot_Api_ArcusSettings:NumberWhiteList:ReturnLimit," +
            " was not found;");
        }
      }
    }


  }
}
