using System;

namespace WmsHub.Business.Entities;

/// <summary>
/// ElectiveCarePostErrors is a flattened container class for Elective Care Post
/// Errors.  Each column is a boolean of which property failed, but without
/// any reason which could contain user data.
/// <br />Each row only contains a single data point, and the reference is by
/// ProcessDate and RowNumber.
/// </summary>
public class ElectiveCarePostError
{
  public int Id { get; set; }

  /// <summary>
  /// This is the error as reported by the validation error on the
  /// ElectiveCareReferralTrustData.
  /// </summary>
  public string PostError { get; set; }
  /// <summary>
  /// Date the file was processed.
  /// </summary>
  public DateTimeOffset ProcessDate { get; set; }
  /// <summary>
  /// Row number of the file processed.
  /// </summary>
  public int RowNumber { get; set; }
  /// <summary>
  /// The Trust ODS Code used to process the data.
  /// </summary>
  public string TrustOdsCode { get; set; }
  /// <summary>
  /// The Id of the user processing the data.
  /// </summary>
  public Guid TrustUserId { get; set; }
}
