using System.Data;
using System.IO;
using System.Text;
namespace PgManager.Helpers
{
    public static class CSVHelper
    {

        public static void ExportDataTableToCsv(DataTable table, string filePath, char separator = ',')
        {
            var sb = new StringBuilder();

            var columnNames = table.Columns.Cast<DataColumn>()
                                           .Select(c => EscapeCsv(c.ColumnName, separator));
            sb.AppendLine(string.Join(separator, columnNames));

            foreach (DataRow row in table.Rows)
            {
                var fields = row.ItemArray.Select(field => EscapeCsv(field?.ToString(), separator));
                sb.AppendLine(string.Join(separator, fields));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string EscapeCsv(string? field, char separator)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            field = field.Replace("\"", "\"\""); // escape double quotes

            if (field.Contains(separator) || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
                return $"\"{field}\"";

            return field;
        }
    

}
}
