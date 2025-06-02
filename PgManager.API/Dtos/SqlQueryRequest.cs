namespace PgManager.API.Dtos
{
    public class SqlQueryRequest : DbConnectionConfigDto
    {
        public string Sql { get; set; } = default!;
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 50;
    }
}
