using System;

namespace WmsHub.Common.Models
{
  public class ErsAttachment : IComparable<ErsAttachment>
  {
    public string Id { get; set; }
    public string ContentType { get; set; }
    public string Url { get; set; }
    public int Size { get; set; }
    public string Title { get; set; }
    public DateTime Creation { get; set; }
    
    public string FileExtension {
      get
      {
        return System.IO.Path.GetExtension(Title).ToUpper().Replace(".","");
      }
    }
    
    public int CompareTo(ErsAttachment other)
    {
      return -Id.CompareTo(other.Id);
    }
  }
}
