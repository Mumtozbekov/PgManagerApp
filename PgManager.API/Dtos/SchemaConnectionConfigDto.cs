using System.ComponentModel;

namespace PgManager.API.Dtos;


public class SchemaConnectionConfigDto : DbConnectionConfigDto
{
    public static SchemaConnectionConfigDto Create(PgConnectionConfigDto pgConnection)
    {
        return new()
        {
            Host = pgConnection.Host,
            Port = pgConnection.Port,
            Username = pgConnection.Username,
            Password = pgConnection.Password,
        };
    }
    [DefaultValue("public")]
    public string Schema { get; set; }
}
