using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PgManager.Models
{
    public class TableColumnInfo
    {
        [JsonPropertyName("columnName")]
        public string ColumnName { get; set; } = default!;
        [JsonPropertyName("dataType")]
        public string DataType { get; set; } = default!;
        [JsonPropertyName("isNullable")]
        public bool IsNullable { get; set; }
        [JsonPropertyName("defaultValue")]
        public string? DefaultValue { get; set; }
    }
}
