namespace PgManager.API.Dtos
{
  
    public class SqlQueryResult
    {
        public List<string> Columns { get; set; } = new();
        public List<Dictionary<string, object>> Rows { get; set; } = new();
        public int TotalCount { get; set; }
        public string? Error { get; set; }
    }

}
