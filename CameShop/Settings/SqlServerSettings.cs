namespace Cameshop.Repositories
{
  public class SqlServerSettings
  {
    public string Host { get; set; }
    public int Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }

    public string ConnectionString
    {
      get
      {
        return $"sqlserver://{User}:{Password}@{Host}:{Port}";
      }
    }

  }
}