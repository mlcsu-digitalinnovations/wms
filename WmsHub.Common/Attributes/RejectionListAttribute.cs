#nullable enable
using System;

namespace WmsHub.Common.Attributes;

public class RejectionListAttribute : Attribute
{
  public RejectionListAttribute(bool isRejectionList)
  {
    IsRejectionList = isRejectionList;
  }
  public RejectionListAttribute():this(true){}

  public bool IsRejectionList { get; set; }
}