using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cameshop.Entities;

namespace Cameshop.Repositories
{
  public interface IItemsRepository
  {
    Task<Item> GetItemAsync(Guid id);
    Task<List<Item>> GetItemsAsync();
    Task CreateItemAsync(Item item);
    Task UpdateItemAsync(Item item);
    Task DeleteItemAsync(Guid id);
  }
}