using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class NhsNumberUpdateReferralMismatchException : Exception
  {
    public NhsNumberUpdateReferralMismatchException() : base() { }
    public NhsNumberUpdateReferralMismatchException(
      string entityNhsNumber, string updateNhsNumber)
      : base($"The referral NHS number {entityNhsNumber} does not match " +
             $"the update NHS number {updateNhsNumber}.")
    { }
    public NhsNumberUpdateReferralMismatchException(string message) 
      : base(message) { }
    public NhsNumberUpdateReferralMismatchException(string message, 
      Exception inner)
      : base(message, inner)
    { }

    protected NhsNumberUpdateReferralMismatchException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}