using WmsHub.Common.Helpers;

namespace WmsHub.ErsMock.Api.Models.ReferralRequest;

public class RecordReviewOutcomeRequest : ARequestBase
{
  private const string EXPECTED_REVIEW_OUTCOME =
    "RETURN_TO_REFERRER_WITH_ADVICE";
  private const string EXPECTED_REVIEW_PRIORITY = "ROUTINE";

  public List<string> Errors = new();

  public string? ReviewComments => Parameter
    ?.FirstOrDefault(x => x.Name == "reviewComments")
    ?.ValueString;

  public string? ReviewOutcome => Parameter
    ?.FirstOrDefault(x => x.Name == "reviewOutcome")
    ?.ValueCoding
    ?.Code;

  public string? ReviewPriority => Parameter
    ?.FirstOrDefault(x => x.Name == "reviewPriority")
    ?.ValueCoding
    ?.Code;

  public bool IsValid()
  {
    bool isValid = true;

    if (ReviewComments != Common.Helpers.Constants.ErsOutcomeMessage)
    {
      Errors.Add(
        "Review comments: expected '" +
        $"{Common.Helpers.Constants.ErsOutcomeMessage}', " +
        $"received: '{ReviewOutcome}'");
      isValid = false;
    }

    if (ReviewOutcome != EXPECTED_REVIEW_OUTCOME)
    {
      Errors.Add(
        $"Review outcome: expected '{EXPECTED_REVIEW_OUTCOME}', " +
        $"received: '{ReviewOutcome}'");
      isValid = false;
    }

    return isValid; 
  }    
}
