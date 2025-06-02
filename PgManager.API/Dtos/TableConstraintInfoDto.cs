
public class TableConstraintInfoDto
{
    public string TableName { get; set; } = default!;
    public string ConstraintName { get; set; } = default!;
    public string ConstraintType { get; set; } = default!;
    public string ColumnName { get; set; } = default!;
    public string? ForeignTable { get; set; }
    public string? ForeignColumn { get; set; }
}
