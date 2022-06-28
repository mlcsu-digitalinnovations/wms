namespace WmsHub.Business.Enums
{
  public enum ChatBotCallOutcome : int
  {
    Undefined,
    CallerReached,
    TransferredToPhoneNumber,
    TransferredToQueue,
    TransferredToVoicemail,
    VoicemailLeft,
    Connected,
    HungUp,
    Engaged,
    CallGuardian,
    NoAnswer,
    InvalidNumber,
    Error,
    TransferringToRmc
  }
}
