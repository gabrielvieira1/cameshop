using Cameshop.Dtos;
using Cameshop.Entities;
using Cameshop.Extensions;
using Cameshop.Repositories;
using Cameshop.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cameshop.Controllers
{
  [EnableCors("MyPolicy")]
  [ApiController]
  [Route("[controller]")]
  public class UsersController : ControllerBase
  {
    private readonly IUsersRepository _usersRepository;
    private readonly ILogger<UsersController> logger;
    private readonly JwtTokenGenerator _tokenGenerator;

    public UsersController(IUsersRepository usersRepository, ILogger<UsersController> logger, JwtTokenGenerator tokenGenerator)
    {
      _usersRepository = usersRepository;
      this.logger = logger;
      _tokenGenerator = tokenGenerator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUserAsync(Guid id)
    {
      var user = await _usersRepository.GetUserAsync(id);
      if (user is null)
      {
        return NotFound();
      }

      return Ok(user.AsDto());
    }

    [HttpGet]
    public async Task<IEnumerable<UserResponseDto>> GetUsersAsync()
    {
      var users = await _usersRepository.GetUsersAsync();
      return users.Select(user => user.AsDto());
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
        Id = Guid.NewGuid(),
        Name = model.Name,
        Email = model.Email,
        PasswordHash = model.Password, // Em produção, use um hash com salt.
        CreatedDate = DateTimeOffset.UtcNow
      };

      await _usersRepository.CreateUserAsync(newUser);

      var responseDto = new UserResponseDto(
          newUser.Id,
          newUser.Name,
          newUser.Email,
          newUser.CreatedDate
      );

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

      //var responseDto = new UserResponseDto(
      //    user.Id,
      //    user.Name,
      //    user.Email,
      //    user.CreatedDate
      //);

      //return Ok(responseDto);

      var token = _tokenGenerator.GenerateToken(user);

      return Ok(new
      {
        Token = token
      });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserAsync(Guid id, UserRegisterDto model)
    {
      var user = await _usersRepository.GetUserAsync(id);
      if (user is null)
      {
        return NotFound();
      }

      user.Name = model.Name;
      user.Email = model.Email;
      user.PasswordHash = model.Password;

      await _usersRepository.UpdateUserAsync(user);
      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserAsync(Guid id)
    {
      var user = await _usersRepository.GetUserAsync(id);
      if (user is null)
      {
        return NotFound();
      }

      await _usersRepository.DeleteUserAsync(id);
      return NoContent();
    }
  }
}
