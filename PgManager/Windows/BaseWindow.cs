using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Wpf.Ui.Controls;

namespace PgManager.Windows
{
    public class BaseWindow : FluentWindow
    {
        public static void ShowWindow<T>() where T : FluentWindow , new()
        {
            var window = App.GetRequiredService<T>();
            App.Current.MainWindow = window;
            App.Current.MainWindow.Show();
        }
    }
}
