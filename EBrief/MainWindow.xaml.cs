using EBrief.Data;
using EBrief.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;

namespace EBrief;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        WindowState = WindowState.Maximized;
        Closing += OnWindowClosing;
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

        foreach (var casefile in appState.CurrentCourtList.GetCasefiles())
        {
            if (casefile.Notes.HasChanged)
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
            .Include(cl => cl.Casefiles)
            .First();

        foreach (var casefile in appState.CurrentCourtList.GetCasefiles())
        {
            courtListModel.Casefiles.First(cf => cf.CasefileNumber == casefile.CasefileNumber).Notes = casefile.Notes.Text;
        }

        dbContext.CourtLists.Update(courtListModel);
        dbContext.SaveChanges();
    }
}