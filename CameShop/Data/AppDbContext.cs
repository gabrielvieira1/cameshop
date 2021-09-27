using System;
using Cameshop.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cameshop.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<Item> catalog { get; set; }
  }
}