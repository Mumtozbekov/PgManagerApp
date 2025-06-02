using System.ComponentModel;

namespace PgManager.API.Dtos;


public class DbConnectionConfigDto : PgConnectionConfigDto
{
    public static DbConnectionConfigDto Create(PgConnectionConfigDto pgConnection)
    {
        return new()
        {
            Host = pgConnection.Host,
            Port = pgConnection.Port,
            Username = pgConnection.Username,
            Password = pgConnection.Password,
        };
    }

    [DefaultValue("test_db")]
    public string Database { get; set; }
}
