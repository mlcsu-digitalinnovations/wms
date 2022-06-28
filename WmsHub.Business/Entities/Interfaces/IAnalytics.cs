using System;

namespace WmsHub.Business.Entities
{
  public interface IAnalytics
  {
    Guid LinkId { get; set; }

    /// <summary>
    /// The table the link id was provided for
    /// </summary>
    string LinkDescription { get; set; }

    /// <summary>
    /// The value of the field to be analised
    /// </summary>
    string Value { get; set; }

    /// <summary>
    /// Lookup enum from PropertyLookupEnum
    /// </summary>
    int PropertyLookup { get; set; }

    Guid Id { get; set; }
    bool IsActive { get; set; }
    DateTimeOffset ModifiedAt { get; set; }
    Guid ModifiedByUserId { get; set; }
  }
}