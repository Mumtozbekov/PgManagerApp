using System.Configuration;
using System.Data;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using PgManager.Services;
using PgManager.ViewModels;
using PgManager.Windows;

namespace PgManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ServiceProvider serviceProvider;



        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        
        }
        void ShowWindow()
        {
            if (string.IsNullOrEmpty(Settings.Default.BaseUrl) || Settings.Default.ApiPort ==0 || string.IsNullOrEmpty(Global.DbPassword))
                BaseWindow.ShowWindow<SettingsWindow>();
            else
                BaseWindow.ShowWindow<MainWindow>();
        }

        private void ConfigureServices(ServiceCollection services)
        {

            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<TablePropertiesViewModel>();
            services.AddTransient<MainViewModel>();

            services.AddTransient<SettingsWindow>();
            services.AddTransient<TablePropertiesWindow>();
            services.AddTransient<MainWindow>();

            services.AddSingleton<ApiService>();
        }

        public static T GetRequiredService<T>() where T : class
        {
            return serviceProvider.GetRequiredService<T>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            ShowWindow();
        }
    }

}
