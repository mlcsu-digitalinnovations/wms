namespace WmsHub.Business.Enums;

public enum ValidationType
{
  NotSet,
  Invalid,
  KeyNotSet,
  KeyNotFound,
  KeyNotRecognised,
  KeyIsNotNumeric,
  KeyApiKeyMismatch,
  KeyIncorrectLength,
  KeyOutOfDate,
  /// <summary>
  /// The request is missing a parameter so the server can’t proceed with
  /// the request. This may also be returned if the request includes an
  /// unsupported parameter or repeats a parameter.
  /// </summary>
  InvalidRequest,
  /// <summary>
  /// Client authentication failed, such as if the request contains an
  /// invalid client ID or secret. Send an HTTP 401 response in this case.
  /// </summary>
  InvalidClient,
  /// <summary>
  /// The authorization code
  /// (or user’s password for the password grant type)
  /// is invalid or expired. This is also the error you would return if
  /// the redirect URL given in the authorization grant does not match
  /// the URL provided in this access token request.
  /// </summary>
  InvalidGrant,
  /// <summary>
  /// For access token requests that include a scope
  /// (password or client_credentials grants),
  /// this error indicates an invalid scope value in the request.
  /// </summary>
  InvalidScope,
  /// <summary>
  /// This client is not authorized to use the requested grant type.
  /// For example, if you restrict which applications can use the
  /// Implicit grant, you would return this error for the other apps.
  /// </summary>
  UnauthorisedClient,
  /// <summary>
  /// f a grant type is requested that the authorization server doesn't
  /// recognize, use this code. Note that unknown grant types also use
  /// this specific error code rather than using the invalid_request above.
  /// </summary>
  UnsupportedGrantType,
  /// <summary>
  /// The IsActive field for the associated provider has been set to false,
  /// and therefore generating new access tokens is forbidden.
  /// </summary>
  InactiveProvider,
  ValidKey,
  Valid

}
