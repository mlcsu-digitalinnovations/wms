using Microsoft.Extensions.Options;
using MiniValidation;

namespace WmsHub.AzureFunctions.Validation;

public class MiniValidationValidateOptions<TOptions>(string? name)
  : IValidateOptions<TOptions> where TOptions : class
{
  public string? Name { get; } = name;

  public ValidateOptionsResult Validate(string? name, TOptions options)
  {
    // Null name is used to configure ALL named options, so always applys.
    if (Name != null && Name != name)
    {
      // Ignored if not validating this instance.
      return ValidateOptionsResult.Skip;
    }

    // Ensure options are provided to validate against
    ArgumentNullException.ThrowIfNull(options);

    // MiniValidation validation
    if (MiniValidator.TryValidate(options, out IDictionary<string, string[]>? validationErrors))
    {
      return ValidateOptionsResult.Success;
    }

    string typeName = options.GetType().Name;
    List<string> errors = [];
    foreach ((string member, string[] memberErrors) in validationErrors)
    {
      errors.Add($"DataAnnotation validation failed for '{typeName}' member: '{member}' with " +
        $"errors: '{string.Join("', '", memberErrors)}'.");
    }

    return ValidateOptionsResult.Fail(errors);
  }
}