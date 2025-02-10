namespace WmsHub.Business.Entities
{
  public interface IEthnicity
  {
    string DisplayName { get; set; }
    string GroupName { get; set; }
    string Census2001 { get; set; }
    string NhsDataDictionary2001Code { get; set; }
    string NhsDataDictionary2001Description { get; set; }
    string TriageName { get; set; }
    int GroupOrder { get; set; }
    int DisplayOrder { get; set; }
    decimal MinimumBmi { get; set; }
  }
}