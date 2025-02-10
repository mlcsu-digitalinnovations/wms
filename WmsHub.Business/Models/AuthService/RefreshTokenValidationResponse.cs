using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.AuthService
{
  public class RefreshTokenValidationResponse : BaseValidationResponse
  {
    public RefreshTokenValidationResponse(ValidationType validationStatus,
      string errorMessage = "")
    {
      if (string.IsNullOrWhiteSpace(errorMessage))
        ValidationStatus = validationStatus;
      else
        SetStatus(validationStatus, errorMessage);
    }
  }
}