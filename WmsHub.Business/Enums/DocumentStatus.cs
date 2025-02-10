namespace WmsHub.Business.Enums;

public enum DocumentStatus
{
  OrganisationNotSupported = 0,
  DischargePending = 1,
  Received = 4002,
  Delivered = 4003,
  Rejected = 4005,
  Accepted = 4010,
  SystemError = 5000,
  RejectionResolved = 6000,
  SystemErrorOnHold = 7000
}