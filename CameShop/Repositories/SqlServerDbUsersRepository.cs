using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cameshop.Data;
using Cameshop.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cameshop.Repositories
{
  public class SqlServerDbUsersRepository : IUsersRepository
  {
    private readonly AppDbContext dbContext;

    public SqlServerDbUsersRepository(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public async Task CreateUserAsync(User user)
    {
      await dbContext.Users.AddAsync(user);
      await dbContext.SaveChangesAsync();
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
      return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> GetUserAsync(Guid id)
    {
      return await dbContext.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
      return await dbContext.Users.ToListAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
      dbContext.Users.Update(user);
      await dbContext.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid id)
    {
      var user = await dbContext.Users.FindAsync(id);
      if (user != null)
      {
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
      }
    }
  }
}