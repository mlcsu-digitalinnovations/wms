﻿using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class InvalidOptionsException : Exception
{
  public InvalidOptionsException() : base() { }
  public InvalidOptionsException(string message) : base(message) { }
  public InvalidOptionsException(string message, Exception inner)
    : base(message, inner)
  { }
}