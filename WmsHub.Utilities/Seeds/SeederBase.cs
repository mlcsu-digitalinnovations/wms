using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WmsHub.Business;
using WmsHub.Business.Entities;

namespace WmsHub.Utilities.Seeds
{
  public class SeederBase<TEntity> : SeederBaseBase
    where TEntity : BaseEntity, new()
  {
    public readonly static Guid SYSTEM_ID =
      new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

    protected DateTimeOffset _now = DateTimeOffset.Now;

    public virtual void DeleteSeeds()
    {
      DatabaseContext.Set<TEntity>().RemoveRange(
        DatabaseContext.Set<TEntity>().ToList()
      );
      ResetIdentity();
    }

    internal virtual void ResetIdentity(int newReseedValue = 0)
    {
      string tableName = GetEntityTableName();

      DatabaseContext.Database.ExecuteSqlRaw(
        "IF EXISTS (SELECT * FROM sys.identity_columns WHERE " +
        $"object_id = OBJECT_ID('dbo.{tableName}') AND last_value IS NOT " +
        $"NULL) DBCC CHECKIDENT('{tableName}', RESEED, {newReseedValue})");
    }

    private static string GetEntityTableName()
    {
      return DatabaseContext.Model.GetEntityTypes()
        .First(t => t.ClrType == typeof(TEntity)).GetAnnotations()
        .First(a => a.Name == "Relational:TableName").Value.ToString();
    }
  }
}