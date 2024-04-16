using EBrief.Shared.Helpers;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace EBrief;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Directory.CreateDirectory(Path.Combine(FileHelpers.AppDataPath));
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            MessageBox.Show(error.ExceptionObject.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };
    }
}

