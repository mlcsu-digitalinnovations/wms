using System;

namespace WmsHub.Business.Models.ElectiveCareReferral;
public class ElectiveCarePostError
{
  public int Id { get; set; }

  /// <summary>
  /// This is the error as reported by the validation errors on the
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
