using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Cameshop.Repositories
{
  public class ApiKeyMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
      _next = next;
      _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
      if (!context.Request.Headers.TryGetValue("ApiKey", out var apiKey))
      {
        context.Response.StatusCode = 401; // Unauthorized
        await context.Response.WriteAsync("API Key is missing.");
        return;
      }

      var validApiKey = _configuration.GetValue<string>("ApiKeys:AllowedKey");
      if (apiKey != validApiKey)
      {
        context.Response.StatusCode = 403; // Forbidden
        await context.Response.WriteAsync("Invalid API Key.");
        return;
      }

      await _next(context);
    }
  }
}