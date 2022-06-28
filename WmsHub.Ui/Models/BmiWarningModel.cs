namespace WmsHub.Ui.Models
{
  public class BmiWarningModel : BaseModel
  {
    public decimal CalculatedBmiAtRegistration { get; set; }
    public string ServiceUserEthnicityGroup { get; set; }
  }
}
