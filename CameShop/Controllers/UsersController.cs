using Cameshop.Dtos;
using Cameshop.Entities;
using Cameshop.Extensions;
using Cameshop.Repositories;
using Cameshop.Services;
using Cameshop.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cameshop.Controllers
{
  [EnableCors("AllowAll")]
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

    [Authorize(Roles = "Admin,Cliente")]
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

    [Authorize(Roles = "Admin,Cliente")]
    [HttpGet]
    public async Task<IEnumerable<UserResponseDto>> GetUsersAsync()
    {
      var users = await _usersRepository.GetUsersAsync();
      return users.Select(user => user.AsDto());
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto model)
    {
      if (!Utils.String.InputIsValid(model.Name))
        ModelState.AddModelError(nameof(model.Name), "Nome inválido.");

      if (!Utils.String.EmailIsValid(model.Email))
        ModelState.AddModelError(nameof(model.Email), "Email inválido.");

      if (!Utils.Security.PasswordIsValid(model.Password))
        ModelState.AddModelError(nameof(model.Password), "A senha não atende aos requisitos.");

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        var existingUser = await _usersRepository.GetUserByEmailAsync(model.Email);
        if (existingUser != null)
        {
          return Conflict("O e-mail informado já está cadastrado.");
        }

        var newUser = new User
        {
          Id = Guid.NewGuid(),
          Name = model.Name,
          Email = model.Email,
          PasswordHash = Utils.Security.HashPassword(model.Password),
          CreatedDate = DateTimeOffset.UtcNow,
          Active = true,
          Role = "Cliente"
        };

        await _usersRepository.CreateUserAsync(newUser);

        var responseDto = new UserResponseDto(
            newUser.Id,
            newUser.Name,
            newUser.Email,
            newUser.CreatedDate,
            newUser.Active
        );

        return Ok(responseDto);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Erro ao registrar usuário: {ex.Message}");

        return StatusCode(500, "Ocorreu um erro inesperado ao processar sua solicitação.");
      }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userModel)
    {
      if (!Utils.String.EmailIsValid(userModel.Email))
        ModelState.AddModelError(nameof(userModel.Email), "Email inválido.");

      if (!Utils.Security.PasswordIsValid(userModel.Password))
        ModelState.AddModelError(nameof(userModel.Password), "Senha inválida.");

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        var user = await _usersRepository.GetUserByEmailAsync(userModel.Email);

        if (user == null || !Utils.Security.VerifyHashedPassword(userModel.Password, user.PasswordHash))
        {
          return Unauthorized("Email ou senha incorretos.");
        }

        var token = _tokenGenerator.GenerateToken(user);

        return Ok(new
        {
          Token = token,
          User = new
          {
            user.Id,
            user.Name,
            user.Email
          }
        });
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Erro no login: {ex.Message}");
        return StatusCode(500, "Ocorreu um erro interno ao tentar efetuar o login.");
      }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserAsync(Guid id, UserRegisterDto model)
    {
      if (!Utils.String.InputIsValid(model.Name))
        ModelState.AddModelError(nameof(model.Name), "Nome inválido.");

      if (!Utils.String.EmailIsValid(model.Email))
        ModelState.AddModelError(nameof(model.Email), "Email inválido.");

      if (!Utils.Security.PasswordIsValid(model.Password))
        ModelState.AddModelError(nameof(model.Password), "A senha não atende aos requisitos.");

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        var user = await _usersRepository.GetUserAsync(id);
        if (user is null)
        {
          return NotFound();
        }

        user.Name = model.Name;
        user.Email = model.Email;
        user.PasswordHash = Utils.Security.HashPassword(model.Password);

        await _usersRepository.UpdateUserAsync(user);

        return NoContent();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Erro ao registrar usuário: {ex.Message}");

        return StatusCode(500, "Ocorreu um erro inesperado ao processar sua solicitação.");
      }
    }

    [Authorize(Roles = "Admin")]
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
