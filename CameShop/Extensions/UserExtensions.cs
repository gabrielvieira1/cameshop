using Cameshop.Dtos;
using Cameshop.Entities;
using static Cameshop.Dtos.ItemDtos;

namespace Cameshop.Extensions
{
  public static class UserExtensions
  {
    public static UserResponseDto AsDto(this User user)
    {
      return new UserResponseDto(user.Id, user.Name, user.Email, user.CreatedDate, user.Active);
    }
  }
}