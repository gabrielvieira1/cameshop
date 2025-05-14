using Cameshop.Dtos;
using Cameshop.Entities;
using Cameshop.Errors;
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
      try
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
          PasswordHash = model.Password,
          CreatedDate = DateTimeOffset.UtcNow,
          Active = true,
          Role = "Cliente"
        };

        await _usersRepository.CreateUserAsync(newUser);

        return Ok(newUser.AsDto());
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Erro inesperado: {ex.Message}\nStackTrace: {ex.StackTrace}");
      }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userModel)
    {
      try
      {
        var user = await _usersRepository.GetUserByEmailAsync(userModel.Email);

        if (user == null)
          return NotFound("Usuário não encontrado.");

        if (userModel.Password != user.PasswordHash)
          return Unauthorized("Senha incorreta.");

        var token = _tokenGenerator.GenerateToken(user);

        logger.LogInformation($"Performing user login: {userModel.Email} with password: {userModel.Password}");

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
        return StatusCode(500, $"Erro inesperado: {ex.Message}\nStackTrace: {ex.StackTrace}");
      }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserAsync(Guid id, UserRegisterDto model)
    {
      if (!IsValidCredentials(model))
      {
        return BadRequest();
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
        user.PasswordHash = model.Password;

        await _usersRepository.UpdateUserAsync(user);

        return NoContent();
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Erro inesperado: {ex.Message}\nStackTrace: {ex.StackTrace}");
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

    private bool IsValidCredentials(UserRegisterDto model)
    {
      bool isValid = true;

      if (!Utils.String.InputIsValid(model.Name))
      {
        ModelState.AddModelError(nameof(model.Name), DomainErrors.Validation.InvalidName.Message);
        isValid = false;
      }

      if (!Utils.String.EmailIsValid(model.Email))
      {
        ModelState.AddModelError(nameof(model.Email), DomainErrors.Validation.InvalidEmail.Message);
        isValid = false;
      }

      if (!Utils.Security.PasswordIsValid(model.Password))
      {
        ModelState.AddModelError(nameof(model.Password), DomainErrors.Validation.InvalidPassword.Message);
        isValid = false;
      }

      return isValid;
    }

    private bool IsValidLogin(UserLoginDto model)
    {
      bool isValid = true;

      if (!Utils.String.EmailIsValid(model.Email))
      {
        ModelState.AddModelError(nameof(model.Email), DomainErrors.Validation.InvalidEmail.Message);
        isValid = false;
      }

      if (!Utils.Security.PasswordIsValid(model.Password))
      {
        ModelState.AddModelError(nameof(model.Password), DomainErrors.Validation.InvalidPassword.Message);
        isValid = false;
      }

      return isValid;
    }
  }
}
