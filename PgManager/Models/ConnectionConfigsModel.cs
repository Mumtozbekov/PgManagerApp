using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace PgManager.Models
{
    public partial class ConnectionConfigsModel : ObservableObject
    {
        [ObservableProperty]
        private string apiHost;
        [ObservableProperty]
        private int apiPort;
        [ObservableProperty]
        private string dbHost;
        [ObservableProperty]
        private int dbPort;
        [ObservableProperty]
        private string dbUser;
        [ObservableProperty]
        private string dbPassword;

        public string ApiUrl => $"{ApiHost}:{apiPort}/";
    }
}
