namespace WmsHub.Business.Enums;

public enum ProgrammeOutcome : int
{
  NotSet = 0,
  DidNotCommence = 1,
  DidNotComplete = 2,
  Complete = 3,
  RejectedBeforeProviderSelection = 4,
  RejectedAfterProviderSelection = 5,
  FailedToContact = 6,
  InvalidContactDetails = 7
}
