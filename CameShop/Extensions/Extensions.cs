using Cameshop.Entities;
using static Cameshop.Dtos.Dtos;

namespace Cameshop.Extensions
{
  public static class Extensions
  {
    public static ItemDto AsDto(this Item item)
    {
      return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
    }
  }
}