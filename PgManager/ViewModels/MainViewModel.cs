using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using PgManager.Constants;
using PgManager.Converters;
using PgManager.Helpers;
using PgManager.Models;
using PgManager.Services;
using PgManager.Windows;

using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace PgManager.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private ApiService apiService;
        private BaseWindow window;

        [ObservableProperty]
        private ObservableCollection<DbInfoTreeNode> dbInfoTree;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand), nameof(PrevPageCommand))]
        int currentPage = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand), nameof(PrevPageCommand))]
        int pageCount;
        [ObservableProperty]
        int limit = 50;

        [ObservableProperty]
        string? selectedDbName;

        [ObservableProperty]
        bool queryExecuting;

        [ObservableProperty]
        bool isLoadingDbs;

        [ObservableProperty]
        SqlQueryResult queryResult;

        [ObservableProperty]
        DataTable dataTable;

        [ObservableProperty]
        ObservableCollection<string> databaseNames;
        public MainViewModel(ApiService apiService)
        {
            this.apiService = apiService;

            DbInfoTree = new();
            DatabaseNames = new();
            LoadDBs();
        }


        public async Task LoadDBs()
        {
            IsLoadingDbs = true;

            var dbNode = await apiService.GetDbTree();
            if (dbNode != null)
                DbInfoTree.Add(dbNode);

            foreach (var child in dbNode!.Children)
            {
                if (child is DbInfoTreeNode && child.Key == DbTreeNodeKeys.Database)
                {
                    DatabaseNames.Add(child.Name);
                }
            }

            IsLoadingDbs = false;
        }
        private string lastQuery;
        [RelayCommand]
        public void RunQuery(string query)
        {
            lastQuery = query;
            SendQuery(query);
        }

        [RelayCommand]
        public async Task Refresh()
        {
            DbInfoTree.Clear();
            DatabaseNames.Clear();
            await LoadDBs();
            SelectedDbName = DatabaseNames.FirstOrDefault();
        }
        public async void SendQuery(string query)
        {
            QueryExecuting = true;
            QueryResult = await apiService.SendQuery(SelectedDbName, query, CurrentPage, Limit);
            if (QueryResult != null)
            {
                if (QueryResult.Error != null)
                {
                    PageCount = CurrentPage = 1;
                    MessageBoxHelper.Show(QueryResult.Error);
                }
                else
                {
                    PageCount = QueryResult.TotalCount / Limit;
                    DataTable = QueryResultConverter.ConvertToDataTable(QueryResult);
                }
            }
            QueryExecuting = false;
        }

        [RelayCommand]
        public void OpenSettings()
        {
            BaseWindow.ShowDialog<SettingsWindow>();
        }

        [RelayCommand(CanExecute = nameof(CanNext))]
        public async void NextPage()
        {
            CurrentPage++;
            SendQuery(lastQuery);
        }

        private bool CanNext()
        {
            return CurrentPage < PageCount;
        }
        [RelayCommand(CanExecute = nameof(CanPrev))]
        public async void PrevPage()
        {
            CurrentPage--;
            SendQuery(lastQuery);
        }

        private bool CanPrev()
        {
            return CurrentPage > 1;
        }

        public void LoadTables(DbInfoTreeNode node)
        {
            if (node == null)
                return;

            foreach (DbInfoTreeNode childNode in node.Children)
            {
                Task.Run(() =>
                {
                    Application.Current.Dispatcher.InvokeAsync(async () =>
                    {

                        if (childNode.Key == DbTreeNodeKeys.Shema && childNode.Children.Count == 0)
                        {
                            try
                            {

                                childNode.IsLoading = true;
                                var tables = await apiService.GetTableTree(node.DbName, childNode.Name);
                                foreach (var table in tables) { childNode.Children.Add(table); }

                            }
                            catch { }
                            childNode.IsLoading = false;
                        }
                    });
                });
            }
        }
        [RelayCommand]
        public void ExportToCsv()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = "data.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                CSVHelper.ExportDataTableToCsv(dataTable, dialog.FileName);
            }

        }


        [RelayCommand]
        public void ShowDbNodeProperties(object param)
        {
            var node = param as DbInfoTreeNode;
            if (node == null) return;

            if (node.Key != DbTreeNodeKeys.Table)
            {
                MessageBoxHelper.Show("Xozircha faqat tablitsalarni ko'ra olamiz!");
                return;
            }

            var propertiesWindow = App.GetRequiredService<TablePropertiesWindow>();
            var propertiesViewModel = App.GetRequiredService<TablePropertiesViewModel>();

            propertiesViewModel.LoadNode(node);
            propertiesWindow.DataContext = propertiesViewModel;
            propertiesWindow.Owner = this.Window;
            propertiesWindow.ShowDialog();

        }
        public async Task HanldeTreviewExpand(RoutedEventArgs e)
        {

            if (e.OriginalSource is System.Windows.Controls.TreeViewItem ti && ti.DataContext is DbInfoTreeNode node)
            {
                switch (node.Key)
                {
                    case DbTreeNodeKeys.Shema:
                        LoadTables(node);
                        break;
                    default:
                        break;
                }
            }
        }


    }
}
