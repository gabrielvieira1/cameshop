using Cameshop.Entities;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace Cameshop.Repositories
{
  public class InMemUsersRepository : IUsersRepository
  {
    private const string FilePath = "users.json";
    private List<User> _users;

    public InMemUsersRepository()
    {
      if (File.Exists(FilePath))
      {
        var json = File.ReadAllText(FilePath);
        _users = JsonSerializer.Deserialize<List<User>>(json);
      }
      else
      {
        _users = new List<User>();
      }
    }

    public async Task<User> CreateUserAsync(User user)
    {
      _users.Add(user);
      await SaveToFileAsync();
      return user;
    }

    public Task DeleteUserAsync(Guid id)
    {
      throw new NotImplementedException();
    }

    public Task<User> GetUserAsync(Guid id)
    {
      throw new NotImplementedException();
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
      return await Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
    }

    public Task<IEnumerable<User>> GetUsersAsync()
    {
      throw new NotImplementedException();
    }

    public Task UpdateUserAsync(User user)
    {
      throw new NotImplementedException();
    }

    Task IUsersRepository.CreateUserAsync(User user)
    {
      return CreateUserAsync(user);
    }

    private async Task SaveToFileAsync()
    {
      var json = JsonSerializer.Serialize(_users);
      await File.WriteAllTextAsync(FilePath, json);
    }
  }
}
