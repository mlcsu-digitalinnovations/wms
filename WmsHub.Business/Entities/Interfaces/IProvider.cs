namespace WmsHub.Business.Entities
{
  public interface IProvider
  {
    bool Level1 { get; set; }
    bool Level2 { get; set; }
    bool Level3 { get; set; }
    string Logo { get; set; }
    string Name { get; set; }
    string Summary { get; set; }
    string Website { get; set; }
  }
}