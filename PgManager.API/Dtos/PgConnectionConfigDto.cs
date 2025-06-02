using System.ComponentModel;

namespace PgManager.API.Dtos;

public class PgConnectionConfigDto 
{
    [DefaultValue("localhost")]
    public string Host { get; set; }
    [DefaultValue(5432)]
    public int Port { get; set; }
    [DefaultValue("postgres")]
    public string Username { get; set; }
    [DefaultValue("1111")]
    public string Password { get; set; }
}
