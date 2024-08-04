using EBrief.Data;
using EBrief.Shared.Data;
using EBrief.Shared.Exceptions;
using EBrief.Shared.Helpers;
using EBrief.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.ComponentModel;
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
                    .WriteTo.Console()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning);

                builder.AddSerilog(loggerConfiguration.CreateLogger());
            });

            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            {
                string dbPath = Path.Combine(FileHelpers.AppDataPath, "EBrief.db");
                options.UseSqlite($"Filename={dbPath}");
                options.EnableSensitiveDataLogging();
            });
            serviceCollection.AddScoped<IDataAccess, CourtListDataAccess>();
            serviceCollection.AddScoped<IFileService, FileService>();
            serviceCollection.AddSingleton(new AppState());

            Resources.Add("services", serviceCollection.BuildServiceProvider());

            WindowState = WindowState.Maximized;
            Closing += OnWindowClosing;
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

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        var serviceProvider = (IServiceProvider)Resources["services"];
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var appState = serviceProvider.GetRequiredService<AppState>();
        if (appState == null || dbContext == null)
        {
            return;
        }

        if (appState.CurrentCourtList == null)
        {
            return;
        }

        foreach (var caseFile in appState.CurrentCourtList.GetCaseFiles())
        {
            if (caseFile.Notes.HasChanged)
            {
                var button = MessageBoxButton.YesNoCancel;
                var result = MessageBox.Show($"You have unsaved changes to your court list. Save before closing?", "Save changes", button, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                break;
            }
        }

        var courtListModel = dbContext.CourtLists
            .Where(cl => cl.Id == appState.CurrentCourtList.Id)
            .Include(cl => cl.CaseFiles)
            .First();

        foreach (var caseFile in appState.CurrentCourtList.GetCaseFiles())
        {
            courtListModel.CaseFiles.First(cf => cf.CaseFileNumber == caseFile.CaseFileNumber).Notes = caseFile.Notes.Text;
        }

        dbContext.CourtLists.Update(courtListModel);
        dbContext.SaveChanges();
    }
}