namespace WmsHub.Business.Enums
{
  /// <summary>
  /// 'Replace' will create the list and halt processing of all other call 
  /// lists for the specified contact flow. 'Append' will create the list and 
  /// add it to the queue of lists to be processed for this contact flow.
  /// </summary>
  public enum ModeType
  {
    Replace,
    Append
  }
}
