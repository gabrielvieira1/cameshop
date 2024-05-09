using System;

namespace Cameshop.Dtos
{
  public class UserDto
  {
    public string Name { get; set; }
    public string Email { get; set; }
  }

  public class UserRegisterDto : UserDto
  {
    public string Password { get; set; }
  }

  public class UserLoginDto
  {
    public string Email { get; set; }
    public string Password { get; set; }
  }

  public class UserResponseDto : UserDto
  {
    public Guid Id { get; set; }
  }
}
