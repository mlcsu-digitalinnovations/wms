using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models
{
  public class Deprivation
  {
    public Deprivation()
    { }

    public Deprivation(int imdDecile, string lsoa)
    {
      ImdDecile = imdDecile;
      Lsoa = lsoa ?? throw new ArgumentNullException(nameof(lsoa));
    }

    [Range(1,10)]
    public int ImdDecile { get; set; }
    
    [Required]
    public string Lsoa { get; set; }

    public Enums.Deprivation ImdQuintile()
    {
      return
        (Enums.Deprivation)Enum.Parse(
          typeof(Enums.Deprivation),
          $"IMD{ImdQuintileValue()}");
    }

    public int ImdQuintileValue()
    {
      if (ImdDecile >= 1 && ImdDecile <= 10)
        return Convert.ToInt32(
          Math.Round((ImdDecile * 0.5), 0, MidpointRounding.AwayFromZero));
      else
        throw new ArgumentOutOfRangeException(nameof(ImdDecile));
    }
  }
}