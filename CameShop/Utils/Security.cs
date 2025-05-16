using System.Security.Claims;
using System;

namespace Cameshop.Utils
{
  public class Security
  {
    public static string HashPassword(string password)
    {
      return BCrypt.Net.BCrypt.EnhancedHashPassword(password, 14);
    }
    public static bool VerifyHashedPassword(string password, string hashedPassword)
    {
      return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    }
    public static bool PasswordIsValid(string passWord)
    {
      int validConditions = 0;

      if (string.IsNullOrEmpty(passWord)) return false;
      if (passWord.Length < 8 || passWord.Length >= 100) return false;


      foreach (char c in passWord)
      {
        if (c >= 'a' && c <= 'z')
        {
          validConditions++;
          break;
        }
      }
      foreach (char c in passWord)
      {
        if (c >= 'A' && c <= 'Z')
        {
          validConditions++;
          break;
        }
      }
      if (validConditions == 0) return false;
      foreach (char c in passWord)
      {
        if (c >= '0' && c <= '9')
        {
          validConditions++;
          break;
        }
      }
      if (validConditions == 1) return false;
      if (validConditions == 2)
      {
        char[] special = { '@', '#', '$', '%', '^', '&', '+', '=' }; // or whatever    
        if (passWord.IndexOfAny(special) == -1) return false;
      }
      return true;
    }
    public static Guid GetAuthenticatedUserId(ClaimsPrincipal user)
    {
      var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      return Guid.TryParse(userIdClaim, out var userId)
          ? userId
          : Guid.Empty;
    }
  }
}
