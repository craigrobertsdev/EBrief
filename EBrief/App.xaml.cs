using EBrief.Data;
using EBrief.Shared.Data;
using EBrief.Shared.Helpers;
using EBrief.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;
using System.Windows;

namespace EBrief;
public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddWpfBlazorWebView();

#if DEBUG
                    services.AddBlazorWebViewDeveloperTools();
#endif

                    services.AddLogging(builder =>
                    {
                        var loggerConfiguration = new LoggerConfiguration()
                            .WriteTo.File("test.txt", rollingInterval: RollingInterval.Day)
                            .WriteTo.Console()
                            .MinimumLevel.Information()
                            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning);
                        builder.AddSerilog(loggerConfiguration.CreateLogger());
                    });

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        string dbPath = Path.Combine(FileHelpers.AppDataPath, "EBrief.db");
                        options.UseSqlite($"Filename={dbPath}");
                        options.EnableSensitiveDataLogging();
                    });

                    services.AddTransient<IDataAccess, DataAccess>();
                    services.AddTransient<IFileService, FileService>();

                    services.AddSingleton<IDataAccessFactory, DataAccessFactory>();
                    services.AddSingleton<IFileServiceFactory, FileServiceFactory>();


                    services.AddSingleton(new AppState());
                    services.AddHttpClient();
                    services.AddTransient<HttpService>();
                })
                .Build();

            RegisterErrorHandling();

            await host.StartAsync();

            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            mainWindow.Resources.Add("services", host.Services);
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "The application failed to start correctly.");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void RegisterErrorHandling()
    {
        Directory.CreateDirectory(Path.Combine(FileHelpers.AppDataPath));
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            MessageBox.Show(error.ExceptionObject.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };
    }
}

