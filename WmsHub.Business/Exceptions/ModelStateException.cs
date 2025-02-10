using System;

namespace WmsHub.Business.Exceptions;

[Serializable()]
public class ModelStateException : Exception
{
  public ModelStateException(string[] keys, string message)
    : base(message)
  {
    Keys = keys;
  }

  public ModelStateException(string key, string message)
    : base(message)
  {
    Keys = new string[] { key };
  }

  public string[] Keys { get; private set; }
}