using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cameshop.Data;
using Cameshop.Entities;

namespace Cameshop.Repositories
{
  public class SqlServerDbItemsRepository : IItemsRepository
  {
    private const string databaseName = "catalog";
    private const string collectionName = "items";
    private readonly AppDbContext itemsCollection;
    public SqlServerDbItemsRepository(AppDbContext sqlClient)
    {
      itemsCollection = sqlClient;
    }

    public async Task CreateItemAsync(Item item)
    {
      await itemsCollection.catalog.AddAsync(item);
      await itemsCollection.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(Guid id)
    {
      var filter = itemsCollection.catalog.FirstOrDefault(item => item.Id == id);

      if (filter != null)
      {
        itemsCollection.catalog.Remove(filter);

        await itemsCollection.SaveChangesAsync();
      }
    }
    public async Task<Item> GetItemAsync(Guid id)
    {
      var filter = await itemsCollection.catalog.FindAsync(id);
      return filter;
    }

    public async Task UpdateItemAsync(Item item)
    {
      var filter = itemsCollection.catalog.FirstOrDefault(item => item.Id == item.Id);
      if (filter != null)
      {
        itemsCollection.catalog.Update(filter);

        await itemsCollection.SaveChangesAsync();
      }
    }
    public Task<List<Item>> GetItemsAsync() => Task.FromResult(itemsCollection.catalog.ToList());
  }
}