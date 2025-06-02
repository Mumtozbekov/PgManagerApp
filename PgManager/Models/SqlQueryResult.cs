using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PgManager.Models
{
    public class SqlQueryResult
    {
        [JsonPropertyName("columns")]
        public List<string> Columns { get; set; } = new();
        [JsonPropertyName("rows")]
        public List<Dictionary<string, object>> Rows { get; set; } = new();
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
        [JsonPropertyName("errors")]
        public string? Error { get; set; }
    }
}
