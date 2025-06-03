using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PgManager.Models;
using PgManager.Services;
using PgManager.Windows;

namespace PgManager.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private ApiService _apiService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private bool isConnected;
        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string apiHost;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private int apiPort;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string dbHost;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private int dbPort;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string dbUser;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string dbPassword;


        public SettingsViewModel(ApiService apiService)
        {


            ApiHost = string.IsNullOrEmpty(Settings.Default.BaseUrl) ? "https://localhost" : Settings.Default.BaseUrl;
            ApiPort = Settings.Default.ApiPort > 0 ? Settings.Default.ApiPort : 5044;

            DbHost = string.IsNullOrEmpty(Settings.Default.DbHost) ? "https://localhost" : Settings.Default.DbHost;
            DbPort = Settings.Default.DbPort > 0 ? Settings.Default.DbPort : 5432;
            DbUser = string.IsNullOrEmpty(Settings.Default.DbUser) ? "postgres" : Settings.Default.DbUser;

            DbPassword = Global.DbPassword;


            _apiService = apiService;
        }


        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            Settings.Default.BaseUrl = ApiHost;
            Settings.Default.ApiPort = ApiPort;
            Settings.Default.DbHost = DbHost;
            Settings.Default.DbPort = DbPort;
            Settings.Default.DbUser = DbUser;

            Global.DbPassword = DbPassword;
            Settings.Default.Save();

            if (App.Current.MainWindow is MainWindow)
            {
                var vm = App.GetRequiredService<MainViewModel>();
                vm.Refresh();

                CloseWindow();
            }
        }

        private bool CanSave()
        {
            return IsConnected || !(string.IsNullOrWhiteSpace(ApiHost)
                || string.IsNullOrWhiteSpace(DbHost)
                || string.IsNullOrWhiteSpace(DbUser)
                || string.IsNullOrWhiteSpace(DbPassword));
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseWindow();
        }
        [RelayCommand]
        private async Task CheckConnection()
        {
            IsConnected = false;
            IsBusy = true;
            var config = new ConnectionConfigsModel()
            {
                ApiHost = this.ApiHost,
                ApiPort = this.ApiPort,

                DbHost = this.DbHost,
                DbPort = this.DbPort,
                DbUser = this.DbUser,

                DbPassword = this.DbPassword
            };
            _apiService.CreateHttpClient(config);
            IsConnected = await _apiService.CheckConnection(config);


            IsBusy = false;

            if (IsConnected && App.Current.MainWindow is not MainWindow)
            {
                Save();
                BaseWindow.ShowWindow<MainWindow>();
                CloseWindow();
            }
        }

    }
}
