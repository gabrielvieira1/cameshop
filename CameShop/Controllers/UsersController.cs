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
using System.Security.Claims;
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
    private Guid GetAuthenticatedUserId()
    {
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [Authorize(Roles = "Admin,Cliente")]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUserAsync(Guid id)
    {
      var authenticatedUserId = GetAuthenticatedUserId();

      if (authenticatedUserId == Guid.Empty || authenticatedUserId != id)
      {
        logger.LogWarning("Erro {Code}: Acesso negado ao usuário com ID: {RequestedId}",
            DomainErrors.User.Unauthorized.Code,
            id);

        return Unauthorized();
      }

      var user = await _usersRepository.GetUserAsync(id);
      if (user is null)
      {
        logger.LogWarning("Erro {Code}: {Message} - ID: {UserId}",
            DomainErrors.User.NotFound.Code,
            DomainErrors.User.NotFound.Message,
            id);

        return NotFound(new
        {
          DomainErrors.User.NotFound.Code,
          DomainErrors.User.NotFound.Message
        });
      }

      return Ok(user.AsDto());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IEnumerable<UserResponseDto>> GetUsersAsync()
    {
      var users = await _usersRepository.GetUsersAsync();
      return users.Select(user => user.AsDto());
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto model)
    {
      if (!IsValidCredentials(model))
      {
        logger.LogWarning("Erro {Code}: {Message} ao registrar usuário com e-mail: {Email}",
            DomainErrors.Validation.InvalidCredentials.Code,
            DomainErrors.Validation.InvalidCredentials.Message,
            model.Email);

        return BadRequest(new
        {
          Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage),
          DomainError = new
          {
            DomainErrors.Validation.InvalidCredentials.Code,
            DomainErrors.Validation.InvalidCredentials.Message
          }
        });
      }

      try
      {
        var existingUser = await _usersRepository.GetUserByEmailAsync(model.Email);
        if (existingUser != null)
        {
          return Conflict(new
          {
            DomainErrors.User.EmailInUse.Code,
            DomainErrors.User.EmailInUse.Message
          });
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

        return Ok(newUser.AsDto());
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Erro {Code}: {Message} - E-mail: {Email}",
            DomainErrors.System.ErrorUserRegister.Code,
            DomainErrors.System.ErrorUserRegister.Message,
            model.Email);

        return StatusCode(500, new
        {
          DomainErrors.System.UnexpectedError.Code,
          DomainErrors.System.UnexpectedError.Message
        });
      }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userModel)
    {
      if (!IsValidLogin(userModel))
      {
        logger.LogWarning("Erro {Code}: {Message} no login - E-mail: {Email}",
           DomainErrors.Validation.InvalidCredentials.Code,
           DomainErrors.Validation.InvalidCredentials.Message,
           userModel.Email);

        return BadRequest(new
        {
          Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage),
          DomainError = new
          {
            DomainErrors.Validation.InvalidCredentials.Code,
            DomainErrors.Validation.InvalidCredentials.Message
          }
        });
      }

      try
      {
        var user = await _usersRepository.GetUserByEmailAsync(userModel.Email);

        if (user == null || !Utils.Security.VerifyHashedPassword(userModel.Password, user.PasswordHash))
        {
          logger.LogWarning("Erro {Code}: {Message} - Tentativa de login inválida: {Email}",
              DomainErrors.User.InvalidLogin.Code,
              DomainErrors.User.InvalidLogin.Message,
              userModel.Email);

          return Unauthorized(new
          {
            DomainErrors.User.InvalidLogin.Code,
            DomainErrors.User.InvalidLogin.Message
          });
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
        logger.LogError(ex, "Erro {Code}: {Message} - E-mail: {Email}",
           DomainErrors.System.ErrorLogin.Code,
           DomainErrors.System.ErrorLogin.Message,
           userModel.Email);

        return StatusCode(500, new
        {
          DomainErrors.System.UnexpectedError.Code,
          DomainErrors.System.UnexpectedError.Message
        });
      }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserAsync(Guid id, UserRegisterDto model)
    {
      if (!IsValidCredentials(model))
      {
        logger.LogWarning("Erro {Code}: {Message} ao registrar usuário com e-mail: {Email}",
            DomainErrors.Validation.InvalidCredentials.Code,
            DomainErrors.Validation.InvalidCredentials.Message,
            model.Email);

        return BadRequest(new
        {
          Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage),
          DomainError = new
          {
            DomainErrors.Validation.InvalidCredentials.Code,
            DomainErrors.Validation.InvalidCredentials.Message
          }
        });
      }

      try
      {
        var user = await _usersRepository.GetUserAsync(id);
        if (user is null)
        {
          return NotFound(DomainErrors.User.NotFound);
        }

        user.Name = model.Name;
        user.Email = model.Email;
        user.PasswordHash = Utils.Security.HashPassword(model.Password);

        await _usersRepository.UpdateUserAsync(user);

        return NoContent();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Erro {Code}: {Message} - E-mail: {Email}",
            DomainErrors.System.ErrorUserUpdate.Code,
            DomainErrors.System.ErrorUserUpdate.Message,
            model.Email);

        return StatusCode(500, new
        {
          DomainErrors.System.UnexpectedError.Code,
          DomainErrors.System.UnexpectedError.Message
        });
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

      logger.LogWarning("Erro {Code}: {Message} - ID: {UserId}",
            DomainErrors.System.UserDeleted.Code,
            DomainErrors.System.UserDeleted.Message,
            id);

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
