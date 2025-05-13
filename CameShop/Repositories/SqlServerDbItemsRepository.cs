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
      await itemsCollection.Items.AddAsync(item);
      await itemsCollection.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(Guid id)
    {
      var filter = itemsCollection.Items.FirstOrDefault(item => item.Id == id);

      if (filter != null)
      {
        itemsCollection.Items.Remove(filter);

        await itemsCollection.SaveChangesAsync();
      }
    }
    public async Task<Item> GetItemAsync(Guid id)
    {
      var filter = await itemsCollection.Items.FindAsync(id);
      return filter;
    }

    public async Task UpdateItemAsync(Item item)
    {
      var filter = itemsCollection.Items.FirstOrDefault(item => item.Id == item.Id);
      if (filter != null)
      {
        itemsCollection.Items.Update(filter);

        await itemsCollection.SaveChangesAsync();
      }
    }
    public Task<List<Item>> GetItemsAsync() => Task.FromResult(itemsCollection.Items.ToList());
  }
}