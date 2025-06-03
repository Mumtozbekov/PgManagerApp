using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using PgManager.ViewModels;
using PgManager.Windows;

using Wpf.Ui.Controls;

namespace PgManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(MainViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.Window = this;
        }

  

        private void dbProperties_Click(object sender, RoutedEventArgs e)
        {
            //aslida bu command bilan qilishi kerak edi
            if (sender is Wpf.Ui.Controls.MenuItem mi && DataContext is MainViewModel vm)
            {
                vm.ShowDbNodeProperties(mi.DataContext);
            }
        }
    }
}