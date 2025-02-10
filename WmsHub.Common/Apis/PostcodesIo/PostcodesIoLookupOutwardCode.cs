using System.Collections.Generic;

namespace WmsHub.Common.Apis.PostcodesIo;

public class PostcodesIoLookupOutwardCode
{
  public int Status { get; set; }
  public Result Result { get; set; }
}

public class Result
{
  public string Outcode { get; set; }
  public double Longitude { get; set; }
  public double Latitude { get; set; }
  public int Northings { get; set; }
  public int Eastings { get; set; }
  public List<string> Admin_district { get; set; }
  public List<string> Parish { get; set; }
  public List<string> Admin_county { get; set; }
  public List<string> Admin_ward { get; set; }
  public List<string> Country { get; set; }
  public List<string> Parliamentary_constituency { get; set; }
}

