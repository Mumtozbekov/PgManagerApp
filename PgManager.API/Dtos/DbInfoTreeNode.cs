namespace PgManager.API.Dtos
{
    public class DbInfoTreeNode
    {
        public DbInfoTreeNode(string name, string key = "")
        {
            Name = name;
            Key = key;
            Children = new();
        }
        public string Key { get; set; }
        public string Name { get; set; }
        public string DbName { get; set; }
        public TableColumnInfoDto ColumnInfo { get; set; }
        public List<DbInfoTreeNode> Children { get; set; }
    }


}
