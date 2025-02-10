using System;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Helpers
{
    public static class BmiHelper
  {
    public static decimal CalculateBmi(decimal weightKg, decimal heightCm)
    {
      if (weightKg < Constants.MIN_WEIGHT_KG ||
        weightKg > Constants.MAX_WEIGHT_KG)
      {
        weightKg = 0;
      }

      if (heightCm < Constants.MIN_HEIGHT_CM ||
        heightCm > Constants.MAX_HEIGHT_CM)
      {
        heightCm = 1;
        weightKg = 0;
      }

      decimal result = Math.Round(weightKg / heightCm / heightCm * 10000, 1);
      return result;
    }
  }
}
