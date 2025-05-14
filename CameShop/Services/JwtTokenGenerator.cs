using Cameshop.Entities;
using Cameshop.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cameshop.Services
{
  public class JwtTokenGenerator
  {
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
    {
      _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(User user)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new[]
          {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
        Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
        Issuer = _jwtSettings.Issuer,
        Audience = _jwtSettings.Audience,
        SigningCredentials = null
      };

      var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
      token.Header.Remove("alg");
      token.Header.Add("alg", "none");

      return tokenHandler.WriteToken(token);
    }
  }
}
