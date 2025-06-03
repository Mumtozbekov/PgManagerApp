using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using PgManager.Constants;
using PgManager.Models;

namespace PgManager.ViewModels
{
    public partial class TablePropertiesViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<TableColumnInfo> columns;

        [ObservableProperty]
        private ObservableCollection<DbInfoTreeNode> constraints;

        [ObservableProperty]
        private DbInfoTreeNode info;

        public TablePropertiesViewModel()
        {
        }

        public void LoadNode(DbInfoTreeNode node)
        {
            Info = node;

            Columns = new();
            Constraints = new();

            foreach (DbInfoTreeNode child in node.Children)
            {
                if (child.Key == DbTreeNodeKeys.Column)
                    foreach(DbInfoTreeNode info in child.Children)
                        Columns.Add(info.ColumnInfo);
                else if (child.Key == DbTreeNodeKeys.Constraints)
                    foreach (DbInfoTreeNode info in child.Children)
                        Constraints.Add(info);

            }
        }
    }
}
