using Cameshop.Entities;
using static Cameshop.Dtos.ItemDtos;

namespace Cameshop.Extensions
{
  public static class ItemExtensions
  {
    public static ItemDto AsDto(this Item item)
    {
      return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
    }
  }
}