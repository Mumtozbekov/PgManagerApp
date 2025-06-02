using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgManager.Models
{
    public class QueryHistoryModel
    {
        public QueryHistoryModel(string query)
        {
            Query = query;
            DateTime = DateTime.Now;
        }
        public DateTime DateTime { get; set; }
        public string Query { get; set; }
    }
}
