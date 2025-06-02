public class TableColumnInfoDto
{
    public string ColumnName { get; set; } = default!;
    public string DataType { get; set; } = default!;
    public bool IsNullable { get; set; }
    public string? DefaultValue { get; set; }
}