﻿using System.Text;
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
        }

        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();
        //    if (DataContext is MainViewModel viewModel)
        //    {
        //        viewModel.LoadDBs();
        //    }
        //}


        
        //public bool Navigate(Type pageType)
        //{

        //    try
        //    {

        //        return MainNavigation.Navigate(pageType);
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //}
    }
}