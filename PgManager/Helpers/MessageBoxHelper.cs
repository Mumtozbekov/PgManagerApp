using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgManager.Helpers
{
    internal class MessageBoxHelper
    {
        public static async void Show(string message)
        {
            var uiMessageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Xatolik yuz berdi!",
                Content = message
            };

            _ = await uiMessageBox.ShowDialogAsync();
        }
    }
}
