using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace WmsHub.Business.Configuration
{
  public class EfConfigurationSource : IConfigurationSource
  {
    private readonly Action<DbContextOptionsBuilder> _optionsAction;
    private readonly string _prefix;

    public EfConfigurationSource(
      Action<DbContextOptionsBuilder> optionsAction)
    {
      _optionsAction = optionsAction
        ?? throw new ArgumentNullException(nameof(optionsAction));
    }

    public EfConfigurationSource(
      Action<DbContextOptionsBuilder> optionsAction, 
      string prefix) 
      : this(optionsAction)
    {
      if (string.IsNullOrWhiteSpace(prefix))
      {
        throw new ArgumentException(
          $"'{nameof(prefix)}' cannot be null or whitespace.", nameof(prefix));
      }
      _prefix = prefix;
    }    

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new
      EfConfigurationProvider(_optionsAction, _prefix);
  }
}
