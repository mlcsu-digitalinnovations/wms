namespace WmsHub.Common.Apis.Ods.PostcodesIo;

public class PostcodesIoLookupPostcode
{
  public string Error { get; set; }
  public int Status { get; set; }
  public Result Result { get; set; }
}

public class Result
{
  public string Admin_County { get; set; }
  public string Admin_District { get; set; }
  public string Admin_Ward { get; set; }
  public string Ccg { get; set; }
  public string Ced { get; set; }
  public Code Codes { get; set; }
  public string Country { get; set; }
  public int Eastings { get; set; }
  public string Incode { get; set; }
  public string European_Electoral_Region { get; set; }
  public decimal Latitude { get; set; }
  public decimal Longitude { get; set; }
  public string Lsoa { get; set; }
  public string Msoa { get; set; }
  public string Nhs_ha { get; set; }
  public int Northings { get; set; }
  public string Nuts { get; set; }
  public string Outcode { get; set; }
  public string Parish { get; set; }
  public string Parliamentary_constituency { get; set; }
  public string Postcode { get; set; }
  public string Primary_care_trust { get; set; }
  public int Quality { get; set; }
  public string Region { get; set; }

}

public class Code
{
  public string Admin_District { get; set; }
  public string Admin_County { get; set; }
  public string Admin_Ward { get; set; }
  public string Parish { get; set; }
  public string Parliamentary_Constituency { get; set; }
  public string Ccg { get; set; }
  public string Ccg_Id { get; set; }
  public string Ced { get; set; }
  public string Nuts { get; set; }
  public string Lsoa { get; set; }
  public string Msoa { get; set; }
  public string Lau2 { get; set; }
}
