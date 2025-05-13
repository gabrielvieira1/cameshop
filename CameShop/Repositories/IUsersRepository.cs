using Cameshop.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cameshop.Repositories
{
  public interface IUsersRepository
  {
    Task<User> GetUserAsync(Guid id);
    Task<IEnumerable<User>> GetUsersAsync();
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(Guid id);
    Task<User> GetUserByEmailAsync(string email);
  }
}
