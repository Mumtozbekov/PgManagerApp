using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PgManager.Models;

namespace PgManager.Converters
{
    public class QueryResultConverter
    {
        public static DataTable ConvertToDataTable(SqlQueryResult result)
        {
            var table = new DataTable();

            foreach (var col in result.Columns)
                table.Columns.Add(col);

            foreach (var row in result.Rows)
            {
                var dataRow = table.NewRow();
                foreach (var col in result.Columns)
                {
                    dataRow[col] = row.ContainsKey(col) ? row[col] ?? DBNull.Value : DBNull.Value;
                }
                table.Rows.Add(dataRow);
            }

            return table;
        }
    }
}
