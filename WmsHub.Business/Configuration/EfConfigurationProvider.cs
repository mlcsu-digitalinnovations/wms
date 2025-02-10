using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace WmsHub.Business.Configuration
{
  internal class EfConfigurationProvider : ConfigurationProvider
  {
    public readonly Action<DbContextOptionsBuilder> _optionsAction;
    private readonly string _prefix;

    public EfConfigurationProvider(
      Action<DbContextOptionsBuilder> optionsAction,
      string prefix)
    {
      _optionsAction = optionsAction 
        ?? throw new ArgumentNullException(nameof(optionsAction));
      _prefix = prefix;
    }

    public override void Load()
    {
      var builder = new DbContextOptionsBuilder<DatabaseContext>();

      _optionsAction(builder);

      using var dbContext = new DatabaseContext(builder.Options);
      if (dbContext == null || dbContext.ConfigurationValues == null)
      {
        throw new Exception("Null DB context");
      }

      if (string.IsNullOrWhiteSpace(_prefix))
      {
        Data = dbContext.ConfigurationValues
          .ToDictionary(c => c.Id, c => c.Value);
      }
      else
      {
        Data = dbContext.ConfigurationValues
          .Where(c => c.Id.StartsWith(_prefix))
          .ToDictionary(c => c.Id.Replace(_prefix, ""), c => c.Value);
      }
    }
  }
}