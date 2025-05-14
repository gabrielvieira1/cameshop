using System;

namespace Cameshop.Entities
{
  public class User
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public bool Active { get; set; }
    public string Role { get; set; }
  }

}
