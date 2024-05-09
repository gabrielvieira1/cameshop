using Cameshop.Entities;
using System.Threading.Tasks;

namespace Cameshop.Repositories
{
  public interface IUsersRepository
  {
    Task<User> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
  }
}
