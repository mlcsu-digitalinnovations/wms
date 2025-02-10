using System;

namespace WmsHub.Common.Exceptions;
public class EnumNoMemberException : Exception
{
  public EnumNoMemberException(Type enumType) : base($"{enumType.Name} has no members.")
  {
  }

  public EnumNoMemberException(string message) : base(message)
  {
  }

  public EnumNoMemberException(string message, Exception innerException)
    : base(message, innerException)
  {
  }
}
