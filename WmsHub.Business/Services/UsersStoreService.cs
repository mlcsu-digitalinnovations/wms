using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Services
{
  public interface IUsersStoreService : IServiceBase
  {
    Task<string> LoadAsync(List<UserStore> users);
  }

  public class UsersStoreService : 
    ServiceBase<Entities.UserStore>, IUsersStoreService
  {

    public UsersStoreService(DatabaseContext context) : base(context)
    { }

    public async Task<string> LoadAsync(List<UserStore> users)
    {
      if (users is null)
      {
        throw new ArgumentNullException(nameof(users));
      }

      List<Entities.UserStore> addedUsers = new();
      foreach (UserStore user in users)
      {
        Entities.UserStore entity = await _context
          .UsersStore
          .FindAsync(user.Id);
        
        if (entity == null)
        {
          addedUsers.Add(new()
          {
            ApiKey = user.ApiKey,
            Domain = user.Domain,
            Expires = user.Expires,
            ForceExpiry = user.ForceExpiry,
            Id = user.Id,
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = User.GetUserId(),
            OwnerName = user.OwnerName,
            Scope = user.Scope
          });
        }
      }
      _context.UsersStore.AddRange(addedUsers);
      await _context.SaveChangesAsync();

      return $"Added {addedUsers.Count} users out of {users.Count}.";
    }
  }
}
