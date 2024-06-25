using EBrief.Data;
using EBrief.Helpers;
using EBrief.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using Serilog;
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
            serviceCollection.AddLogging();
#else

            serviceCollection.AddLogging(builder =>
            {
                var loggerConfiguration = new LoggerConfiguration()
                    .WriteTo.File("test.txt", rollingInterval: RollingInterval.Day)
                    .WriteTo.Console()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning);

                builder.AddSerilog(loggerConfiguration.CreateLogger());
            });
#endif

            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            {
                string dbPath = Path.Combine(FileHelpers.AppDataPath, "EBrief.db");
                options.UseSqlite($"Filename={dbPath}");
            });
            serviceCollection.AddScoped<IDataAccess, CourtListDataAccess>();
            serviceCollection.AddScoped<TooltipService>();
            serviceCollection.AddScoped<IFileService, FileService>();
            serviceCollection.AddSingleton(new AppState());

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