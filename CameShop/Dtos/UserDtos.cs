using System;
using System.ComponentModel.DataAnnotations;

namespace Cameshop.Dtos
{
  public record UserRegisterDto(
      [Required] string Name,
      [Required][EmailAddress] string Email,
      [Required] string Password);

  public record UserLoginDto(
      [Required][EmailAddress] string Email,
      [Required] string Password);

  public record UserResponseDto(
    Guid Id,
    string Name,
    string Email,
    DateTimeOffset CreatedDate,
    bool Active);
}
