using Swashbuckle.AspNetCore.Annotations;
using System;

namespace WmsHub.BusinessIntelligence.Api.SwaggerSchema;

/// <summary>
/// Extending the SwaggerSchemaAttribute allows the setting of:<br />
/// 1. Description<br />
/// 2. Enum Type<br />
/// 3. Format<br />
/// 4. Nullable<br />
/// 5. Title<br />
/// 6. Required<br />
/// However, it does not allow for the setting of Example.  If you need to set
/// the example use the AnonymisedReferralSchemaFilter.
/// </summary>
public class WmsSwaggerSchemaAttribute : SwaggerSchemaAttribute
{
  public WmsSwaggerSchemaAttribute(string description = null) 
    : base(description)
  {
  }
  public WmsSwaggerSchemaAttribute(
    string description = null, Type enumType = null)
  {
    description += string.Join("<br />", Enum.GetNames(enumType));
    base.Description = description;
  }

}
