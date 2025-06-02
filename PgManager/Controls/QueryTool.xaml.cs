using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using CommunityToolkit.Mvvm.Input;

using PgManager.Models;

using Wpf.Ui.Controls;

namespace PgManager.Controls
{
    /// <summary>
    /// Логика взаимодействия для QueryTool.xaml
    /// </summary>
    public partial class QueryTool : UserControl
    {

        public ICommand CopyCommand { get; set; }
        public ICommand CopyToEditorCommand { get; set; }
        public RelayCommand<string> RunQueryCommand
        {
            get { return (RelayCommand<string>)GetValue(RunQueryCommandProperty); }
            set { SetValue(RunQueryCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RunQueryCommandProperty =
            DependencyProperty.Register("RunQueryCommand", typeof(RelayCommand<string>), typeof(QueryTool), new PropertyMetadata(null));

        public ObservableCollection<QueryHistoryModel> History { get; set; }
        public QueryTool()
        {
            InitializeComponent();

            History = new();

            CopyCommand = new RelayCommand(CopyHistory);
            CopyToEditorCommand = new RelayCommand(CopyHistoryToEditor);
        }

        private void CopyHistoryToEditor()
        {
            if (ltbHistory.SelectedItem is QueryHistoryModel history)
            {
                var paragraph = new Paragraph(new Run(history.Query));
                var doc = new FlowDocument();

                doc.Blocks.Add(paragraph);

                editor.Document = doc;
            }
        }

        private void CopyHistory()
        {
            if (ltbHistory.SelectedItem is QueryHistoryModel history)
                Clipboard.SetText(history.Query);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ICollectionView view = CollectionViewSource.GetDefaultView(History);

            view.SortDescriptions.Add(new SortDescription("DateTime", ListSortDirection.Descending));
        }

        private void RunQuery(object sender, RoutedEventArgs e)
        {
            var query = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd).Text;

            if (string.IsNullOrEmpty(query) || string.IsNullOrWhiteSpace(query))
                return;

            query = query.Trim('\n').Trim('\r').Trim(';');
            History.Add(new(query));

            RunQueryCommand?.Execute(query);
        }

        private void editor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                RunQuery(sender, e);
            }
        }

        private void ltbHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListBox;
            if (list != null && list.SelectedItem is QueryHistoryModel history)
            {
                var paragraph = new Paragraph(new Run(history.Query));
                var doc = new FlowDocument();

                doc.Blocks.Add(paragraph);

                rtbHistory.Document = doc;
            }
        }
    }
}
