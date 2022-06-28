using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
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
      Keys = new string[]{key};
    }
    protected ModelStateException(SerializationInfo info,
                                  StreamingContext context)
      : base(info, context)
    {
    }
    
    public string[] Keys { get; private set; }
  }
}