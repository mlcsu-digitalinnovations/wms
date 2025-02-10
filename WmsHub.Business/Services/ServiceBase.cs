using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Common.Extensions;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Services
{
  public abstract class ServiceBase<TEntity> : IServiceBase
    where TEntity : BaseEntity
  {
    protected readonly DatabaseContext _context;

    public virtual ClaimsPrincipal User { get; set; }

    protected ServiceBase(DatabaseContext context)
    {
      _context = context;
    }

    public static Dictionary<string, string> TestConfig()
    {
      string key = DeprivationOptions.SectionKey;
      return new Dictionary<string, string>{

        { $"{key}:{nameof(DeprivationOptions.ImdResourceUrl)}",
          "https://assets.publishing.service.gov.uk/government/uploads/system" +
          "/uploads/attachment_data/file/833970/" +
          "File_1_-_IMD2019_Index_of_Multiple_Deprivation.xlsx" },

        { $"{key}:{nameof(DeprivationOptions.Col1)}", "LSOA code (2011)" },

        { $"{key}:{nameof(DeprivationOptions.Col2)}",
          "Index of Multiple Deprivation (IMD) Decile" }
      };
    }

    public async Task<int> ActivateAsync(int id)
    {
      return await SetActiveStatus(id, isActivating: true);
    }
    public async Task<int> DeactivateAsync(int id)
    {
      return await SetActiveStatus(id, isActivating: false);
    }

    protected virtual void UpdateModified(BaseEntity entity)
    {
      if (User is null)
      {
        throw new ClaimsPrincipalNullException(nameof(User));
      }

      entity.ModifiedByUserId = User.GetUserId();
      entity.ModifiedAt = DateTimeOffset.Now;
    }

    protected virtual ValidateModelResult ValidateModel(object model)
    {
      ValidationContext context = new ValidationContext(instance: model);

      ValidateModelResult result = new ValidateModelResult();
      result.IsValid = Validator.TryValidateObject(
        model, context, result.Results, validateAllProperties: true);

      return result;
    }

    private async Task<int> SetActiveStatus(int id, bool isActivating)
    {
      TEntity entity = await _context.Set<TEntity>()
                                     .FindAsync(id);

      if (entity == null)
      {
        throw new ModelStateException("Id",
          $"A {typeof(TEntity).Name} with an id of {id} was not found.");
      }
      if (entity.IsActive == isActivating)
      {
        throw new ModelStateException("Id",
          $"{typeof(TEntity).Name} with an id of {id} is already " +
          $"{(isActivating ? "active" : "inactive")}.");
      }

      entity.IsActive = isActivating;
      UpdateModified(entity);
      return await _context.SaveChangesAsync();
    }
  }
}