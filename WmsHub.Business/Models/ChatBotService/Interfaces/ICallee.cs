namespace WmsHub.Business.Models.ChatBotService
{
  public interface ICallee
  {
    string CallAttempt { get; set; }
    string Id { get; set; }
    string PrimaryPhone { get; set; }
    string SecondaryPhone { get; set; }
    string ServiceUserName { get; set; }
  }
}