

using CommunityToolkit.Mvvm.ComponentModel;

using PgManager.Windows;


namespace PgManager.ViewModels
{
    public partial class BaseViewModel : ObservableObject, IWindowCloser
    {
        public BaseWindow Window { get; set; }

        public void CloseWindow()
        {
            Window?.Close();
        }
    }
}
