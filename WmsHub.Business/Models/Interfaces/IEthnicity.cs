using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models
{
  public interface IEthnicity : IBaseModel
  {
    string DisplayName { get; set; }
    string GroupName { get; set; }
    string Census2001 { get; set; }
    string NhsDataDictionary2001Code { get; set; }
    string NhsDataDictionary2001Description { get; set; }
    string TriageName { get; set; }
    decimal? MinimumBmi { get; set; }
    int GroupOrder { get; set; }
    int DisplayOrder { get; set; }
  }
}