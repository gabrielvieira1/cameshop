using Cameshop.Dtos;
using Cameshop.Entities;
using Cameshop.Repositories;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Cameshop.Controllers
{
  [EnableCors("MyPolicy")]
  [ApiController]
  [Route("[controller]")]
  public class UsersController : ControllerBase
  {
    private readonly IUsersRepository _usersRepository;

    public UsersController(IUsersRepository usersRepository)
    {
      _usersRepository = usersRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto model)
    {
      var existingUser = await _usersRepository.GetUserByEmailAsync(model.Email);
      if (existingUser != null)
      {
        return Conflict("O email já está em uso.");
      }

      var newUser = new User
      {
        Name = model.Name,
        Email = model.Email,
        PasswordHash = model.Password
      };

      await _usersRepository.CreateUserAsync(newUser);

      var responseDto = new UserResponseDto
      {
        Id = newUser.Id,
        Name = newUser.Name,
        Email = newUser.Email
      };

      return Ok(responseDto);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto model)
    {
      var user = await _usersRepository.GetUserByEmailAsync(model.Email);
      if (user == null || user.PasswordHash != model.Password)
      {
        return Unauthorized("Credenciais inválidas.");
      }

      var responseDto = new UserResponseDto
      {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email
      };

      return Ok(responseDto);
    }
  }
}
