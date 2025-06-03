
using PgManager.Windows;

namespace PgManager.ViewModels
{
    internal interface IWindowCloser
    {
        public BaseWindow Window { get; set; }
        public void CloseWindow();
    }
}
