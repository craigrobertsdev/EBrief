using EBrief.Data;
using EBrief.Shared.Data;
using EBrief.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using System.Windows;

namespace EBrief;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        try
        {
            Log.Information("Application starting up");

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
#if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
#endif
            serviceCollection.AddLogging(builder =>
            {
                var loggerConfiguration = new LoggerConfiguration()
                    .WriteTo.File("test.txt", rollingInterval: RollingInterval.Day)
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning);

                builder.AddSerilog(loggerConfiguration.CreateLogger());
            });

            serviceCollection.AddDbContext<ApplicationDbContext>(builder =>
            {
                string dbPath = Path.Combine(FileHelpers.AppDataPath, "EBrief.db");
                builder.UseSqlite($"Filename={dbPath}");
            });
            serviceCollection.AddScoped<IDataAccess, CourtListDataAccess>();

            Resources.Add("services", serviceCollection.BuildServiceProvider());

            WindowState = WindowState.Maximized;

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
}