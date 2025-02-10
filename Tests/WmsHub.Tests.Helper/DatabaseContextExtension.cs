using Microsoft.EntityFrameworkCore;
using WmsHub.Business;
using WmsHub.Business.Entities;

namespace WmsHub.Tests.Helper;
public static class DatabaseContextExtension
{
  public static void AddSaveAndDetachEntity<T>(this DatabaseContext context, T entity)
    where T : BaseEntity
  {
    _ = context.Set<T>().Add(entity);
    _ = context.SaveChanges();
    context.Entry(entity).State = EntityState.Detached;
  }
}