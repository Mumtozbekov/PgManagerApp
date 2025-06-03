using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace PgManager.Models
{
    public partial class DbInfoTreeNode : ObservableObject
    {
        //public DbInfoTreeNode(string name, string key = "")
        //{
        //    Name = name;
        //    Key = key;

        //}
        [JsonPropertyName("key")]
        public string Key { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("dbName")]
        public string DbName { get; set; }

        [JsonPropertyName("children")]
        public ObservableCollection<DbInfoTreeNode> Children { get; set; }

        [JsonPropertyName("columnInfo")]
        public TableColumnInfo ColumnInfo { get; set; }

        [ObservableProperty]
        private bool isLoading;
    }


}
