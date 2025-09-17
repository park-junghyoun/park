using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CellManager.Services;
using CellManager.ViewModels;

namespace CellManager
{
    /// <summary>
    ///     Application bootstrapper that wires up the dependency injection container
    ///     and ensures the SQLite database is ready before showing the main window.
    /// </summary>
    public partial class App : Application
    {
        public static IHost HostRef { get; private set; } = default!;

        public App()
        {
            HostRef = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    // Repositories
                    services.AddSingleton<ICellRepository, SQLiteCellRepository>();

                    // ViewModels
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<HomeViewModel>();
                    services.AddTransient<CellLibraryViewModel>();
                    services.AddTransient<TestSetupViewModel>();
                    services.AddTransient<ScheduleViewModel>();
                    services.AddTransient<RunViewModel>();
                    services.AddTransient<AnalysisViewModel>();
                    services.AddTransient<DataExportViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<HelpViewModel>();

                    services.AddSingleton<IChargeProfileRepository, SQLiteChargeProfileRepository>();
                    services.AddSingleton<IDischargeProfileRepository, SQLiteDischargeProfileRepository>();
                    services.AddSingleton<IEcmPulseProfileRepository, SQLiteEcmPulseProfileRepository>();
                    services.AddSingleton<IOcvProfileRepository, SQLiteOcvProfileRepository>();
                    services.AddSingleton<IRestProfileRepository, SQLiteRestProfileRepository>();
                    services.AddSingleton<IScheduleRepository, SQLiteScheduleRepository>();


                    // Views
                    services.AddSingleton<MainWindow>();

                    // Services
                    services.AddSingleton<IServerStatusService, MySqlServerStatusService>();
                })
                .Build();
        }

        /// <summary>
        ///     Creates the main window once the database schema has been verified.
        /// </summary>
        /// <param name="e">Default WPF startup arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Ensure DB schema exists
            using (var scope = HostRef.Services.CreateScope())
            {
                // 생성자 실행 → InitializeDatabase() 보장
                _ = scope.ServiceProvider.GetRequiredService<ICellRepository>();
            }

            var main = HostRef.Services.GetRequiredService<MainWindow>();
            main.DataContext = HostRef.Services.GetRequiredService<MainViewModel>();
            main.Show();

            base.OnStartup(e);
        }

        /// <summary>
        ///     Gracefully stops the host so that singletons and scoped services can dispose correctly.
        /// </summary>
        /// <param name="e">Default WPF exit arguments.</param>
        protected override async void OnExit(ExitEventArgs e)
        {
            await HostRef.StopAsync();
            HostRef.Dispose();
            base.OnExit(e);
        }
    }
}
